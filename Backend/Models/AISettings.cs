using System.Text.Json.Serialization;

namespace HDScheduler.API.Models
{
    /// <summary>
    /// AI system settings stored in database
    /// </summary>
    public class AISettings
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Master toggle for AI features
        /// </summary>
        public bool AIEnabled { get; set; } = false;
        
        /// <summary>
        /// Selected AI provider (Gemini, OpenAI, Claude)
        /// </summary>
        public string AIProvider { get; set; } = "Gemini";
        
        /// <summary>
        /// Daily cost limit in USD
        /// </summary>
        public decimal DailyCostLimit { get; set; } = 10.00m;
        
        /// <summary>
        /// Monthly cost limit in USD
        /// </summary>
        public decimal MonthlyCostLimit { get; set; } = 250.00m;
        
        /// <summary>
        /// Enable AI scheduling recommendations
        /// </summary>
        public bool EnableSchedulingRecommendations { get; set; } = true;
        
        /// <summary>
        /// Enable natural language queries
        /// </summary>
        public bool EnableNaturalLanguageQueries { get; set; } = false;
        
        /// <summary>
        /// Enable predictive analytics
        /// </summary>
        public bool EnablePredictiveAnalytics { get; set; } = false;
        
        /// <summary>
        /// Current daily usage cost
        /// </summary>
        public decimal CurrentDailyCost { get; set; } = 0;
        
        /// <summary>
        /// Current monthly usage cost
        /// </summary>
        public decimal CurrentMonthlyCost { get; set; } = 0;
        
        /// <summary>
        /// Total AI requests today
        /// </summary>
        public int TodayRequestCount { get; set; } = 0;
        
        /// <summary>
        /// Total AI requests this month
        /// </summary>
        public int MonthRequestCount { get; set; } = 0;
        
        /// <summary>
        /// Last reset date for daily stats
        /// </summary>
        public DateTime LastDailyReset { get; set; } = DateTime.UtcNow.Date;
        
        /// <summary>
        /// Last reset date for monthly stats
        /// </summary>
        public DateTime LastMonthlyReset { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        /// <summary>
        /// API key encrypted (for Gemini, OpenAI, etc.)
        /// </summary>
        public string? EncryptedApiKey { get; set; }
        
        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Updated by user ID
        /// </summary>
        public int? UpdatedBy { get; set; }
    }
}
