namespace HDScheduler.API.Models;

public class Staff
{
    public int StaffID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? StaffSpecialization { get; set; }
    public int? AssignedSlot { get; set; }
    public string? AssignedSlotName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
