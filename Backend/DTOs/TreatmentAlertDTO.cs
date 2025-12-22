namespace Backend.DTOs
{
    public class TreatmentAlertDTO
    {
        public int AlertID { get; set; }
        public int ScheduleID { get; set; }
        public int PatientID { get; set; }
        public DateTime? SessionDate { get; set; }
        public string? AlertType { get; set; }
        public string? AlertMessage { get; set; }
        public string? Severity { get; set; }
        public string? Resolution { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ResolvedBy { get; set; }
    }
}
