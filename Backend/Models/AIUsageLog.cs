namespace HDScheduler.API.Models
{
    /// <summary>
    /// Tracks AI usage for cost monitoring and analytics
    /// </summary>
    public class AIUsageLog
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Timestamp of the request
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// AI provider used (Gemini, OpenAI, Claude)
        /// </summary>
        public string Provider { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of request (Scheduling, Query, Analytics, etc.)
        /// </summary>
        public string RequestType { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of tokens used in input
        /// </summary>
        public int InputTokens { get; set; }
        
        /// <summary>
        /// Number of tokens used in output
        /// </summary>
        public int OutputTokens { get; set; }
        
        /// <summary>
        /// Total tokens used
        /// </summary>
        public int TotalTokens { get; set; }
        
        /// <summary>
        /// Cost of this request in USD
        /// </summary>
        public decimal Cost { get; set; }
        
        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        public int ProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; } = true;
        
        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// User ID who made the request
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// Additional metadata in JSON format
        /// </summary>
        public string? Metadata { get; set; }
    }
}
