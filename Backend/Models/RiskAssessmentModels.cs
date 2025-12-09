namespace HDScheduler.API.Models
{
    public class PatientRiskAssessment
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<RiskFactor> RiskFactors { get; set; } = new();
        public string Recommendations { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public int TotalSessions { get; set; }
        public int MissedSessions { get; set; }
    }

    public class RiskFactor
    {
        public string Factor { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    public class RiskFactorAnalysis
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public List<RiskFactor> PrimaryFactors { get; set; } = new();
        public List<RiskFactor> SecondaryFactors { get; set; } = new();
        public int TotalRiskScore { get; set; }
        public string DetailedAnalysis { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    public class BatchRiskAssessmentRequest
    {
        public int[] PatientIds { get; set; } = Array.Empty<int>();
    }

    public class RiskFactorsRequest
    {
        public Dictionary<string, object> CustomFactors { get; set; } = new();
    }
}
