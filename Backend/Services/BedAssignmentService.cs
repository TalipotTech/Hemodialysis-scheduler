using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Services;

public interface IBedAssignmentService
{
    Task<int?> GetNextAvailableBedAsync(int slotId, DateTime sessionDate, int? excludeScheduleId = null);
    Task<bool> IsBedAvailableAsync(int slotId, int bedNumber, DateTime sessionDate, int? excludeScheduleId = null);
    Task<BedAssignmentValidationResult> ValidateBedAssignmentAsync(int scheduleId, int slotId, int bedNumber, DateTime sessionDate);
    Task<List<BedConflict>> GetBedConflictsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public class BedAssignmentValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ConflictingScheduleId { get; set; }
    public string? ConflictingPatientName { get; set; }
}

public class BedConflict
{
    public int ScheduleId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int? BedNumber { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public string ConflictDetails { get; set; } = string.Empty;
}

public class BedAssignmentService : IBedAssignmentService
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly DapperContext _context;
    private readonly ILogger<BedAssignmentService> _logger;

    public BedAssignmentService(
        IHDScheduleRepository scheduleRepository,
        DapperContext context,
        ILogger<BedAssignmentService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Smart bed assignment algorithm with infection control spacing
    /// </summary>
    public async Task<int?> GetNextAvailableBedAsync(int slotId, DateTime sessionDate, int? excludeScheduleId = null)
    {
        try
        {
            // Get actual max beds for this slot from database
            using var connection = _context.CreateConnection();
            var maxBeds = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MaxBeds FROM Slots WHERE SlotID = @SlotID",
                new { SlotID = slotId });
            
            if (maxBeds == 0) maxBeds = 10; // Default fallback

            // Get all schedules for this slot and date
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var slotSchedules = allSchedules
                .Where(s => s.SlotID == slotId && 
                           s.SessionDate.Date == sessionDate.Date && 
                           !s.IsDischarged && 
                           !s.IsMovedToHistory &&
                           s.BedNumber.HasValue &&
                           (!excludeScheduleId.HasValue || s.ScheduleID != excludeScheduleId.Value))
                .Select(s => s.BedNumber!.Value)
                .ToHashSet();

            _logger.LogInformation($"🛏️ Finding available bed for Slot {slotId} on {sessionDate:yyyy-MM-dd} (MaxBeds: {maxBeds}, Occupied: {slotSchedules.Count})");

            // Smart assignment: Prioritize spacing for infection control
            // For 10 beds: 1, 3, 5, 7, 9, then 2, 4, 6, 8, 10
            var preferredOrder = new List<int>();
            
            // Add odd numbers first
            for (int i = 1; i <= maxBeds; i += 2)
            {
                preferredOrder.Add(i);
            }
            
            // Then add even numbers
            for (int i = 2; i <= maxBeds; i += 2)
            {
                preferredOrder.Add(i);
            }

            foreach (var bedNum in preferredOrder)
            {
                if (!slotSchedules.Contains(bedNum))
                {
                    _logger.LogInformation($"✅ Smart bed assignment: Bed {bedNum} for Slot {slotId} on {sessionDate:yyyy-MM-dd}");
                    return bedNum;
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

    public async Task<bool> IsBedAvailableAsync(int slotId, int bedNumber, DateTime sessionDate, int? excludeScheduleId = null)
    {
        var allSchedules = await _scheduleRepository.GetAllAsync();
        var isOccupied = allSchedules.Any(s =>
            s.SlotID == slotId &&
            s.BedNumber == bedNumber &&
            s.SessionDate.Date == sessionDate.Date &&
            !s.IsDischarged &&
            !s.IsMovedToHistory &&
            (!excludeScheduleId.HasValue || s.ScheduleID != excludeScheduleId.Value));

        return !isOccupied;
    }

    /// <summary>
    /// Validates bed assignment and checks for conflicts
    /// </summary>
    public async Task<BedAssignmentValidationResult> ValidateBedAssignmentAsync(
        int scheduleId, int slotId, int bedNumber, DateTime sessionDate)
    {
        try
        {
            // Check if bed number is within valid range
            using var connection = _context.CreateConnection();
            var maxBeds = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MaxBeds FROM Slots WHERE SlotID = @SlotID",
                new { SlotID = slotId });
            
            if (maxBeds == 0) maxBeds = 10;

            if (bedNumber < 1 || bedNumber > maxBeds)
            {
                return new BedAssignmentValidationResult
                {
                    IsValid = false,
                    Message = $"Bed number {bedNumber} is out of range. Valid range: 1-{maxBeds}"
                };
            }

            // Check for conflicts (another active session on same bed/slot/date)
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var conflict = allSchedules.FirstOrDefault(s =>
                s.ScheduleID != scheduleId &&
                s.SlotID == slotId &&
                s.BedNumber == bedNumber &&
                s.SessionDate.Date == sessionDate.Date &&
                !s.IsDischarged &&
                !s.IsMovedToHistory);

            if (conflict != null)
            {
                _logger.LogWarning($"❌ Bed conflict detected: Bed {bedNumber} in Slot {slotId} on {sessionDate:yyyy-MM-dd} " +
                                  $"is already assigned to {conflict.PatientName} (ScheduleID: {conflict.ScheduleID})");
                
                return new BedAssignmentValidationResult
                {
                    IsValid = false,
                    Message = $"Bed {bedNumber} is already assigned to {conflict.PatientName}",
                    ConflictingScheduleId = conflict.ScheduleID,
                    ConflictingPatientName = conflict.PatientName
                };
            }

            return new BedAssignmentValidationResult
            {
                IsValid = true,
                Message = "Bed assignment is valid"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating bed assignment");
            return new BedAssignmentValidationResult
            {
                IsValid = false,
                Message = $"Validation error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Scans for bed assignment conflicts across date range
    /// </summary>
    public async Task<List<BedConflict>> GetBedConflictsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var conflicts = new List<BedConflict>();
        
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-7);
            var end = endDate ?? DateTime.Today.AddDays(7);

            var allSchedules = await _scheduleRepository.GetAllAsync();
            var relevantSchedules = allSchedules
                .Where(s => s.SessionDate.Date >= start.Date && 
                           s.SessionDate.Date <= end.Date &&
                           !s.IsMovedToHistory)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.SlotID)
                .ThenBy(s => s.BedNumber)
                .ToList();

            // Group by date, slot, and bed to find conflicts
            var grouped = relevantSchedules
                .Where(s => s.BedNumber.HasValue && s.SlotID.HasValue)
                .GroupBy(s => new { s.SessionDate.Date, s.SlotID, s.BedNumber })
                .Where(g => g.Count() > 1); // Multiple schedules on same bed

            using var connection = _context.CreateConnection();

            foreach (var group in grouped)
            {
                var slotName = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT SlotName FROM Slots WHERE SlotID = @SlotID",
                    new { SlotID = group.Key.SlotID });

                // Check if these are actual conflicts (both active/not discharged)
                var activeSchedules = group.Where(s => !s.IsDischarged).ToList();
                
                if (activeSchedules.Count > 1)
                {
                    foreach (var schedule in activeSchedules)
                    {
                        conflicts.Add(new BedConflict
                        {
                            ScheduleId = schedule.ScheduleID,
                            PatientId = schedule.PatientID,
                            PatientName = schedule.PatientName ?? "Unknown",
                            SessionDate = schedule.SessionDate,
                            SlotId = schedule.SlotID ?? 0,
                            SlotName = slotName ?? "Unknown",
                            BedNumber = schedule.BedNumber,
                            ConflictType = "DOUBLE_BOOKING",
                            ConflictDetails = $"Multiple active sessions assigned to Bed {schedule.BedNumber} in {slotName} on {schedule.SessionDate:yyyy-MM-dd}"
                        });
                    }
                }
            }

            // Check for missing bed assignments
            var missingBeds = relevantSchedules
                .Where(s => !s.BedNumber.HasValue && s.SlotID.HasValue && !s.IsDischarged)
                .ToList();

            foreach (var schedule in missingBeds)
            {
                var slotName = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT SlotName FROM Slots WHERE SlotID = @SlotID",
                    new { SlotID = schedule.SlotID });

                conflicts.Add(new BedConflict
                {
                    ScheduleId = schedule.ScheduleID,
                    PatientId = schedule.PatientID,
                    PatientName = schedule.PatientName ?? "Unknown",
                    SessionDate = schedule.SessionDate,
                    SlotId = schedule.SlotID ?? 0,
                    SlotName = slotName ?? "Unknown",
                    BedNumber = null,
                    ConflictType = "MISSING_BED",
                    ConflictDetails = $"Active session has no bed assignment"
                });
            }

            _logger.LogInformation($"🔍 Bed conflict scan complete: Found {conflicts.Count} conflicts between {start:yyyy-MM-dd} and {end:yyyy-MM-dd}");
            
            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning for bed conflicts");
            return conflicts;
        }
    }
}
