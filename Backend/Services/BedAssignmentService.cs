using HDScheduler.API.Models;
using HDScheduler.API.Repositories;

namespace HDScheduler.API.Services;

public interface IBedAssignmentService
{
    Task<int?> GetNextAvailableBedAsync(int slotId, DateTime sessionDate);
    Task<bool> IsBedAvailableAsync(int slotId, int bedNumber, DateTime sessionDate);
}

public class BedAssignmentService : IBedAssignmentService
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly ILogger<BedAssignmentService> _logger;

    public BedAssignmentService(
        IHDScheduleRepository scheduleRepository,
        ILogger<BedAssignmentService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Smart bed assignment algorithm with infection control spacing
    /// </summary>
    public async Task<int?> GetNextAvailableBedAsync(int slotId, DateTime sessionDate)
    {
        try
        {
            // Get all schedules for this slot and date
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var slotSchedules = allSchedules
                .Where(s => s.SlotID == slotId && 
                           s.SessionDate.Date == sessionDate.Date && 
                           !s.IsDischarged && 
                           !s.IsMovedToHistory &&
                           s.BedNumber.HasValue)
                .Select(s => s.BedNumber!.Value)
                .ToHashSet();

            // Maximum beds per slot (can be made configurable)
            const int maxBeds = 11;

            // Smart assignment: Prioritize spacing for infection control
            // Strategy: Assign beds with gaps (1, 3, 5, 7, 9, 11, then 2, 4, 6, 8, 10)
            var preferredOrder = new[] { 1, 3, 5, 7, 9, 11, 2, 4, 6, 8, 10 };

            foreach (var bedNum in preferredOrder)
            {
                if (!slotSchedules.Contains(bedNum))
                {
                    _logger.LogInformation($"✅ Smart bed assignment: Bed {bedNum} for Slot {slotId} on {sessionDate:yyyy-MM-dd}");
                    return bedNum;
                }
            }

            // If all preferred beds taken, find any available bed
            for (int i = 1; i <= maxBeds; i++)
            {
                if (!slotSchedules.Contains(i))
                {
                    return i;
                }
            }

            _logger.LogWarning($"⚠️ All beds occupied for Slot {slotId} on {sessionDate:yyyy-MM-dd}");
            return null; // All beds occupied
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in smart bed assignment");
            return null;
        }
    }

    public async Task<bool> IsBedAvailableAsync(int slotId, int bedNumber, DateTime sessionDate)
    {
        var allSchedules = await _scheduleRepository.GetAllAsync();
        var isOccupied = allSchedules.Any(s =>
            s.SlotID == slotId &&
            s.BedNumber == bedNumber &&
            s.SessionDate.Date == sessionDate.Date &&
            !s.IsDischarged &&
            !s.IsMovedToHistory);

        return !isOccupied;
    }
}
