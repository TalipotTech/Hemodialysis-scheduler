using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IHDScheduleRepository
{
    // HD Schedule CRUD
    Task<List<HDSchedule>> GetAllAsync();
    Task<HDSchedule?> GetByIdAsync(int scheduleId);
    Task<List<HDSchedule>> GetByPatientIdAsync(int patientId);
    Task<List<HDSchedule>> GetBySlotAsync(int slotId);
    Task<List<HDSchedule>> GetBySlotAndDateAsync(int slotId, DateTime date);
    Task<List<HDSchedule>> GetActiveAsync(); // Not discharged and not moved to history
    Task<List<HDSchedule>> GetTodaySchedulesAsync();
    Task<List<HDSchedule>> GetHistorySessionsAsync(); // Get sessions moved to history
    Task<List<HDSchedule>> GetFutureScheduledSessionsAsync(); // Get future scheduled sessions for Bed Schedule
    Task<bool> MoveCompletedSessionsToHistoryAsync(); // Auto-move completed slots to history
    Task<int> CreateAsync(HDSchedule schedule);
    Task<bool> UpdateAsync(HDSchedule schedule);
    Task<bool> PartialUpdateAsync(int scheduleId, Dictionary<string, object> updates);
    Task<bool> DischargeAsync(int scheduleId);
    Task<bool> DeleteAsync(int scheduleId);
    
    // Equipment tracking
    Task<(int dialyserCount, int bloodTubingCount)> GetLatestEquipmentCountsAsync(int patientId);
    
    // Related tables
    Task<int> CreateHDLogAsync(HDLog log);
    Task<int> CreateIntraDialyticRecordAsync(object record);
    Task<IEnumerable<object>> GetIntraDialyticRecordsAsync(int scheduleId);
    Task<bool> DeleteIntraDialyticRecordAsync(int recordId);
    Task<int> CreatePostDialysisMedicationAsync(object medication);
}
