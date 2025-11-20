using System.ComponentModel.DataAnnotations;

namespace HDScheduler.API.DTOs;

public class PostDialysisDTO
{
    public int HDLogID { get; set; }
    
    [Required]
    public decimal PostDialysisWeight { get; set; }
    
    [Required]
    public decimal PostDialysisSBP { get; set; }
    
    [Required]
    public decimal PostDialysisDBP { get; set; }
    
    [Required]
    public decimal PostDialysisHR { get; set; }
    
    [Required]
    public int AccessBleedingTime { get; set; }
    
    [Required]
    public decimal TotalFluidRemoved { get; set; }
    
    [Required]
    public string PostAccessStatus { get; set; } = string.Empty;
    
    public string? DischargeNotes { get; set; }
    public string? MedicationsAdministered { get; set; }
}
