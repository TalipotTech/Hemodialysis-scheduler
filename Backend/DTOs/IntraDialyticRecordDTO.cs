namespace HDScheduler.API.DTOs
{
    /// <summary>
    /// DTO matching the actual IntraDialyticRecords table structure
    /// </summary>
    public class IntraDialyticRecordDTO
    {
        public int RecordID { get; set; }
        public int PatientID { get; set; }
        public int ScheduleID { get; set; }
        public string SessionDate { get; set; } = string.Empty;
        public string TimeRecorded { get; set; } = string.Empty;
        public string? BloodPressure { get; set; }
        public int? PulseRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? UFVolume { get; set; }
        public int? VenousPressure { get; set; }
        public int? ArterialPressure { get; set; }
        public int? BloodFlowRate { get; set; }
        public int? DialysateFlowRate { get; set; }
        public decimal? CurrentUFR { get; set; }
        public int? TMPPressure { get; set; }
        public string? Symptoms { get; set; }
        public string? Interventions { get; set; }
        public string? StaffInitials { get; set; }
        public int? RecordedBy { get; set; }  // Changed from string to int - this is the staff ID
        public string? Notes { get; set; }
        public string? CreatedAt { get; set; }
    }
}
