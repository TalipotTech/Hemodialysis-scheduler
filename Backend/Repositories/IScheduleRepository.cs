using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IScheduleRepository
{
    Task<List<Slot>> GetAllSlotsAsync();
    Task<Slot?> GetSlotByIdAsync(int slotId);
    Task<List<BedAssignment>> GetAssignmentsByDateAsync(DateTime date);
    Task<List<BedAssignment>> GetActiveAssignmentsBySlotAsync(int slotId, DateTime date);
    Task<int> CreateAssignmentAsync(BedAssignment assignment);
    Task<bool> DischargeAssignmentAsync(int patientId);
    Task<BedAvailability> GetBedAvailabilityAsync(int slotId, DateTime date);
}
