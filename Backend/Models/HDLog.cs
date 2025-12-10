namespace HDScheduler.API.Models;

public class HDLog
{
    public int LogID { get; set; }
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public DateTime SessionDate { get; set; }
    
    // Session Status
    public string Status { get; set; } = "Scheduled";
    public bool IsDischarged { get; set; }
    public DateTime? SessionStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public bool IsMovedToHistory { get; set; }
    
    // Phase Tracking
    public string SessionPhase { get; set; } = "PRE_DIALYSIS";
    public DateTime? PreDialysisCompletedAt { get; set; }
    public DateTime? IntraDialysisStartedAt { get; set; }
    public DateTime? PostDialysisStartedAt { get; set; }
    public bool IsPreDialysisLocked { get; set; }
    public bool IsIntraDialysisLocked { get; set; }
    
    // Pre-Dialysis Assessment
    public decimal? PreWeight { get; set; }
    public decimal? PreSBP { get; set; }
    public decimal? PreDBP { get; set; }
    public decimal? PreHR { get; set; }
    public decimal? PreTemp { get; set; }
    public string? AccessSite { get; set; }
    public string? PreAssessmentNotes { get; set; }
    
    // Post-Dialysis Assessment
    public decimal? PostWeight { get; set; }
    public decimal? PostDialysisWeight { get; set; }
    public decimal? PostDialysisSBP { get; set; }
    public decimal? PostDialysisDBP { get; set; }
    public decimal? PostDialysisHR { get; set; }
    public int? AccessBleedingTime { get; set; }
    public decimal? TotalFluidRemoved { get; set; }
    public string? PostAccessStatus { get; set; }
    public string? DischargeNotes { get; set; }
    public string? MedicationsAdministered { get; set; }
    
    // Calculated Values
    public decimal? WeightLoss { get; set; }
    
    // Blood Pressure (Legacy - keeping for compatibility)
    public string? BloodPressurePre { get; set; }
    public string? BloodPressurePost { get; set; }
    
    // Azure SQL HDLogs Table Columns
    public string? PreBP { get; set; }
    public string? PostBP { get; set; }
    public int? PrePulse { get; set; }
    public int? PostPulse { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? TotalUF { get; set; }
    public int? BloodFlowRate { get; set; }
    public int? DialysateFlow { get; set; }
    public string? Remarks { get; set; }
    
    // Temperature
    public decimal? Temperature { get; set; }
    
    // Notes
    public string? Notes { get; set; }
    
    // Metadata
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Patient? Patient { get; set; }
    public HDSchedule? Schedule { get; set; }
}
