using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HDScheduler.API.Services.AI
{
    public interface IGeminiClient
    {
        Task<GeminiResponse> GenerateContentAsync(GeminiRequest request);
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
    
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiClient> _logger;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1";
        
        public GeminiClient(HttpClient httpClient, ILogger<GeminiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<GeminiResponse> GenerateContentAsync(GeminiRequest request)
        {
            try
            {
                var url = $"{BaseUrl}/models/{request.Model}:generateContent?key={request.ApiKey}";
                
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = request.Prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = request.Temperature,
                        topK = request.TopK,
                        topP = request.TopP,
                        maxOutputTokens = request.MaxOutputTokens
                    },
                    safetySettings = new[]
                    {
                        new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                    }
                };
                
                var response = await _httpClient.PostAsJsonAsync(url, payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
                
                if (result?.Candidates == null || result.Candidates.Length == 0)
                {
                    throw new InvalidOperationException("No response from Gemini API");
                }
                
                var candidate = result.Candidates[0];
                var text = candidate.Content?.Parts?[0]?.Text ?? string.Empty;
                
                return new GeminiResponse
                {
                    Text = text,
                    FinishReason = candidate.FinishReason,
                    SafetyRatings = candidate.SafetyRatings,
                    TokenCount = result.UsageMetadata?.TotalTokenCount ?? 0,
                    PromptTokenCount = result.UsageMetadata?.PromptTokenCount ?? 0,
                    CandidatesTokenCount = result.UsageMetadata?.CandidatesTokenCount ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }
        
        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                var testRequest = new GeminiRequest
                {
                    ApiKey = apiKey,
                    Model = "gemini-1.5-flash-latest",
                    Prompt = "Test",
                    MaxOutputTokens = 10
                };
                
                await GenerateContentAsync(testRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    // Request/Response models
    public class GeminiRequest
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash-latest";
        public string Prompt { get; set; } = string.Empty;
        public double Temperature { get; set; } = 0.3;
        public int TopK { get; set; } = 40;
        public double TopP { get; set; } = 0.95;
        public int MaxOutputTokens { get; set; } = 1024;
    }
    
    public class GeminiResponse
    {
        public string Text { get; set; } = string.Empty;
        public string FinishReason { get; set; } = string.Empty;
        public List<SafetyRating>? SafetyRatings { get; set; }
        public int TokenCount { get; set; }
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
    }
    
    // Internal API response models
    internal class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
        
        [JsonPropertyName("usageMetadata")]
        public UsageMetadata? UsageMetadata { get; set; }
    }
    
    internal class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
        
        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; } = string.Empty;
        
        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating>? SafetyRatings { get; set; }
    }
    
    internal class Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }
    }
    
    internal class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
    
    public class SafetyRating
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        
        [JsonPropertyName("probability")]
        public string Probability { get; set; } = string.Empty;
    }
    
    internal class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }
        
        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }
        
        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }
}
