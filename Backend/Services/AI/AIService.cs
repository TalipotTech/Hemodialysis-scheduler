using HDScheduler.API.DTOs;
using HDScheduler.API.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HDScheduler.API.Services.AI
{
    public interface IAIService
    {
        Task<AIScheduleRecommendation> GetSchedulingRecommendationAsync(AIScheduleRecommendationRequest request, int userId);
        Task<AISettingsDto> GetSettingsAsync();
        Task<AISettingsDto> UpdateSettingsAsync(UpdateAISettingsDto dto, int userId);
        Task<AIUsageStatsDto> GetUsageStatsAsync();
        Task<bool> IsAIEnabledAsync();
        Task<bool> CheckCostLimitAsync();
    }
    
    public class AIService : IAIService
    {
        private readonly IAIRepository _repository;
        private readonly IGeminiClient _geminiClient;
        private readonly ILogger<AIService> _logger;
        private const decimal GEMINI_PRO_INPUT_COST_PER_1K_CHARS = 0.0005m;
        private const decimal GEMINI_PRO_OUTPUT_COST_PER_1K_CHARS = 0.0015m;
        private const string ENCRYPTION_KEY = "HDScheduler2025AiKey32Bytes!@"; // Use Azure Key Vault in production
        
        public AIService(
            IAIRepository repository,
            IGeminiClient geminiClient,
            ILogger<AIService> logger)
        {
            _repository = repository;
            _geminiClient = geminiClient;
            _logger = logger;
        }
        
        public async Task<bool> IsAIEnabledAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return settings.AIEnabled && !string.IsNullOrEmpty(settings.EncryptedApiKey);
        }
        
        public async Task<bool> CheckCostLimitAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            await ResetCountersIfNeededAsync(settings);
            
            if (settings.CurrentDailyCost >= settings.DailyCostLimit)
            {
                _logger.LogWarning("Daily AI cost limit reached: ${Cost}", settings.CurrentDailyCost);
                return false;
            }
            
            if (settings.CurrentMonthlyCost >= settings.MonthlyCostLimit)
            {
                _logger.LogWarning("Monthly AI cost limit reached: ${Cost}", settings.CurrentMonthlyCost);
                return false;
            }
            
            return true;
        }
        
        public async Task<AIScheduleRecommendation> GetSchedulingRecommendationAsync(
            AIScheduleRecommendationRequest request, int userId)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!await IsAIEnabledAsync())
                    throw new InvalidOperationException("AI features are not enabled");
                
                if (!await CheckCostLimitAsync())
                    throw new InvalidOperationException("AI cost limit reached");
                
                var settings = await GetOrCreateSettingsAsync();
                var patient = await _repository.GetPatientAsync(request.PatientId)
                    ?? throw new InvalidOperationException("Patient not found");
                
                var targetDate = request.PreferredDate ?? DateTime.Today.AddDays(1);
                var availableSlots = await _repository.GetAvailableSlotsAsync(targetDate);
                
                var prompt = BuildSchedulingPrompt(patient, availableSlots, request);
                
                var apiKey = DecryptApiKey(settings.EncryptedApiKey!);
                var geminiRequest = new GeminiRequest
                {
                    ApiKey = apiKey,
                    Model = "gemini-pro",
                    Prompt = prompt,
                    Temperature = 0.3,
                    MaxOutputTokens = 1024
                };
                
                var geminiResponse = await _geminiClient.GenerateContentAsync(geminiRequest);
                stopwatch.Stop();
                
                var inputChars = prompt.Length;
                var outputChars = geminiResponse.Text.Length;
                var cost = CalculateCost(inputChars, outputChars);
                
                var recommendation = ParseGeminiResponse(geminiResponse.Text, availableSlots, targetDate);
                recommendation.ModelUsed = "Gemini Pro";
                recommendation.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;
                recommendation.Cost = cost;
                
                await LogUsageAsync(new AIUsageLog
                {
                    Provider = "Gemini",
                    RequestType = "Scheduling",
                    InputTokens = inputChars / 4,
                    OutputTokens = outputChars / 4,
                    TotalTokens = (inputChars + outputChars) / 4,
                    Cost = cost,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Success = true,
                    UserId = userId
                });
                
                return recommendation;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error generating AI scheduling recommendation");
                
                await LogUsageAsync(new AIUsageLog
                {
                    Provider = "Gemini",
                    RequestType = "Scheduling",
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Success = false,
                    ErrorMessage = ex.Message,
                    UserId = userId
                });
                
                throw;
            }
        }
        
        private string BuildSchedulingPrompt(Patient patient, List<SlotAvailability> availableSlots, AIScheduleRecommendationRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a hemodialysis scheduling AI. Analyze and recommend optimal schedule.");
            sb.AppendLine();
            sb.AppendLine("**Patient Profile:**");
            sb.AppendLine($"- Age: {patient.Age} years");
            sb.AppendLine($"- Weight: {patient.DryWeight} kg");
            sb.AppendLine($"- Dialyser: {patient.DialyserType}");
            sb.AppendLine($"- HD Cycle: {patient.HDCycle}");
            sb.AppendLine();
            sb.AppendLine("**Available Slots:**");
            foreach (var slot in availableSlots)
            {
                sb.AppendLine($"- Slot {slot.SlotId}: {slot.TimeDescription}, {slot.AvailableBeds} beds available");
            }
            sb.AppendLine();
            if (request.PreferredSlotId.HasValue)
            {
                sb.AppendLine($"**Preference:** Slot {request.PreferredSlotId}");
                sb.AppendLine();
            }
            sb.AppendLine("**Output JSON format:**");
            sb.AppendLine("{\"recommendedSlotId\": <number>, \"recommendedBedNumber\": <1-10>, \"confidence\": <0.0-1.0>, \"reasoning\": \"<text>\", \"factors\": [\"<list>\"]}");
            
            return sb.ToString();
        }
        
        private AIScheduleRecommendation ParseGeminiResponse(string responseText, List<SlotAvailability> availableSlots, DateTime date)
        {
            try
            {
                var jsonStart = responseText.IndexOf('{');
                var jsonEnd = responseText.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
                    
                    return new AIScheduleRecommendation
                    {
                        RecommendedSlotId = parsed.GetProperty("recommendedSlotId").GetInt32(),
                        RecommendedBedNumber = parsed.GetProperty("recommendedBedNumber").GetInt32(),
                        RecommendedDate = date,
                        Confidence = parsed.GetProperty("confidence").GetDouble(),
                        Reasoning = parsed.GetProperty("reasoning").GetString() ?? "",
                        Factors = parsed.TryGetProperty("factors", out var factors)
                            ? factors.EnumerateArray().Select(f => f.GetString() ?? "").ToList()
                            : new List<string>()
                    };
                }
                
                // Fallback
                var defaultSlot = availableSlots.OrderByDescending(s => s.AvailableBeds).First();
                return new AIScheduleRecommendation
                {
                    RecommendedSlotId = defaultSlot.SlotId,
                    RecommendedBedNumber = 1,
                    RecommendedDate = date,
                    Confidence = 0.5,
                    Reasoning = "Default recommendation based on availability",
                    Factors = new List<string> { "Bed availability" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response");
                throw new InvalidOperationException("Failed to parse AI response", ex);
            }
        }
        
        private decimal CalculateCost(int inputChars, int outputChars)
        {
            var inputCost = (inputChars / 1000m) * GEMINI_PRO_INPUT_COST_PER_1K_CHARS;
            var outputCost = (outputChars / 1000m) * GEMINI_PRO_OUTPUT_COST_PER_1K_CHARS;
            return Math.Round(inputCost + outputCost, 6);
        }
        
        private async Task LogUsageAsync(AIUsageLog log)
        {
            await _repository.LogUsageAsync(log);
            
            var settings = await GetOrCreateSettingsAsync();
            await ResetCountersIfNeededAsync(settings);
            
            settings.CurrentDailyCost += log.Cost;
            settings.CurrentMonthlyCost += log.Cost;
            settings.TodayRequestCount++;
            settings.MonthRequestCount++;
            
            await _repository.UpdateSettingsAsync(settings);
        }
        
        private async Task<AISettings> GetOrCreateSettingsAsync()
        {
            var settings = await _repository.GetSettingsAsync();
            
            if (settings == null)
            {
                settings = new AISettings
                {
                    LastDailyReset = DateTime.UtcNow.Date,
                    LastMonthlyReset = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                };
                settings.Id = await _repository.CreateSettingsAsync(settings);
            }
            
            return settings;
        }
        
        private async Task ResetCountersIfNeededAsync(AISettings settings)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            
            bool changed = false;
            
            if (settings.LastDailyReset < today)
            {
                settings.CurrentDailyCost = 0;
                settings.TodayRequestCount = 0;
                settings.LastDailyReset = today;
                changed = true;
            }
            
            if (settings.LastMonthlyReset < thisMonth)
            {
                settings.CurrentMonthlyCost = 0;
                settings.MonthRequestCount = 0;
                settings.LastMonthlyReset = thisMonth;
                changed = true;
            }
            
            if (changed)
            {
                await _repository.UpdateSettingsAsync(settings);
            }
        }
        
        public async Task<AISettingsDto> GetSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            await ResetCountersIfNeededAsync(settings);
            
            return new AISettingsDto
            {
                AIEnabled = settings.AIEnabled,
                AIProvider = settings.AIProvider,
                DailyCostLimit = settings.DailyCostLimit,
                MonthlyCostLimit = settings.MonthlyCostLimit,
                EnableSchedulingRecommendations = settings.EnableSchedulingRecommendations,
                EnableNaturalLanguageQueries = settings.EnableNaturalLanguageQueries,
                EnablePredictiveAnalytics = settings.EnablePredictiveAnalytics,
                CurrentDailyCost = settings.CurrentDailyCost,
                CurrentMonthlyCost = settings.CurrentMonthlyCost,
                TodayRequestCount = settings.TodayRequestCount,
                MonthRequestCount = settings.MonthRequestCount,
                HasApiKey = !string.IsNullOrEmpty(settings.EncryptedApiKey)
            };
        }
        
        public async Task<AISettingsDto> UpdateSettingsAsync(UpdateAISettingsDto dto, int userId)
        {
            var settings = await GetOrCreateSettingsAsync();
            
            if (dto.AIEnabled.HasValue) settings.AIEnabled = dto.AIEnabled.Value;
            if (dto.AIProvider != null) settings.AIProvider = dto.AIProvider;
            if (dto.DailyCostLimit.HasValue) settings.DailyCostLimit = dto.DailyCostLimit.Value;
            if (dto.MonthlyCostLimit.HasValue) settings.MonthlyCostLimit = dto.MonthlyCostLimit.Value;
            if (dto.EnableSchedulingRecommendations.HasValue) settings.EnableSchedulingRecommendations = dto.EnableSchedulingRecommendations.Value;
            if (dto.EnableNaturalLanguageQueries.HasValue) settings.EnableNaturalLanguageQueries = dto.EnableNaturalLanguageQueries.Value;
            if (dto.EnablePredictiveAnalytics.HasValue) settings.EnablePredictiveAnalytics = dto.EnablePredictiveAnalytics.Value;
            
            if (!string.IsNullOrEmpty(dto.ApiKey))
            {
                if (dto.AIProvider == "Gemini")
                {
                    var isValid = await _geminiClient.ValidateApiKeyAsync(dto.ApiKey);
                    if (!isValid) throw new InvalidOperationException("Invalid Gemini API key");
                }
                settings.EncryptedApiKey = EncryptApiKey(dto.ApiKey);
            }
            
            settings.LastUpdated = DateTime.UtcNow;
            settings.UpdatedBy = userId;
            
            await _repository.UpdateSettingsAsync(settings);
            return await GetSettingsAsync();
        }
        
        public async Task<AIUsageStatsDto> GetUsageStatsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            await ResetCountersIfNeededAsync(settings);
            
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var logs = await _repository.GetUsageLogsAsync(thirtyDaysAgo);
            
            var usageBreakdown = logs
                .GroupBy(l => l.RequestType)
                .Select(g => new UsageByType
                {
                    RequestType = g.Key,
                    Count = g.Count(),
                    TotalCost = g.Sum(l => l.Cost),
                    AvgProcessingTimeMs = (int)g.Average(l => l.ProcessingTimeMs)
                }).ToList();
            
            var costTrend = logs
                .GroupBy(l => l.Timestamp.Date)
                .Select(g => new DailyCostTrend
                {
                    Date = g.Key,
                    Cost = g.Sum(l => l.Cost),
                    RequestCount = g.Count()
                }).OrderBy(t => t.Date).ToList();
            
            return new AIUsageStatsDto
            {
                TodayCost = settings.CurrentDailyCost,
                MonthCost = settings.CurrentMonthlyCost,
                TodayRequests = settings.TodayRequestCount,
                MonthRequests = settings.MonthRequestCount,
                DailyCostLimit = settings.DailyCostLimit,
                MonthlyCostLimit = settings.MonthlyCostLimit,
                DailyUsagePercentage = settings.DailyCostLimit > 0 
                    ? (double)(settings.CurrentDailyCost / settings.DailyCostLimit) * 100 : 0,
                MonthlyUsagePercentage = settings.MonthlyCostLimit > 0 
                    ? (double)(settings.CurrentMonthlyCost / settings.MonthlyCostLimit) * 100 : 0,
                UsageBreakdown = usageBreakdown,
                CostTrend = costTrend
            };
        }
        
        // Encryption helpers
        private string EncryptApiKey(string apiKey)
        {
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(apiKey);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            
            return Convert.ToBase64String(result);
        }
        
        private string DecryptApiKey(string encryptedApiKey)
        {
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
            var fullCipher = Convert.FromBase64String(encryptedApiKey);
            
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
