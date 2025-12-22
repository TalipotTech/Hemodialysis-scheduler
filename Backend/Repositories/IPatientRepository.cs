using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IPatientRepository
{
    // Patient CRUD - Demographics only
    Task<List<Patient>> GetAllAsync();
    Task<List<Patient>> GetAllIncludingInactiveAsync(); // Get all patients including inactive
    Task<List<Patient>> GetActiveAsync(); // Get only active patients
    Task<List<Patient>> GetTodayCompletedSessionsAsync(); // Get patients with completed sessions today
    Task<List<Patient>> GetCompletedSessionsByDateRangeAsync(DateTime startDate, DateTime endDate); // Get patients with completed sessions in date range
    Task<Patient?> GetByIdAsync(int patientId);
    Task<Patient?> GetPatientById(int patientId); // Alias for GetByIdAsync
    Task<Patient?> GetByMRNAsync(string mrn); // Get patient by MRN
    Task<List<Patient>> SearchAsync(string searchTerm); // Search by name, phone, MRN
    Task<List<Patient>> SearchPatientsByName(string name); // Search by name only
    Task<int> CreateAsync(Patient patient);
    Task<bool> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int patientId); // Soft delete
    Task<PatientWithLatestSession?> GetPatientWithLatestSessionAsync(int patientId);
    Task<bool> IncrementEquipmentCountersAsync(int patientId); // Auto-increment equipment counters
}
