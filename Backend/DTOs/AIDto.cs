namespace HDScheduler.API.DTOs
{
    public class AIScheduleRecommendationRequest
    {
        public int PatientId { get; set; }
        public int? PreferredSlotId { get; set; }
        public DateTime? PreferredDate { get; set; }
    }
    
    public class AIScheduleRecommendation
    {
        public int RecommendedSlotId { get; set; }
        public int RecommendedBedNumber { get; set; }
        public DateTime RecommendedDate { get; set; }
        public double Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public List<string> Factors { get; set; } = new();
        public List<AlternativeRecommendation> Alternatives { get; set; } = new();
        public string ModelUsed { get; set; } = string.Empty;
        public int ProcessingTimeMs { get; set; }
        public decimal Cost { get; set; }
    }
    
    public class AlternativeRecommendation
    {
        public int SlotId { get; set; }
        public int BedNumber { get; set; }
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
    
    public class AISettingsDto
    {
        public bool AIEnabled { get; set; }
        public string AIProvider { get; set; } = "Gemini";
        public decimal DailyCostLimit { get; set; }
        public decimal MonthlyCostLimit { get; set; }
        public bool EnableSchedulingRecommendations { get; set; }
        public bool EnableNaturalLanguageQueries { get; set; }
        public bool EnablePredictiveAnalytics { get; set; }
        public decimal CurrentDailyCost { get; set; }
        public decimal CurrentMonthlyCost { get; set; }
        public int TodayRequestCount { get; set; }
        public int MonthRequestCount { get; set; }
        public bool HasApiKey { get; set; }
    }
    
    public class UpdateAISettingsDto
    {
        public bool? AIEnabled { get; set; }
        public string? AIProvider { get; set; }
        public decimal? DailyCostLimit { get; set; }
        public decimal? MonthlyCostLimit { get; set; }
        public bool? EnableSchedulingRecommendations { get; set; }
        public bool? EnableNaturalLanguageQueries { get; set; }
        public bool? EnablePredictiveAnalytics { get; set; }
        public string? ApiKey { get; set; }
    }
    
    public class AIUsageStatsDto
    {
        public decimal TodayCost { get; set; }
        public decimal MonthCost { get; set; }
        public int TodayRequests { get; set; }
        public int MonthRequests { get; set; }
        public decimal DailyCostLimit { get; set; }
        public decimal MonthlyCostLimit { get; set; }
        public double DailyUsagePercentage { get; set; }
        public double MonthlyUsagePercentage { get; set; }
        public List<UsageByType> UsageBreakdown { get; set; } = new();
        public List<DailyCostTrend> CostTrend { get; set; } = new();
    }
    
    public class UsageByType
    {
        public string RequestType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalCost { get; set; }
        public int AvgProcessingTimeMs { get; set; }
    }
    
    public class DailyCostTrend
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public int RequestCount { get; set; }
    }
    
    public class SlotAvailability
    {
        public int SlotId { get; set; }
        public string TimeDescription { get; set; } = string.Empty;
        public int AvailableBeds { get; set; }
        public int TotalBeds { get; set; }
    }
}
