namespace HDScheduler.API.Models
{
    /// <summary>
    /// AI-generated feature suggestions for future development
    /// </summary>
    public class AIFeatureSuggestion
    {
        public int Id { get; set; }
        public string FeatureTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Autocomplete", "Workflow", "Analytics", "UI/UX", "Integration"
        public string Priority { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string Context { get; set; } = string.Empty; // JSON with contextual data
        public string Reasoning { get; set; } = string.Empty;
        public int ImpactScore { get; set; } // 1-10
        public int ImplementationComplexity { get; set; } // 1-10
        public string EstimatedEffort { get; set; } = string.Empty; // "Hours", "Days", "Weeks"
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty; // AI model version
        public bool IsReviewed { get; set; }
        public bool IsImplemented { get; set; }
        public DateTime? ImplementedAt { get; set; }
        public string? DeveloperNotes { get; set; }
        public int UpvoteCount { get; set; }
    }

    /// <summary>
    /// Form field autocomplete prediction
    /// </summary>
    public class FormAutocomplete
    {
        public string FieldName { get; set; } = string.Empty;
        public object PredictedValue { get; set; } = null!;
        public double Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public List<string> DataSources { get; set; } = new();
    }

    /// <summary>
    /// Session data prediction for autocomplete
    /// </summary>
    public class SessionAutocompleteRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0")]
        public int PatientId { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime SessionDate { get; set; }
        
        public int? SlotId { get; set; }
        public Dictionary<string, object>? PartialData { get; set; }
    }

    public class SessionAutocompleteResponse
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public List<FormAutocomplete> Predictions { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Feature analysis request
    /// </summary>
    public class FeatureAnalysisRequest
    {
        public string AnalysisType { get; set; } = string.Empty; // "UsagePattern", "PainPoint", "Enhancement"
        public string? FeatureArea { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    public class FeatureAnalysisResponse
    {
        public List<AIFeatureSuggestion> Suggestions { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public int TotalSuggestions { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }
}
