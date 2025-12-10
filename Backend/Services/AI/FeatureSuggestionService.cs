using HDScheduler.API.Models;
using HDScheduler.API.Data;
using Dapper;
using System.Text.Json;

namespace HDScheduler.API.Services.AI
{
    public interface IFeatureSuggestionService
    {
        Task<FeatureAnalysisResponse> AnalyzeAndSuggestFeaturesAsync(FeatureAnalysisRequest request);
        Task<AIFeatureSuggestion> SaveFeatureSuggestionAsync(AIFeatureSuggestion suggestion);
        Task<List<AIFeatureSuggestion>> GetPendingSuggestionsAsync(string? category = null);
        Task<AIFeatureSuggestion> UpdateSuggestionStatusAsync(int id, bool isImplemented, string? notes);
        Task<Dictionary<string, int>> GetSuggestionStatsAsync();
    }

    public class FeatureSuggestionService : IFeatureSuggestionService
    {
        private readonly DapperContext _context;
        private readonly IGeminiClient _geminiClient;
        private readonly IAIRepository _aiRepository;
        private readonly ILogger<FeatureSuggestionService> _logger;
        private const string AI_MODEL_VERSION = "Gemini-2.0-Flash";

        public FeatureSuggestionService(
            DapperContext context,
            IGeminiClient geminiClient,
            IAIRepository aiRepository,
            ILogger<FeatureSuggestionService> logger)
        {
            _context = context;
            _geminiClient = geminiClient;
            _aiRepository = aiRepository;
            _logger = logger;
        }

        public async Task<FeatureAnalysisResponse> AnalyzeAndSuggestFeaturesAsync(FeatureAnalysisRequest request)
        {
            using var connection = _context.CreateConnection();

            // Gather system usage data
            var usageStats = await GetSystemUsageStatsAsync();
            var recentErrors = await GetRecentErrorPatternsAsync();
            var userBehavior = await GetUserBehaviorPatternsAsync();

            var prompt = BuildFeatureAnalysisPrompt(request, usageStats, recentErrors, userBehavior);

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
                    Temperature = 0.7,
                    MaxOutputTokens = 2000
                };

                var geminiResponse = await _geminiClient.GenerateContentAsync(geminiRequest);
                var aiResponse = geminiResponse.Text;
                var suggestions = ParseAISuggestionsFromResponse(aiResponse);

                // Save to database for developer review
                foreach (var suggestion in suggestions)
                {
                    suggestion.GeneratedBy = AI_MODEL_VERSION;
                    suggestion.GeneratedAt = DateTime.UtcNow;
                    await SaveFeatureSuggestionAsync(suggestion);
                }

                return new FeatureAnalysisResponse
                {
                    Suggestions = suggestions,
                    Summary = $"Generated {suggestions.Count} feature suggestions based on {request.AnalysisType} analysis",
                    TotalSuggestions = suggestions.Count,
                    AnalyzedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating feature suggestions");
                throw;
            }
        }

        private string BuildFeatureAnalysisPrompt(FeatureAnalysisRequest request, dynamic usageStats, List<string> errors, dynamic behavior)
        {
            return $@"You are an AI software architect analyzing a hemodialysis scheduling system. 
Analyze current usage patterns and suggest features to improve developer productivity and user experience.

ANALYSIS TYPE: {request.AnalysisType}
FEATURE AREA: {request.FeatureArea ?? "All"}

CURRENT SYSTEM USAGE:
{JsonSerializer.Serialize(usageStats, new JsonSerializerOptions { WriteIndented = true })}

RECENT ERROR PATTERNS:
{string.Join("\n", errors.Take(10))}

USER BEHAVIOR PATTERNS:
{JsonSerializer.Serialize(behavior, new JsonSerializerOptions { WriteIndented = true })}

SUGGEST FEATURES IN THESE CATEGORIES:
1. AUTOCOMPLETE & AI-ASSISTED INPUT
   - Smart form field predictions
   - Context-aware suggestions
   - Reduce data entry effort

2. WORKFLOW OPTIMIZATION
   - Process automation opportunities
   - Bottleneck elimination
   - Time-saving features

3. ANALYTICS & INSIGHTS
   - Predictive analytics
   - Trend detection
   - Decision support

4. UI/UX IMPROVEMENTS
   - Usability enhancements
   - Mobile responsiveness
   - Accessibility features

5. INTEGRATION OPPORTUNITIES
   - Third-party system connections
   - API expansions
   - Data import/export

For EACH suggestion, provide:
- Feature Title (concise, action-oriented)
- Description (detailed, technical)
- Category (from above)
- Priority (High/Medium/Low)
- Impact Score (1-10): Business value + user benefit
- Implementation Complexity (1-10): Technical difficulty
- Estimated Effort (Hours/Days/Weeks)
- Reasoning: WHY this feature matters now

Respond in JSON:
{{
  ""suggestions"": [
    {{
      ""featureTitle"": ""Smart Heparin Dose Prediction"",
      ""description"": ""ML model predicts optimal heparin dose based on patient weight, age, history. Reduces manual calculation errors."",
      ""category"": ""Autocomplete"",
      ""priority"": ""High"",
      ""impactScore"": 9,
      ""implementationComplexity"": 6,
      ""estimatedEffort"": ""Days"",
      ""reasoning"": ""Heparin dosing is repeated 100+ times/day. 30% of sessions show dose variance. Autocomplete can save 2 min/session = 200 min/day.""
    }}
  ]
}}

FOCUS on features that:
- Reduce repetitive data entry (biggest pain point)
- Leverage AI/ML for predictions
- Have high ROI (impact vs effort)
- Are technically feasible in <2 weeks

Generate 5-10 prioritized suggestions.";
        }

        private List<AIFeatureSuggestion> ParseAISuggestionsFromResponse(string aiResponse)
        {
            var suggestions = new List<AIFeatureSuggestion>();

            try
            {
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}') + 1;

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = aiResponse.Substring(jsonStart, jsonEnd - jsonStart);
                    var data = JsonSerializer.Deserialize<JsonElement>(json);

                    if (data.TryGetProperty("suggestions", out var suggestionsArray))
                    {
                        foreach (var item in suggestionsArray.EnumerateArray())
                        {
                            suggestions.Add(new AIFeatureSuggestion
                            {
                                FeatureTitle = item.GetProperty("featureTitle").GetString() ?? "",
                                Description = item.GetProperty("description").GetString() ?? "",
                                Category = item.GetProperty("category").GetString() ?? "General",
                                Priority = item.GetProperty("priority").GetString() ?? "Medium",
                                ImpactScore = item.TryGetProperty("impactScore", out var impact) ? impact.GetInt32() : 5,
                                ImplementationComplexity = item.TryGetProperty("implementationComplexity", out var complexity) ? complexity.GetInt32() : 5,
                                EstimatedEffort = item.GetProperty("estimatedEffort").GetString() ?? "Days",
                                Reasoning = item.GetProperty("reasoning").GetString() ?? "",
                                Context = JsonSerializer.Serialize(item),
                                IsReviewed = false,
                                IsImplemented = false,
                                UpvoteCount = 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI suggestions");
            }

            return suggestions;
        }

        public async Task<AIFeatureSuggestion> SaveFeatureSuggestionAsync(AIFeatureSuggestion suggestion)
        {
            using var connection = _context.CreateConnection();

            // Check for duplicates
            var existingQuery = "SELECT COUNT(*) FROM AIFeatureSuggestions WHERE FeatureTitle = @Title";
            var exists = await connection.ExecuteScalarAsync<int>(existingQuery, new { Title = suggestion.FeatureTitle });

            if (exists > 0)
            {
                _logger.LogInformation($"Feature suggestion '{suggestion.FeatureTitle}' already exists, skipping");
                return suggestion;
            }

            var query = @"
                INSERT INTO AIFeatureSuggestions 
                (FeatureTitle, Description, Category, Priority, Context, Reasoning, 
                 ImpactScore, ImplementationComplexity, EstimatedEffort, GeneratedBy, GeneratedAt)
                VALUES 
                (@FeatureTitle, @Description, @Category, @Priority, @Context, @Reasoning,
                 @ImpactScore, @ImplementationComplexity, @EstimatedEffort, @GeneratedBy, @GeneratedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await connection.ExecuteScalarAsync<int>(query, suggestion);
            suggestion.Id = id;

            _logger.LogInformation($"Saved feature suggestion: {suggestion.FeatureTitle} (Priority: {suggestion.Priority}, Impact: {suggestion.ImpactScore})");

            return suggestion;
        }

        public async Task<List<AIFeatureSuggestion>> GetPendingSuggestionsAsync(string? category = null)
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT * FROM AIFeatureSuggestions
                WHERE IsImplemented = 0
                " + (category != null ? "AND Category = @Category" : "") + @"
                ORDER BY 
                    CASE Priority 
                        WHEN 'High' THEN 1 
                        WHEN 'Medium' THEN 2 
                        ELSE 3 
                    END,
                    ImpactScore DESC,
                    ImplementationComplexity ASC";

            var suggestions = await connection.QueryAsync<AIFeatureSuggestion>(query, new { Category = category });
            return suggestions.ToList();
        }

        public async Task<AIFeatureSuggestion> UpdateSuggestionStatusAsync(int id, bool isImplemented, string? notes)
        {
            using var connection = _context.CreateConnection();

            var query = @"
                UPDATE AIFeatureSuggestions
                SET IsImplemented = @IsImplemented,
                    ImplementedAt = CASE WHEN @IsImplemented = 1 THEN GETUTCDATE() ELSE NULL END,
                    DeveloperNotes = @Notes,
                    IsReviewed = 1
                WHERE Id = @Id";

            await connection.ExecuteAsync(query, new { Id = id, IsImplemented = isImplemented, Notes = notes });

            var selectQuery = "SELECT * FROM AIFeatureSuggestions WHERE Id = @Id";
            var updated = await connection.QueryFirstOrDefaultAsync<AIFeatureSuggestion>(selectQuery, new { Id = id });

            return updated ?? throw new InvalidOperationException($"Feature suggestion {id} not found");
        }

        public async Task<Dictionary<string, int>> GetSuggestionStatsAsync()
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT 
                    Category,
                    COUNT(*) as Count
                FROM AIFeatureSuggestions
                WHERE IsImplemented = 0
                GROUP BY Category";

            var stats = await connection.QueryAsync<dynamic>(query);

            var totalQuery = @"
                SELECT 
                    COUNT(*) as Total,
                    SUM(CASE WHEN IsImplemented = 1 THEN 1 ELSE 0 END) as Implemented,
                    SUM(CASE WHEN Priority = 'High' THEN 1 ELSE 0 END) as HighPriority,
                    AVG(CAST(ImpactScore AS FLOAT)) as AvgImpact
                FROM AIFeatureSuggestions";

            var totals = await connection.QueryFirstOrDefaultAsync<dynamic>(totalQuery);

            var result = new Dictionary<string, int>
            {
                ["Total"] = totals?.Total ?? 0,
                ["Implemented"] = totals?.Implemented ?? 0,
                ["Pending"] = (totals?.Total ?? 0) - (totals?.Implemented ?? 0),
                ["HighPriority"] = totals?.HighPriority ?? 0
            };

            foreach (var stat in stats)
            {
                result[stat.Category] = stat.Count;
            }

            return result;
        }

        private async Task<dynamic> GetSystemUsageStatsAsync()
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT 
                    (SELECT COUNT(*) FROM BedAssignments WHERE CAST(AssignmentDate AS DATE) = CAST(GETDATE() AS DATE)) as TodaySessions,
                    (SELECT COUNT(*) FROM Patients WHERE IsActive = 1) as ActivePatients,
                    (SELECT COUNT(*) FROM AIUsageLogs WHERE Timestamp >= DATEADD(day, -7, GETDATE())) as AIRequestsLast7Days,
                    (SELECT AVG(CAST(ProcessingTimeMs AS FLOAT)) FROM AIUsageLogs WHERE Timestamp >= DATEADD(day, -1, GETDATE())) as AvgAIResponseTime";

            return await connection.QueryFirstOrDefaultAsync<dynamic>(query);
        }

        private async Task<List<string>> GetRecentErrorPatternsAsync()
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT TOP 20 ErrorMessage 
                FROM AIUsageLogs 
                WHERE Success = 0 AND ErrorMessage IS NOT NULL AND Timestamp >= DATEADD(day, -7, GETDATE())
                GROUP BY ErrorMessage
                ORDER BY COUNT(*) DESC";

            var errors = await connection.QueryAsync<string>(query);
            return errors.ToList();
        }

        private async Task<dynamic> GetUserBehaviorPatternsAsync()
        {
            using var connection = _context.CreateConnection();

            var query = @"
                SELECT 
                    RequestType,
                    COUNT(*) as UsageCount,
                    AVG(CAST(ProcessingTimeMs AS FLOAT)) as AvgResponseTime
                FROM AIUsageLogs
                WHERE Timestamp >= DATEADD(day, -30, GETDATE())
                GROUP BY RequestType
                ORDER BY UsageCount DESC";

            var patterns = await connection.QueryAsync<dynamic>(query);
            return patterns;
        }

        private string DecryptApiKey(string encryptedKey)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedKey));
        }
    }
}
