namespace HDScheduler.API.Models;

public class IntraDialyticRecord
{
    public int MonitoringID { get; set; }
    public int HDLogID { get; set; }
    public TimeSpan RecordTime { get; set; }
    public int? BP_Systolic { get; set; }
    public int? BP_Diastolic { get; set; }
    public int? HeartRate { get; set; }
    public int? BloodFlowRate { get; set; } // Qb in mL/min
    public int? VenousPressure { get; set; } // VP in mmHg
    public int? ArterialPressure { get; set; } // AP in mmHg
    public int? UFRate { get; set; } // in mL/hr
    public decimal? TotalUF { get; set; } // in liters
    public string? InitialStaff { get; set; } // Staff initials
    public string? Comments { get; set; }
    
    // Navigation property
    public HDLog? HDLog { get; set; }
}
