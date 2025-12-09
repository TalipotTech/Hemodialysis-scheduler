using HDScheduler.API.Models;
using HDScheduler.API.Data;
using Dapper;
using System.Text;

namespace HDScheduler.API.Services.AI
{
    public interface IReportGenerationService
    {
        Task<string> GenerateDailyReportAsync(DateTime date);
        Task<string> GenerateWeeklyReportAsync(DateTime startDate);
        Task<string> GeneratePatientReportAsync(int patientId, DateTime startDate, DateTime endDate);
        Task<CustomReport> GenerateCustomReportAsync(string reportType, Dictionary<string, object>? parameters);
        List<ReportTemplate> GetAvailableTemplates();
        Task<byte[]> ExportToPdfAsync(string reportContent, string title);
    }

    public class ReportGenerationService : IReportGenerationService
    {
        private readonly DapperContext _context;
        private readonly IGeminiClient _geminiClient;
        private readonly IAIRepository _aiRepository;
        private readonly ILogger<ReportGenerationService> _logger;

        public ReportGenerationService(
            DapperContext context,
            IGeminiClient geminiClient,
            IAIRepository aiRepository,
            ILogger<ReportGenerationService> logger)
        {
            _context = context;
            _geminiClient = geminiClient;
            _aiRepository = aiRepository;
            _logger = logger;
        }

        public async Task<string> GenerateDailyReportAsync(DateTime date)
        {
            using var connection = _context.CreateConnection();

            var dailyQuery = "SELECT COUNT(DISTINCT ba.AssignmentID) as TotalSessions, " +
                           "COUNT(DISTINCT CASE WHEN ba.DischargedAt IS NOT NULL THEN ba.AssignmentID END) as CompletedSessions, " +
                           "COUNT(DISTINCT ba.PatientID) as PatientsServed " +
                           "FROM BedAssignments ba WHERE CAST(ba.AssignmentDate AS DATE) = @Date";

            var stats = await connection.QueryFirstOrDefaultAsync<dynamic>(dailyQuery, new { Date = date.Date });

            int total = stats?.TotalSessions ?? 0;
            int completed = stats?.CompletedSessions ?? 0;
            int patients = stats?.PatientsServed ?? 0;

            var prompt = $"Generate a brief daily hemodialysis report for {date:yyyy-MM-dd}. " +
                        $"Total sessions: {total}, Completed: {completed}, Patients served: {patients}. " +
                        "Provide a 2-3 sentence summary with key insights.";

            return await GenerateReportWithAI(prompt);
        }

        public async Task<string> GenerateWeeklyReportAsync(DateTime startDate)
        {
            using var connection = _context.CreateConnection();
            var endDate = startDate.AddDays(7);

            var weeklyQuery = "SELECT COUNT(DISTINCT ba.AssignmentID) as TotalSessions, " +
                            "COUNT(DISTINCT ba.PatientID) as UniquePatientsServed " +
                            "FROM BedAssignments ba " +
                            "WHERE ba.AssignmentDate >= @StartDate AND ba.AssignmentDate < @EndDate";

            var stats = await connection.QueryFirstOrDefaultAsync<dynamic>(weeklyQuery, 
                new { StartDate = startDate, EndDate = endDate });

            int sessions = stats?.TotalSessions ?? 0;
            int patients = stats?.UniquePatientsServed ?? 0;

            var prompt = $"Generate a weekly hemodialysis summary for {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. " +
                        $"Total sessions: {sessions}, Unique patients: {patients}. " +
                        "Provide insights on utilization trends and recommendations.";

            return await GenerateReportWithAI(prompt);
        }

        public async Task<string> GeneratePatientReportAsync(int patientId, DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();

            var patientQuery = "SELECT Name, Age, HDCycle, IsActive FROM Patients WHERE PatientID = @PatientId";
            var patient = await connection.QueryFirstOrDefaultAsync<dynamic>(patientQuery, new { PatientId = patientId });

            if (patient == null)
            {
                throw new InvalidOperationException($"Patient {patientId} not found");
            }

            var treatmentQuery = "SELECT COUNT(*) as TreatmentCount " +
                               "FROM BedAssignments ba " +
                               "WHERE ba.PatientID = @PatientId AND ba.AssignmentDate >= @StartDate AND ba.AssignmentDate < @EndDate";

            var treatments = await connection.QueryFirstOrDefaultAsync<dynamic>(treatmentQuery,
                new { PatientId = patientId, StartDate = startDate, EndDate = endDate });

            int count = treatments?.TreatmentCount ?? 0;

            var prompt = $"Generate a patient treatment summary for {patient.Name}, Age {patient.Age}, HD Cycle {patient.HDCycle}x/week. " +
                        $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. Total treatments: {count}. " +
                        "Provide clinical summary and recommendations.";

            return await GenerateReportWithAI(prompt);
        }

        public async Task<CustomReport> GenerateCustomReportAsync(string reportType, Dictionary<string, object>? parameters)
        {
            var paramsText = parameters != null 
                ? string.Join(", ", parameters.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "None";

            var prompt = $"Generate a {reportType} hemodialysis report with parameters: {paramsText}. " +
                        "Include executive summary, key metrics, trends, and recommendations.";

            var content = await GenerateReportWithAI(prompt);

            return new CustomReport
            {
                ReportType = reportType,
                Parameters = parameters ?? new Dictionary<string, object>(),
                Content = content,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public List<ReportTemplate> GetAvailableTemplates()
        {
            return new List<ReportTemplate>
            {
                new ReportTemplate { Id = "daily", Name = "Daily Summary Report", Description = "Overview of daily sessions and activity", Category = "Operational" },
                new ReportTemplate { Id = "weekly", Name = "Weekly Summary Report", Description = "Weekly trends and performance metrics", Category = "Operational" },
                new ReportTemplate { Id = "patient", Name = "Patient Treatment Report", Description = "Individual patient treatment history", Category = "Clinical" },
                new ReportTemplate { Id = "utilization", Name = "Bed Utilization Report", Description = "Capacity analysis and optimization", Category = "Capacity" },
                new ReportTemplate { Id = "compliance", Name = "Treatment Compliance Report", Description = "Patient adherence and missed sessions", Category = "Quality" },
                new ReportTemplate { Id = "performance", Name = "Performance Metrics Report", Description = "Key performance indicators analysis", Category = "Analytics" }
            };
        }

        public async Task<byte[]> ExportToPdfAsync(string reportContent, string title)
        {
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{title}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #5f27cd; border-bottom: 3px solid #5f27cd; padding-bottom: 10px; }}
        .metadata {{ color: #666; font-size: 12px; margin-bottom: 30px; }}
        .content {{ white-space: pre-wrap; line-height: 1.6; }}
    </style>
</head>
<body>
    <h1>{title}</h1>
    <div class='metadata'>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</div>
    <div class='content'>{reportContent}</div>
</body>
</html>";

            return Encoding.UTF8.GetBytes(html);
        }

        private async Task<string> GenerateReportWithAI(string prompt)
        {
            try
            {
                var settings = await _aiRepository.GetSettingsAsync();
                if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
                {
                    return "AI service not configured. Please set up Gemini API key.";
                }

                var apiKey = DecryptApiKey(settings.EncryptedApiKey);
                var geminiRequest = new GeminiRequest
                {
                    ApiKey = apiKey,
                    Model = "gemini-2.0-flash",
                    Prompt = prompt,
                    Temperature = 0.3,
                    MaxOutputTokens = 800
                };

                var response = await _geminiClient.GenerateContentAsync(geminiRequest);
                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report with AI");
                return $"Report generation failed: {ex.Message}";
            }
        }

        private string DecryptApiKey(string encryptedApiKey)
        {
            const string ENCRYPTION_KEY = "HDScheduler2025AiKey32Bytes!@";
            var keyBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ENCRYPTION_KEY));
            var fullCipher = Convert.FromBase64String(encryptedApiKey);
            
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = keyBytes;
            
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
}
