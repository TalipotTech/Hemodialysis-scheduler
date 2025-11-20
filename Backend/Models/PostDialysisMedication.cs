namespace HDScheduler.API.Models;

public class PostDialysisMedication
{
    public int MedicationID { get; set; }
    public int HDLogID { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Route { get; set; } // SC/IV/PO
    public int AdministeredBy { get; set; }
    public DateTime AdministeredAt { get; set; }
    
    // Navigation properties
    public HDLog? HDLog { get; set; }
    public User? AdministeredByUser { get; set; }
}
