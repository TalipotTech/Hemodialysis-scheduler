namespace HDScheduler.API.Models;

public class ScheduledPatientInfo
{
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int BedNumber { get; set; }
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string SessionTime { get; set; } = string.Empty;
}

public class SlotUtilizationInfo
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public decimal UtilizationRate { get; set; }
}

public class MissedAppointmentInfo
{
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int BedNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}
