namespace Backend.DTOs
{
    public class PostDialysisMedicationDTO
    {
        public int MedicationID { get; set; }
        public int ScheduleID { get; set; }
        public int PatientID { get; set; }
        public DateTime? SessionDate { get; set; }
        public string? MedicationName { get; set; }
        public string? Dosage { get; set; }
        public string? Route { get; set; }
        public TimeSpan? GivenTime { get; set; }  // Changed from DateTime to TimeSpan to match TIME column
        public string? GivenBy { get; set; }
    }
}
