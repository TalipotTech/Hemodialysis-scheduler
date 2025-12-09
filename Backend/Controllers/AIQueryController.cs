using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Repositories;
using HDScheduler.API.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIQueryController : ControllerBase
{
    private readonly IGeminiClient _geminiClient;
    private readonly IAIRepository _aiRepository;
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly ILogger<AIQueryController> _logger;
    private const string ENCRYPTION_KEY = "HDScheduler2025AiKey32Bytes!@";

    public AIQueryController(
        IGeminiClient geminiClient,
        IAIRepository aiRepository,
        IScheduleRepository scheduleRepo,
        IPatientRepository patientRepo,
        ILogger<AIQueryController> logger)
    {
        _geminiClient = geminiClient;
        _aiRepository = aiRepository;
        _scheduleRepo = scheduleRepo;
        _patientRepo = patientRepo;
        _logger = logger;
    }

    [HttpPost("natural-query")]
    public async Task<IActionResult> ProcessNaturalQuery([FromBody] NaturalQueryRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing natural language query: {request.Query}");

            // Step 1: Use Gemini to interpret the query
            var interpretation = await InterpretQuery(request.Query);

            // Step 2: Execute the query based on interpretation
            var result = await ExecuteQuery(interpretation);

            // Step 3: Generate natural language response
            var response = await GenerateNaturalResponse(result, request.Query, interpretation);

            return Ok(new
            {
                success = true,
                query = request.Query,
                interpretation = interpretation,
                data = result,
                answer = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural query");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpGet("saved-prompts")]
    public async Task<IActionResult> GetSavedPrompts()
    {
        try
        {
            var prompts = await _aiRepository.GetSavedPromptsAsync();
            return Ok(prompts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved prompts");
            return StatusCode(500, new { error = "Failed to retrieve saved prompts" });
        }
    }

    [HttpPost("saved-prompts")]
    public async Task<IActionResult> SavePrompt([FromBody] SavePromptRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PromptText))
            {
                return BadRequest(new { error = "Prompt text is required" });
            }

            var promptId = await _aiRepository.SavePromptAsync(request.PromptText, request.Category);
            return Ok(new { promptId, message = "Prompt saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving prompt");
            return StatusCode(500, new { error = "Failed to save prompt" });
        }
    }

    [HttpPut("saved-prompts/{id}/use")]
    public async Task<IActionResult> RecordPromptUsage(int id)
    {
        try
        {
            await _aiRepository.IncrementPromptUsageAsync(id);
            return Ok(new { message = "Prompt usage recorded" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording prompt usage");
            return StatusCode(500, new { error = "Failed to record usage" });
        }
    }

    [HttpDelete("saved-prompts/{id}")]
    public async Task<IActionResult> DeletePrompt(int id)
    {
        try
        {
            await _aiRepository.DeletePromptAsync(id);
            return Ok(new { message = "Prompt deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt");
            return StatusCode(500, new { error = "Failed to delete prompt" });
        }
    }

    private async Task<QueryInterpretation> InterpretQuery(string query)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        var promptText = $@"Parse this query into structured JSON. Output ONLY valid JSON with no markdown or explanations.

TODAY={today:yyyy-MM-dd}, TOMORROW={tomorrow:yyyy-MM-dd}

Query: ""{query}""

Match to ONE intent:
• bed_availability: questions about available/free beds, capacity, open slots
• patient_schedule: questions about scheduled patients or who is scheduled
• slot_utilization: questions about usage patterns, occupancy, how busy
• patient_search: searching for specific patient by name
• missed_appointments: questions about no-shows or missed appointments
• general_info: ONLY if query is truly unclear or general

Extract parameters (use null if not mentioned):
- date: specific date in YYYY-MM-DD format
- timeOfDay: ""morning"", ""afternoon"", ""evening"", or null
- patientName: patient name if searching
- All other parameters should be null

EXAMPLE 1:
Query: ""Show me available beds for tomorrow morning""
{{""intent"":""bed_availability"",""parameters"":{{""date"":""{tomorrow:yyyy-MM-dd}"",""timeOfDay"":""morning"",""dateFrom"":null,""dateTo"":null,""slot"":null,""patientName"":null,""patientId"":null}},""confidence"":0.95}}

EXAMPLE 2:
Query: ""Who is scheduled tomorrow""
{{""intent"":""patient_schedule"",""parameters"":{{""date"":""{tomorrow:yyyy-MM-dd}"",""timeOfDay"":null,""dateFrom"":null,""dateTo"":null,""slot"":null,""patientName"":null,""patientId"":null}},""confidence"":0.95}}

Now parse the user's query and respond with ONLY the JSON:";

        try
        {
            var settings = await _aiRepository.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
            {
                throw new InvalidOperationException("AI not configured");
            }

            var geminiRequest = new GeminiRequest
            {
                ApiKey = DecryptApiKey(settings.EncryptedApiKey),
                Model = "gemini-2.0-flash",
                Prompt = promptText,
                Temperature = 0.3,
                MaxOutputTokens = 1000
            };

            var geminiResponse = await _geminiClient.GenerateContentAsync(geminiRequest);
            var response = geminiResponse.Text;
            
            _logger.LogInformation($"Gemini raw response: {response}");
            
            // Clean response - remove markdown code blocks if present
            var cleanedResponse = response
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();
            
            // Parse JSON from response - try to find JSON object
            var jsonMatch = Regex.Match(cleanedResponse, @"\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                var interpretation = JsonSerializer.Deserialize<QueryInterpretation>(jsonMatch.Value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new QueryInterpretation { Intent = "general_info" };
                
                _logger.LogInformation($"Parsed intent: '{interpretation.Intent}', Parameters: {JsonSerializer.Serialize(interpretation.Parameters)}");
                return interpretation;
            }

            _logger.LogWarning($"Could not parse JSON from Gemini response. Raw: {response}");
            return new QueryInterpretation { Intent = "general_info" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interpreting query");
            return new QueryInterpretation { Intent = "general_info" };
        }
    }

    private async Task<object> ExecuteQuery(QueryInterpretation interpretation)
    {
        // Log the interpretation for debugging
        _logger.LogInformation($"Executing query with intent: '{interpretation.Intent}', Parameters: {JsonSerializer.Serialize(interpretation.Parameters)}");

        // Normalize intent (trim and lowercase)
        var normalizedIntent = interpretation.Intent?.Trim().ToLower() ?? "general_info";

        // Map time of day to slot
        if (!string.IsNullOrEmpty(interpretation.Parameters.TimeOfDay) && interpretation.Parameters.Slot == null)
        {
            interpretation.Parameters.Slot = interpretation.Parameters.TimeOfDay.ToLower() switch
            {
                "morning" => 1,
                "afternoon" => 2,
                "evening" => 3,
                "night" => 4,
                _ => null
            };
        }

        return normalizedIntent switch
        {
            "bed_availability" => await GetBedAvailability(
                interpretation.Parameters.Date ?? DateTime.Today.ToString("yyyy-MM-dd"),
                interpretation.Parameters.Slot
            ),
            "patient_schedule" => await GetPatientSchedule(
                interpretation.Parameters.Date ?? DateTime.Today.ToString("yyyy-MM-dd"),
                interpretation.Parameters.Slot
            ),
            "slot_utilization" => await GetSlotUtilization(
                interpretation.Parameters.Date ?? DateTime.Today.ToString("yyyy-MM-dd")
            ),
            "patient_search" => await SearchPatients(
                interpretation.Parameters.PatientId,
                interpretation.Parameters.PatientName
            ),
            "missed_appointments" => await GetMissedAppointments(
                interpretation.Parameters.DateFrom ?? DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd"),
                interpretation.Parameters.DateTo ?? DateTime.Today.ToString("yyyy-MM-dd")
            ),
            _ => new { message = "Query understood but not yet implemented" }
        };
    }

    private async Task<string> GenerateNaturalResponse(object data, string originalQuery, QueryInterpretation interpretation)
    {
        var dataJson = JsonSerializer.Serialize(data);
        var promptText = $"User asked: \"{originalQuery}\"\n\n" +
            $"Data found:\n{dataJson}\n\n" +
            "Generate a friendly, professional response that:\n" +
            "1. Directly answers the user's question\n" +
            "2. Presents the data in a clear, readable format\n" +
            "3. Keeps the response concise (2-3 sentences)\n\n" +
            "Response:";

        try
        {
            var settings = await _aiRepository.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
            {
                _logger.LogWarning("AI settings not configured for natural response generation");
                return FormatSimpleResponse(data, interpretation);
            }

            string apiKey;
            try
            {
                apiKey = DecryptApiKey(settings.EncryptedApiKey);
            }
            catch (Exception decryptEx)
            {
                _logger.LogError(decryptEx, "Failed to decrypt API key");
                return FormatSimpleResponse(data, interpretation);
            }

            var geminiRequest = new GeminiRequest
            {
                ApiKey = apiKey,
                Model = "gemini-2.0-flash",
                Prompt = promptText,
                Temperature = 0.5,
                MaxOutputTokens = 500
            };

            var geminiResponse = await _geminiClient.GenerateContentAsync(geminiRequest);
            return geminiResponse.Text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating natural response");
            return FormatSimpleResponse(data, interpretation);
        }
    }

    // Helper methods for different query types
    private async Task<object> GetBedAvailability(string date, int? slot)
    {
        var dateObj = DateTime.Parse(date);
        var allBeds = Enumerable.Range(1, 10).ToList();
        
        var occupiedBeds = await _scheduleRepo.GetOccupiedBeds(dateObj, slot);
        var availableBeds = allBeds.Except(occupiedBeds).ToList();

        return new
        {
            date,
            slot = slot ?? 0,
            slotName = slot switch
            {
                1 => "Morning (6:00 AM - 10:00 AM)",
                2 => "Afternoon (11:00 AM - 3:00 PM)",
                3 => "Evening (4:00 PM - 8:00 PM)",
                4 => "Night (9:00 PM - 1:00 AM)",
                _ => "All Slots"
            },
            totalBeds = 10,
            occupiedBeds = occupiedBeds.Count,
            availableBeds = availableBeds.Count,
            availableBedNumbers = availableBeds
        };
    }

    private async Task<object> GetPatientSchedule(string date, int? slot)
    {
        var dateObj = DateTime.Parse(date);
        var patients = await _scheduleRepo.GetScheduledPatients(dateObj, slot);

        return new
        {
            date,
            slot = slot ?? 0,
            count = patients.Count,
            patients = patients
        };
    }

    private async Task<object> GetSlotUtilization(string date)
    {
        var dateObj = DateTime.Parse(date);
        var utilization = await _scheduleRepo.GetSlotUtilization(dateObj);

        return new
        {
            date,
            slots = utilization
        };
    }

    private async Task<object> SearchPatients(int? id, string? name)
    {
        if (id.HasValue)
        {
            var patient = await _patientRepo.GetPatientById(id.Value);
            return new { patients = patient != null ? new[] { patient } : Array.Empty<Patient>() };
        }
        else if (!string.IsNullOrEmpty(name))
        {
            var patients = await _patientRepo.SearchPatientsByName(name);
            return new { patients = patients };
        }

        return new { patients = Array.Empty<Patient>() };
    }

    private async Task<object> GetMissedAppointments(string dateFrom, string dateTo)
    {
        var fromDate = DateTime.Parse(dateFrom);
        var toDate = DateTime.Parse(dateTo);
        var missedAppointments = await _scheduleRepo.GetMissedAppointments(fromDate, toDate);

        return new
        {
            dateFrom,
            dateTo,
            count = missedAppointments.Count,
            appointments = missedAppointments
        };
    }

    // Helper method for simple response formatting
    private string FormatSimpleResponse(object data, QueryInterpretation interpretation)
    {
        var intent = interpretation.Intent?.ToLower() ?? "unknown";
        var dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        
        return intent switch
        {
            "bed_availability" => $"Here are the available beds based on your query:\n\n{dataJson}",
            "patient_schedule" => $"Here are the scheduled patients:\n\n{dataJson}",
            "slot_utilization" => $"Here is the slot utilization information:\n\n{dataJson}",
            "patient_search" => $"Here are the search results:\n\n{dataJson}",
            "missed_appointments" => $"Here are the missed appointments:\n\n{dataJson}",
            _ => $"Here's the information you requested:\n\n{dataJson}"
        };
    }

    // Utility method
    private string DecryptApiKey(string encryptedApiKey)
    {
        // Use SHA256 hash of the key to match AIService encryption
        var keyBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
        var fullCipher = Convert.FromBase64String(encryptedApiKey);
        
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = keyBytes;
        
        // Extract IV from first 16 bytes
        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];
        
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        
        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}

// DTOs
public class NaturalQueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class SavePromptRequest
{
    public string PromptText { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class QueryInterpretation
{
    public string Intent { get; set; } = string.Empty;
    public QueryParameters Parameters { get; set; } = new();
    public double Confidence { get; set; }
}

public class QueryParameters
{
    public string? Date { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public int? Slot { get; set; }
    public string? TimeOfDay { get; set; }
    public string? PatientName { get; set; }
    public int? PatientId { get; set; }
}
