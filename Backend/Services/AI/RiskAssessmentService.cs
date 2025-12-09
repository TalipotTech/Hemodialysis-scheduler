using HDScheduler.API.Models;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Services.AI
{
    public interface IRiskAssessmentService
    {
        Task<PatientRiskAssessment> AssessPatientRiskAsync(int patientId);
        Task<List<PatientRiskAssessment>> BatchAssessRiskAsync(int[] patientIds);
        Task<List<PatientRiskAssessment>> GetHighRiskPatientsAsync(int threshold);
        Task<RiskFactorAnalysis> AnalyzeRiskFactorsAsync(int patientId, Dictionary<string, object>? customFactors);
    }

    public class RiskAssessmentService : IRiskAssessmentService
    {
        private readonly DapperContext _context;
        private readonly IGeminiClient _geminiClient;
        private readonly IAIRepository _aiRepository;
        private readonly ILogger<RiskAssessmentService> _logger;

        public RiskAssessmentService(
            DapperContext context,
            IGeminiClient geminiClient,
            IAIRepository aiRepository,
            ILogger<RiskAssessmentService> logger)
        {
            _context = context;
            _geminiClient = geminiClient;
            _aiRepository = aiRepository;
            _logger = logger;
        }

        public async Task<PatientRiskAssessment> AssessPatientRiskAsync(int patientId)
        {
            using var connection = _context.CreateConnection();

            // Get patient data
            var patient = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT p.PatientID, p.Name, p.Age, p.Gender, p.DryWeight, p.DialyserType, 
                       p.HDCycle, p.HDStartDate, p.IsActive,
                    COUNT(DISTINCT ba.AssignmentID) as TotalSessions,
                    COUNT(DISTINCT CASE WHEN ba.DischargedAt IS NULL AND ba.AssignmentDate < GETDATE() THEN ba.AssignmentID END) as MissedSessions,
                    AVG(CAST(DATEDIFF(day, p.HDStartDate, ba.AssignmentDate) AS FLOAT)) as AvgDaysBetweenSessions
                FROM Patients p
                LEFT JOIN BedAssignments ba ON p.PatientID = ba.PatientID
                WHERE p.PatientID = @PatientId
                GROUP BY p.PatientID, p.Name, p.Age, p.Gender, p.DryWeight, p.DialyserType, 
                         p.HDCycle, p.HDStartDate, p.IsActive",
                new { PatientId = patientId });

            if (patient == null)
            {
                throw new InvalidOperationException($"Patient {patientId} not found");
            }

            // Calculate risk score based on multiple factors
            int riskScore = 0;
            var factors = new List<RiskFactor>();

            // Age risk
            if (patient.Age > 70)
            {
                riskScore += 25;
                factors.Add(new RiskFactor { Factor = "Age", Value = patient.Age.ToString(), Impact = "High", Points = 25 });
            }
            else if (patient.Age > 60)
            {
                riskScore += 15;
                factors.Add(new RiskFactor { Factor = "Age", Value = patient.Age.ToString(), Impact = "Medium", Points = 15 });
            }

            // Missed sessions risk
            int missedSessions = patient.MissedSessions ?? 0;
            if (missedSessions > 3)
            {
                riskScore += 30;
                factors.Add(new RiskFactor { Factor = "Missed Sessions", Value = missedSessions.ToString(), Impact = "High", Points = 30 });
            }
            else if (missedSessions > 1)
            {
                riskScore += 15;
                factors.Add(new RiskFactor { Factor = "Missed Sessions", Value = missedSessions.ToString(), Impact = "Medium", Points = 15 });
            }

            // Patient status risk
            bool isActive = patient.IsActive ?? true;
            if (!isActive)
            {
                riskScore += 25;
                factors.Add(new RiskFactor { Factor = "Patient Status", Value = "Inactive", Impact = "High", Points = 25 });
            }

            // HD Cycle frequency risk
            if (patient.HDCycle == 2) // Twice weekly
            {
                riskScore += 15;
                factors.Add(new RiskFactor { Factor = "HD Frequency", Value = "Twice Weekly", Impact = "Medium", Points = 15 });
            }

            // Use AI for additional analysis
            string aiRecommendation = await GetAIRecommendationAsync(patient, riskScore, factors);

            var assessment = new PatientRiskAssessment
            {
                PatientId = patientId,
                PatientName = patient.Name,
                RiskScore = Math.Min(riskScore, 100),
                RiskLevel = GetRiskLevel(riskScore),
                RiskFactors = factors,
                Recommendations = aiRecommendation,
                AssessmentDate = DateTime.UtcNow,
                TotalSessions = patient.TotalSessions ?? 0,
                MissedSessions = missedSessions
            };

            return assessment;
        }

        public async Task<List<PatientRiskAssessment>> BatchAssessRiskAsync(int[] patientIds)
        {
            var assessments = new List<PatientRiskAssessment>();
            foreach (var patientId in patientIds)
            {
                try
                {
                    var assessment = await AssessPatientRiskAsync(patientId);
                    assessments.Add(assessment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assessing patient {PatientId}", patientId);
                }
            }
            return assessments;
        }

        public async Task<List<PatientRiskAssessment>> GetHighRiskPatientsAsync(int threshold)
        {
            using var connection = _context.CreateConnection();
            var patientIds = await connection.QueryAsync<int>("SELECT PatientID FROM Patients WHERE IsActive = 1");
            
            var allAssessments = await BatchAssessRiskAsync(patientIds.ToArray());
            return allAssessments.Where(a => a.RiskScore >= threshold).OrderByDescending(a => a.RiskScore).ToList();
        }

        public async Task<RiskFactorAnalysis> AnalyzeRiskFactorsAsync(int patientId, Dictionary<string, object>? customFactors)
        {
            var assessment = await AssessPatientRiskAsync(patientId);
            
            var analysis = new RiskFactorAnalysis
            {
                PatientId = patientId,
                PatientName = assessment.PatientName,
                PrimaryFactors = assessment.RiskFactors.Where(f => f.Points >= 20).ToList(),
                SecondaryFactors = assessment.RiskFactors.Where(f => f.Points < 20).ToList(),
                TotalRiskScore = assessment.RiskScore,
                AnalysisDate = DateTime.UtcNow
            };

            // AI-powered detailed analysis
            analysis.DetailedAnalysis = await GetDetailedFactorAnalysisAsync(assessment, customFactors);

            return analysis;
        }

        private string GetRiskLevel(int score)
        {
            if (score >= 80) return "Critical";
            if (score >= 60) return "High";
            if (score >= 40) return "Medium";
            if (score >= 20) return "Low";
            return "Minimal";
        }

        private async Task<string> GetAIRecommendationAsync(dynamic patient, int riskScore, List<RiskFactor> factors)
        {
            try
            {
                var settings = await _aiRepository.GetSettingsAsync();
                if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
                {
                    return GenerateFallbackRecommendation(riskScore);
                }

                var prompt = $@"As a hemodialysis clinical decision support AI, analyze this patient profile and provide specific recommendations:

Patient Profile:
- Name: {patient.Name}
- Age: {patient.Age}
- HD Cycle: {patient.HDCycle}x per week
- Total Sessions: {patient.TotalSessions}
- Missed Sessions: {patient.MissedSessions}
- Patient Status: {(patient.IsActive ? "Active" : "Inactive")}
- Current Risk Score: {riskScore}/100

Risk Factors Identified:
{string.Join("\n", factors.Select(f => $"- {f.Factor}: {f.Value} ({f.Impact} impact)"))}

Provide:
1. Top 3 specific clinical recommendations
2. Immediate actions if risk is high
3. Long-term care suggestions
4. Monitoring priorities

Keep response concise (under 300 words) and actionable.";

                var apiKey = DecryptApiKey(settings.EncryptedApiKey);
                var geminiRequest = new GeminiRequest
                {
                    ApiKey = apiKey,
                    Model = "gemini-2.0-flash",
                    Prompt = prompt,
                    Temperature = 0.3,
                    MaxOutputTokens = 500
                };

                var response = await _geminiClient.GenerateContentAsync(geminiRequest);
                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI recommendation");
                return GenerateFallbackRecommendation(riskScore);
            }
        }

        private async Task<string> GetDetailedFactorAnalysisAsync(PatientRiskAssessment assessment, Dictionary<string, object>? customFactors)
        {
            try
            {
                var settings = await _aiRepository.GetSettingsAsync();
                if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
                {
                    return "AI analysis unavailable. Please review risk factors manually.";
                }

                var customFactorsText = customFactors != null 
                    ? string.Join("\n", customFactors.Select(kv => $"- {kv.Key}: {kv.Value}"))
                    : "None";

                var prompt = $@"Provide a detailed risk factor analysis for this hemodialysis patient:

Patient: {assessment.PatientName}
Overall Risk: {assessment.RiskLevel} ({assessment.RiskScore}/100)

Key Risk Factors:
{string.Join("\n", assessment.RiskFactors.Select(f => $"- {f.Factor}: {f.Value} ({f.Points} points)"))}

Additional Factors:
{customFactorsText}

Analyze:
1. How each factor contributes to overall risk
2. Interactions between risk factors
3. Prioritized mitigation strategies
4. Expected outcomes with interventions

Format as structured analysis with clear sections.";

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
                _logger.LogError(ex, "Error getting detailed analysis");
                return "Detailed analysis unavailable. Please consult clinical team.";
            }
        }

        private string GenerateFallbackRecommendation(int riskScore)
        {
            if (riskScore >= 80)
            {
                return "CRITICAL: Immediate clinical review required. Consider increasing HD frequency and close monitoring.";
            }
            else if (riskScore >= 60)
            {
                return "HIGH RISK: Schedule urgent clinical assessment. Monitor vital signs closely and review medication adherence.";
            }
            else if (riskScore >= 40)
            {
                return "MEDIUM RISK: Regular monitoring recommended. Address identified risk factors and ensure treatment compliance.";
            }
            else
            {
                return "LOW RISK: Continue standard care protocol. Maintain current HD schedule and regular follow-ups.";
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
