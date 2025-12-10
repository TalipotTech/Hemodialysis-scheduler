using HDScheduler.API.Models;
using HDScheduler.API.Data;
using Dapper;
using System.Text.Json;

namespace HDScheduler.API.Services.AI
{
    public interface IFormAutocompleteService
    {
        Task<SessionAutocompleteResponse> PredictSessionDataAsync(SessionAutocompleteRequest request);
        Task<FormAutocomplete> PredictFieldValueAsync(int patientId, string fieldName, Dictionary<string, object>? context);
        Task CacheAutocompleteAsync(int patientId, string fieldName, object value, double confidence);
        Task<object?> GetCachedAutocompleteAsync(int patientId, string fieldName);
    }

    public class FormAutocompleteService : IFormAutocompleteService
    {
        private readonly DapperContext _context;
        private readonly IGeminiClient _geminiClient;
        private readonly IAIRepository _aiRepository;
        private readonly ILogger<FormAutocompleteService> _logger;

        public FormAutocompleteService(
            DapperContext context,
            IGeminiClient geminiClient,
            IAIRepository aiRepository,
            ILogger<FormAutocompleteService> logger)
        {
            _context = context;
            _geminiClient = geminiClient;
            _aiRepository = aiRepository;
            _logger = logger;
        }

        public async Task<SessionAutocompleteResponse> PredictSessionDataAsync(SessionAutocompleteRequest request)
        {
            using var connection = _context.CreateConnection();

            // Get patient history
            var historyQuery = @"
                SELECT TOP 10 
                    AnticoagulationType, HeparinDose, SyringeType, BolusDose, HeparinInfusionRate,
                    AccessType, BloodPressure, Symptoms, SessionDate
                FROM PatientHistory
                WHERE PatientID = @PatientId
                ORDER BY SessionDate DESC";

            var history = await connection.QueryAsync<dynamic>(historyQuery, new { request.PatientId });

            // Get patient details
            var patientQuery = "SELECT Name, Age, Gender, HDCycle, DryWeight, DialyserType FROM Patients WHERE PatientID = @PatientId";
            var patient = await connection.QueryFirstOrDefaultAsync<dynamic>(patientQuery, new { request.PatientId });

            if (patient == null)
            {
                throw new InvalidOperationException($"Patient {request.PatientId} not found");
            }

            var predictions = new List<FormAutocomplete>();
            var warnings = new List<string>();

            // Build AI prompt directly (simplified, no unused contextData variable)

            var prompt = $@"You are a hemodialysis scheduling assistant. Based on the patient's history and profile, predict the most likely values for the HD session form.

Patient Profile:
- Name: {patient.Name}, Age: {patient.Age}, Gender: {patient.Gender}
- HD Cycle: {patient.HDCycle}, Dry Weight: {patient.DryWeight}
- Dialyser Type: {patient.DialyserType}

Recent Session History (last 5 sessions):
{JsonSerializer.Serialize(history.Take(5), new JsonSerializerOptions { WriteIndented = true })}

Target Session Date: {request.SessionDate:yyyy-MM-dd}

Predict the following fields with confidence scores (0-1):
1. AnticoagulationType (Heparin/LMWH/None)
2. HeparinDose (units)
3. SyringeType (10ml/20ml/50ml)
4. BolusDose (ml)
5. HeparinInfusionRate (ml/hr)
6. AccessType (AVF/AVG/Permcath)
7. BloodPressure (systolic/diastolic)

Respond in JSON format:
{{
  ""predictions"": [
    {{
      ""fieldName"": ""anticoagulationType"",
      ""predictedValue"": ""Heparin"",
      ""confidence"": 0.95,
      ""reasoning"": ""Patient consistently uses Heparin in last 10 sessions""
    }}
  ],
  ""warnings"": [""Patient age >65, monitor BP closely""],
  ""summary"": ""Based on consistent history, high confidence predictions""
}}";

            try
            {
                var settings = await _aiRepository.GetSettingsAsync();
                if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
                {
                    throw new InvalidOperationException("AI settings not configured");
                }

                var apiKey = DecryptApiKey(settings.EncryptedApiKey);
                var geminiRequest = new GeminiRequest
                {
                    ApiKey = apiKey,
                    Model = "gemini-2.0-flash",
                    Prompt = prompt,
                    Temperature = 0.5,
                    MaxOutputTokens = 1000
                };

                var geminiResponse = await _geminiClient.GenerateContentAsync(geminiRequest);
                var aiResponse = geminiResponse.Text;
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}') + 1;
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = aiResponse.Substring(jsonStart, jsonEnd - jsonStart);
                    var aiData = JsonSerializer.Deserialize<JsonElement>(json);

                    if (aiData.TryGetProperty("predictions", out var predsArray))
                    {
                        foreach (var pred in predsArray.EnumerateArray())
                        {
                            predictions.Add(new FormAutocomplete
                            {
                                FieldName = pred.GetProperty("fieldName").GetString() ?? "",
                                PredictedValue = pred.GetProperty("predictedValue").GetString() ?? "",
                                Confidence = pred.GetProperty("confidence").GetDouble(),
                                Reasoning = pred.GetProperty("reasoning").GetString() ?? "",
                                DataSources = new List<string> { "Patient History", "AI Analysis" }
                            });
                        }
                    }

                    if (aiData.TryGetProperty("warnings", out var warningsArray))
                    {
                        foreach (var warning in warningsArray.EnumerateArray())
                        {
                            warnings.Add(warning.GetString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI predictions");
                // Fallback to rule-based predictions
                predictions.AddRange(GetRuleBasedPredictions(history, patient));
            }

            // Cache predictions
            foreach (var pred in predictions.Where(p => p.Confidence > 0.7))
            {
                await CacheAutocompleteAsync(request.PatientId, pred.FieldName, pred.PredictedValue, pred.Confidence);
            }

            return new SessionAutocompleteResponse
            {
                PatientId = request.PatientId,
                PatientName = patient.Name,
                Predictions = predictions,
                Summary = predictions.Any() 
                    ? $"Generated {predictions.Count} predictions with avg confidence {predictions.Average(p => p.Confidence):P0}"
                    : "No predictions available",
                Warnings = warnings,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private List<FormAutocomplete> GetRuleBasedPredictions(IEnumerable<dynamic> history, dynamic patient)
        {
            var predictions = new List<FormAutocomplete>();
            var historyList = history.ToList();

            if (!historyList.Any()) return predictions;

            // Most common anticoagulation type
            var mostCommonAnticoag = historyList
                .GroupBy(h => (string)h.AnticoagulationType)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            if (!string.IsNullOrEmpty(mostCommonAnticoag))
            {
                predictions.Add(new FormAutocomplete
                {
                    FieldName = "anticoagulationType",
                    PredictedValue = mostCommonAnticoag,
                    Confidence = 0.9,
                    Reasoning = $"Used in {historyList.Count(h => h.AnticoagulationType == mostCommonAnticoag)} recent sessions",
                    DataSources = new List<string> { "Patient History" }
                });
            }

            // Average heparin dose
            var avgHeparinDose = historyList
                .Where(h => h.HeparinDose != null)
                .Select(h => (int)h.HeparinDose)
                .DefaultIfEmpty(0)
                .Average();

            if (avgHeparinDose > 0)
            {
                predictions.Add(new FormAutocomplete
                {
                    FieldName = "heparinDose",
                    PredictedValue = (int)avgHeparinDose,
                    Confidence = 0.85,
                    Reasoning = $"Average from last {historyList.Count} sessions",
                    DataSources = new List<string> { "Patient History" }
                });
            }

            return predictions;
        }

        public async Task<FormAutocomplete> PredictFieldValueAsync(int patientId, string fieldName, Dictionary<string, object>? context)
        {
            // Check cache first
            var cached = await GetCachedAutocompleteAsync(patientId, fieldName);
            if (cached != null)
            {
                return new FormAutocomplete
                {
                    FieldName = fieldName,
                    PredictedValue = cached,
                    Confidence = 0.95,
                    Reasoning = "Cached from recent sessions",
                    DataSources = new List<string> { "Cache" }
                };
            }

            using var connection = _context.CreateConnection();
            var query = $"SELECT TOP 1 {fieldName} FROM PatientHistory WHERE PatientID = @PatientId AND {fieldName} IS NOT NULL ORDER BY SessionDate DESC";
            
            var value = await connection.QueryFirstOrDefaultAsync<object>(query, new { PatientId = patientId });

            return new FormAutocomplete
            {
                FieldName = fieldName,
                PredictedValue = value ?? "",
                Confidence = value != null ? 0.8 : 0.0,
                Reasoning = value != null ? "From most recent session" : "No history available",
                DataSources = new List<string> { "Patient History" }
            };
        }

        public async Task CacheAutocompleteAsync(int patientId, string fieldName, object value, double confidence)
        {
            using var connection = _context.CreateConnection();

            var query = @"
                MERGE AutocompleteCache AS target
                USING (SELECT @PatientId AS PatientID, @FieldName AS FieldName) AS source
                ON target.PatientID = source.PatientID AND target.FieldName = source.FieldName
                WHEN MATCHED THEN
                    UPDATE SET 
                        PredictedValue = @Value,
                        Confidence = @Confidence,
                        UsageCount = target.UsageCount + 1,
                        LastUsed = GETUTCDATE(),
                        ExpiresAt = DATEADD(day, 30, GETUTCDATE())
                WHEN NOT MATCHED THEN
                    INSERT (PatientID, FieldName, PredictedValue, Confidence, UsageCount, LastUsed, CreatedAt, ExpiresAt)
                    VALUES (@PatientId, @FieldName, @Value, @Confidence, 1, GETUTCDATE(), GETUTCDATE(), DATEADD(day, 30, GETUTCDATE()));";

            await connection.ExecuteAsync(query, new
            {
                PatientId = patientId,
                FieldName = fieldName,
                Value = JsonSerializer.Serialize(value),
                Confidence = confidence
            });
        }

        public async Task<object?> GetCachedAutocompleteAsync(int patientId, string fieldName)
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT PredictedValue, Confidence
                FROM AutocompleteCache
                WHERE PatientID = @PatientId 
                  AND FieldName = @FieldName 
                  AND ExpiresAt > GETUTCDATE()
                  AND Confidence > 0.7";

            var cached = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { PatientId = patientId, FieldName = fieldName });

            if (cached != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(cached.PredictedValue);
                }
                catch
                {
                    return cached.PredictedValue;
                }
            }

            return null;
        }

        private string DecryptApiKey(string encryptedKey)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedKey));
        }
    }
}
