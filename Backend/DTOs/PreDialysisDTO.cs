using System.ComponentModel.DataAnnotations;

namespace HDScheduler.API.DTOs;

public class PreDialysisDTO
{
    public int HDLogID { get; set; }
    
    [Required]
    public decimal PreWeight { get; set; }
    
    [Required]
    public decimal PreSBP { get; set; }
    
    [Required]
    public decimal PreDBP { get; set; }
    
    public decimal? PreHR { get; set; }
    public decimal? PreTemp { get; set; }
    public string? AccessSite { get; set; }
    public string? PreAssessmentNotes { get; set; }
}
