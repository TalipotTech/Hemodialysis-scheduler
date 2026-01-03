namespace Backend.DTOs;

/// <summary>
/// DTO for updating a session's time slot
/// </summary>
public class UpdateSlotDTO
{
    public int SlotId { get; set; }
}

/// <summary>
/// DTO for rescheduling a session to a new date
/// </summary>
public class RescheduleSessionDTO
{
    public string NewDate { get; set; } = string.Empty; // Format: YYYY-MM-DD
}

/// <summary>
/// DTO for marking a session as missed
/// </summary>
public class MarkMissedAppointmentDTO
{
    public int ScheduleID { get; set; }
    public string MissedReason { get; set; } = string.Empty; // Sick, Emergency, Transportation, Unknown, Other
    public string? MissedNotes { get; set; }
}

/// <summary>
/// DTO for resolving a missed appointment
/// </summary>
public class ResolveMissedAppointmentDTO
{
    public int ScheduleID { get; set; }
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// DTO for missed appointment details
/// </summary>
public class MissedAppointmentDetailsDTO
{
    public int ScheduleID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int? SlotID { get; set; }
    public string? SlotName { get; set; }
    public bool IsMissed { get; set; }
    public string? MissedReason { get; set; }
    public string? MissedNotes { get; set; }
    public DateTime? MissedDateTime { get; set; }
    public string? MissedMarkedBy { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? MissedResolvedDateTime { get; set; }
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// DTO for possible no-show detection
/// </summary>
public class PossibleNoShowDTO
{
    public int ScheduleID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int? SlotID { get; set; }
    public string? SlotName { get; set; }
    public TimeSpan? SlotStartTime { get; set; }
    public int MinutesLate { get; set; }
    public bool IsAutoDetected { get; set; }
}
