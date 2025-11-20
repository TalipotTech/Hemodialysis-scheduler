namespace HDScheduler.API.Models;

public class Slot
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty; // Changed to string for SQLite compatibility
    public string EndTime { get; set; } = string.Empty; // Changed to string for SQLite compatibility
    public int BedCapacity { get; set; }
}

public class BedAssignment
{
    public int AssignmentID { get; set; }
    public int PatientID { get; set; }
    public int SlotID { get; set; }
    public int BedNumber { get; set; }
    public DateTime AssignmentDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
}

public class DailyScheduleResponse
{
    public DateTime Date { get; set; }
    public List<SlotSchedule> Slots { get; set; } = new();
}

public class SlotSchedule
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public List<BedStatus> Beds { get; set; } = new();
}

public class BedStatus
{
    public int BedNumber { get; set; }
    public string Status { get; set; } = "available"; // available, occupied, reserved
    public PatientSummary? Patient { get; set; }
}

public class PatientSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? BloodPressure { get; set; }
}

public class AssignBedRequest
{
    public int PatientID { get; set; }
    public int SlotID { get; set; }
    public int BedNumber { get; set; }
    public DateTime AssignmentDate { get; set; }
}

public class BedAvailability
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public List<int> AvailableBedNumbers { get; set; } = new();
}
