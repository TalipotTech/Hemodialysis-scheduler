namespace HDScheduler.API.DTOs
{
    public class IntraDialyticRecordDTO
    {
        public int PatientID { get; set; }
        public int ScheduleID { get; set; }
        public DateTime SessionDate { get; set; }
        public string TimeRecorded { get; set; } = string.Empty;
        public string? BloodPressure { get; set; }
        public int? PulseRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? UFVolume { get; set; }
        public int? VenousPressure { get; set; }
        public string? Notes { get; set; }
    }
}
