using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IStaffRepository
{
    Task<List<Staff>> GetAllAsync();
    Task<List<Staff>> GetActiveAsync();
    Task<Staff?> GetByIdAsync(int staffId);
    Task<List<Staff>> GetByRoleAsync(string role);
    Task<List<Staff>> GetBySlotAsync(int slotId);
    Task<int> CreateAsync(Staff staff);
    Task<bool> UpdateAsync(Staff staff);
    Task<bool> DeleteAsync(int staffId);
    Task<bool> AssignToSlotAsync(int staffId, int? slotId);
    Task<bool> ToggleActiveStatusAsync(int staffId, bool isActive);
}
