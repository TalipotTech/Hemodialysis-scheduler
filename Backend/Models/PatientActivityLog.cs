namespace HDScheduler.API.Models;

/// <summary>
/// Records all patient-related activities for history tracking
/// Tracks: Late arrivals, Missed appointments, Rescheduling, Discharge reasons, etc.
/// </summary>
public class PatientActivityLog
{
    public int ActivityID { get; set; }
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public DateTime ActivityDate { get; set; }
    public string ActivityType { get; set; } = string.Empty; // LATE, MISSED, RESCHEDULED, DISCHARGED, NOTE
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public string? RecordedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // For rescheduling - store old and new dates
    public DateTime? OldDateTime { get; set; }
    public DateTime? NewDateTime { get; set; }
}

/// <summary>
/// DTO for patient history timeline view
/// </summary>
public class PatientHistoryTimeline
{
    public DateTime Date { get; set; }
    public string EventType { get; set; } = string.Empty; // SESSION_COMPLETED, LATE, MISSED, RESCHEDULED, DISCHARGED
    public string Status { get; set; } = string.Empty; // Completed, Missed, Late, Rescheduled, Discharged
    public string? Details { get; set; }
    public string? Reason { get; set; }
    public string? SlotName { get; set; }
    public int? BedNumber { get; set; }
    public string? RecordedBy { get; set; }
}
