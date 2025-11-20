namespace HDScheduler.API.Models;

/// <summary>
/// Equipment usage limits and alert configuration
/// </summary>
public class EquipmentUsageLimits
{
    public const int DIALYSER_MAX_REUSE = 7;
    public const int BLOOD_TUBING_MAX_REUSE = 12;
    
    // Warning thresholds (when to start showing warnings)
    public const int DIALYSER_WARNING_THRESHOLD = 6; // Warn at 6 uses (85% of limit)
    public const int BLOOD_TUBING_WARNING_THRESHOLD = 10; // Warn at 10 uses (83% of limit)
}

/// <summary>
/// Equipment usage alert for tracking and displaying warnings
/// </summary>
public class EquipmentUsageAlert
{
    public int AlertID { get; set; }
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public string EquipmentType { get; set; } = string.Empty; // "Dialyser" or "BloodTubing"
    public int CurrentUsageCount { get; set; }
    public int MaxUsageLimit { get; set; }
    public string Severity { get; set; } = string.Empty; // "Warning", "Critical", "Expired"
    public string AlertMessage { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Patient? Patient { get; set; }
}

/// <summary>
/// Response model for equipment usage status
/// </summary>
public class EquipmentUsageStatus
{
    public string EquipmentType { get; set; } = string.Empty;
    public int CurrentUsageCount { get; set; }
    public int MaxUsageLimit { get; set; }
    public int RemainingUses { get; set; }
    public double UsagePercentage { get; set; }
    public string Status { get; set; } = string.Empty; // "OK", "Warning", "Critical", "Expired"
    public string? Message { get; set; }
    public bool RequiresReplacement { get; set; }
}

/// <summary>
/// Request to acknowledge an equipment alert
/// </summary>
public class AcknowledgeAlertRequest
{
    public int AlertID { get; set; }
    public string AcknowledgedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
