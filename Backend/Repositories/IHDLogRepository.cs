using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IHDLogRepository
{
    Task<HDLog?> GetByIdAsync(int hdLogId);
    Task<IEnumerable<HDLog>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<HDLog>> GetByDateAsync(DateTime date);
    Task<IEnumerable<HDLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> CreateAsync(HDLog hdLog);
    Task<bool> UpdateAsync(HDLog hdLog);
    Task<bool> DeleteAsync(int hdLogId);
    
    // Intra-Dialytic Records
    Task<IEnumerable<IntraDialyticRecord>> GetIntraDialyticRecordsAsync(int hdLogId);
    Task<int> AddIntraDialyticRecordAsync(IntraDialyticRecord record);
    Task<bool> UpdateIntraDialyticRecordAsync(IntraDialyticRecord record);
    Task<bool> DeleteIntraDialyticRecordAsync(int monitoringId);
    
    // Post-Dialysis Medications
    Task<IEnumerable<PostDialysisMedication>> GetMedicationsAsync(int hdLogId);
    Task<int> AddMedicationAsync(PostDialysisMedication medication);
    Task<bool> DeleteMedicationAsync(int medicationId);
}
