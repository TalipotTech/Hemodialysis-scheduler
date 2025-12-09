namespace HDScheduler.API.Models
{
    // Analytics Dashboard Models
    public class UsageMetrics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public long TotalInputTokens { get; set; }
        public long TotalOutputTokens { get; set; }
        public decimal TotalCost { get; set; }
        public double AverageResponseTime { get; set; }
    }

    public class CostAnalytics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal InputCost { get; set; }
        public decimal OutputCost { get; set; }
        public decimal AverageDailyCost { get; set; }
        public decimal ProjectedMonthlyCost { get; set; }
        public List<DailyCostData> DailyCosts { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public double AverageResponseTime { get; set; }
        public double MedianResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
        public int TotalRequests { get; set; }
        public int FastestRequestTime { get; set; }
        public int SlowestRequestTime { get; set; }
        public DateTime MeasuredAt { get; set; }
    }

    public class UsageTrends
    {
        public int Days { get; set; }
        public List<TrendData> Trends { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public double GrowthRate { get; set; }
    }

    public class SystemHealth
    {
        public string Status { get; set; } = string.Empty;
        public bool AIEnabled { get; set; }
        public bool APIKeyConfigured { get; set; }
        public decimal DailyCostLimit { get; set; }
        public decimal CurrentDailyCost { get; set; }
        public double CostUtilization { get; set; }
        public int TodaysRequests { get; set; }
        public List<string> Warnings { get; set; } = new();
        public DateTime CheckedAt { get; set; }
    }

    public class FeatureUsage
    {
        public int NaturalQueryCount { get; set; }
        public int RiskAssessmentCount { get; set; }
        public int ReportGenerationCount { get; set; }
        public int SavedPromptsCount { get; set; }
        public Dictionary<string, int> PopularQueries { get; set; } = new();
        public DateTime MeasuredAt { get; set; }
    }

    public class CostProjection
    {
        public DateTime ProjectionDate { get; set; }
        public int ProjectionDays { get; set; }
        public decimal CurrentDailyCost { get; set; }
        public decimal ProjectedDailyCost { get; set; }
        public decimal ProjectedTotalCost { get; set; }
        public List<ProjectionData> DailyProjections { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }

    public class DailyCostData
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public int RequestCount { get; set; }
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public int RequestCount { get; set; }
        public decimal Cost { get; set; }
        public long TokensUsed { get; set; }
    }

    public class ProjectionData
    {
        public DateTime Date { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal CumulativeCost { get; set; }
    }
}
