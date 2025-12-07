using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly DapperContext _context;

    public PatientRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<Patient>> GetAllAsync()
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.DryWeight, p.HDCycle, p.HDFrequency, p.HDStartDate, p.DialyserType,
                p.DialyserModel, p.PrescribedDuration, p.PrescribedBFR, p.DialysatePrescription,
                p.DialyserCount, p.BloodTubingCount, p.TotalDialysisCompleted,
                p.DialysersPurchased, p.BloodTubingPurchased,
                h.ScheduleID, h.SlotID, h.BedNumber,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT h1.* FROM HDSchedule h1
                INNER JOIN (
                    SELECT PatientID, MAX(SessionDate) as MaxSessionDate
                    FROM HDSchedule
                    GROUP BY PatientID
                ) h2 ON h1.PatientID = h2.PatientID AND h1.SessionDate = h2.MaxSessionDate
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE p.IsActive = 1 
            ORDER BY p.CreatedAt DESC";
        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<Patient>(query);
        return patients.ToList();
    }

    public async Task<List<Patient>> GetAllIncludingInactiveAsync()
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.DryWeight, p.HDCycle, p.HDFrequency, p.HDStartDate, p.DialyserType,
                p.DialyserModel, p.PrescribedDuration, p.PrescribedBFR, p.DialysatePrescription,
                p.DialyserCount, p.BloodTubingCount, p.TotalDialysisCompleted,
                p.DialysersPurchased, p.BloodTubingPurchased,
                h.ScheduleID, h.SlotID, h.BedNumber,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT h1.* FROM HDSchedule h1
                INNER JOIN (
                    SELECT PatientID, MAX(SessionDate) as MaxSessionDate
                    FROM HDSchedule
                    GROUP BY PatientID
                ) h2 ON h1.PatientID = h2.PatientID AND h1.SessionDate = h2.MaxSessionDate
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            ORDER BY p.CreatedAt DESC";
        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<Patient>(query);
        return patients.ToList();
    }

    public async Task<Patient?> GetByMRNAsync(string mrn)
    {
        var query = "SELECT * FROM Patients WHERE MRN = @MRN";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { MRN = mrn });
    }

    public async Task<List<Patient>> GetActiveAsync()
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.DryWeight, p.HDCycle, p.HDFrequency, p.HDStartDate, p.DialyserType,
                p.DialyserModel, p.PrescribedDuration, p.PrescribedBFR, p.DialysatePrescription,
                p.DialyserCount, p.BloodTubingCount, p.TotalDialysisCompleted,
                p.DialysersPurchased, p.BloodTubingPurchased,
                h.ScheduleID, h.SlotID, h.BedNumber,
                h.AssignedDoctor, h.AssignedNurse, h.IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT h1.* FROM HDSchedule h1
                INNER JOIN (
                    SELECT PatientID, MAX(SessionDate) as MaxSessionDate
                    FROM HDSchedule
                    GROUP BY PatientID
                ) h2 ON h1.PatientID = h2.PatientID AND h1.SessionDate = h2.MaxSessionDate
                WHERE h1.IsDischarged = 0
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE p.IsActive = 1 
            ORDER BY p.CreatedAt DESC";
        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<Patient>(query);
        return patients.ToList();
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.DryWeight, p.HDCycle, p.HDFrequency, p.HDStartDate, p.DialyserType,
                p.DialyserModel, p.PrescribedDuration, p.PrescribedBFR, p.DialysatePrescription,
                p.DialyserCount, p.BloodTubingCount, p.TotalDialysisCompleted,
                p.DialysersPurchased, p.BloodTubingPurchased,
                h.ScheduleID, h.SlotID, h.BedNumber,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT TOP 1 * FROM HDSchedule
                WHERE PatientID = @PatientID
                ORDER BY SessionDate DESC
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE p.PatientID = @PatientID";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { PatientID = patientId });
    }

    public async Task<List<Patient>> SearchAsync(string searchTerm)
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.DryWeight, p.HDCycle, p.HDFrequency, p.HDStartDate, p.DialyserType,
                p.DialyserModel, p.PrescribedDuration, p.PrescribedBFR, p.DialysatePrescription,
                p.DialyserCount, p.BloodTubingCount, p.TotalDialysisCompleted,
                p.DialysersPurchased, p.BloodTubingPurchased,
                h.SlotID, h.BedNumber,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT h1.* FROM HDSchedule h1
                INNER JOIN (
                    SELECT PatientID, MAX(SessionDate) as MaxSessionDate
                    FROM HDSchedule
                    GROUP BY PatientID
                ) h2 ON h1.PatientID = h2.PatientID AND h1.SessionDate = h2.MaxSessionDate
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE p.IsActive = 1 
            AND (p.Name LIKE @Search 
                 OR p.ContactNumber LIKE @Search 
                 OR p.EmergencyContact LIKE @Search
                 OR p.MRN LIKE @Search)
            ORDER BY p.Name";
        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<Patient>(query, new { Search = $"%{searchTerm}%" });
        return patients.ToList();
    }

    public async Task<int> CreateAsync(Patient patient)
    {
        var query = @"INSERT INTO Patients 
                     (MRN, Name, Age, Gender, ContactNumber, EmergencyContact, Address, GuardianName, 
                      HDCycle, HDFrequency, DryWeight, HDStartDate, DialyserType, 
                      DialyserModel, PrescribedDuration, PrescribedBFR, DialysatePrescription,
                      DialyserCount, BloodTubingCount, TotalDialysisCompleted,
                      DialysersPurchased, BloodTubingPurchased,
                      IsActive, CreatedAt, UpdatedAt)
                     VALUES 
                     (@MRN, @Name, @Age, @Gender, @ContactNumber, @EmergencyContact, @Address, @GuardianName,
                      @HDCycle, @HDFrequency, @DryWeight, @HDStartDate, @DialyserType,
                      @DialyserModel, @PrescribedDuration, @PrescribedBFR, @DialysatePrescription,
                      @DialyserCount, @BloodTubingCount, @TotalDialysisCompleted,
                      @DialysersPurchased, @BloodTubingPurchased,
                      1, GETUTCDATE(), GETUTCDATE());
                     SELECT CAST(SCOPE_IDENTITY() AS INT)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, patient);
    }

    public async Task<bool> UpdateAsync(Patient patient)
    {
        var query = @"UPDATE Patients SET 
                     MRN = @MRN,
                     Name = @Name, 
                     Age = @Age, 
                     Gender = @Gender,
                     ContactNumber = @ContactNumber,
                     EmergencyContact = @EmergencyContact,
                     Address = @Address,
                     GuardianName = @GuardianName,
                     HDCycle = @HDCycle,
                     HDFrequency = @HDFrequency,
                     DryWeight = @DryWeight,
                     HDStartDate = @HDStartDate,
                     DialyserType = @DialyserType,
                     DialyserModel = @DialyserModel,
                     PrescribedDuration = @PrescribedDuration,
                     PrescribedBFR = @PrescribedBFR,
                     DialysatePrescription = @DialysatePrescription,
                     DialyserCount = @DialyserCount,
                     BloodTubingCount = @BloodTubingCount,
                     TotalDialysisCompleted = @TotalDialysisCompleted,
                     DialysersPurchased = @DialysersPurchased,
                     BloodTubingPurchased = @BloodTubingPurchased,
                     IsActive = @IsActive,
                     UpdatedAt = GETUTCDATE()
                     WHERE PatientID = @PatientID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, patient);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int patientId)
    {
        // Soft delete
        var query = @"UPDATE Patients SET IsActive = 0, UpdatedAt = GETUTCDATE() 
                     WHERE PatientID = @PatientID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { PatientID = patientId });
        return affected > 0;
    }

    public async Task<PatientWithLatestSession?> GetPatientWithLatestSessionAsync(int patientId)
    {
        var query = @"
            SELECT 
                p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
                p.ContactNumber, p.EmergencyContact, p.Address, p.GuardianName,
                h.ScheduleID as LatestScheduleID,
                h.SessionDate as LastSessionDate,
                h.SlotID as LastSlotID,
                h.BedNumber as LastBedNumber,
                h.IsDischarged as LastSessionDischarged
            FROM Patients p
            LEFT JOIN (
                SELECT * FROM HDSchedule 
                WHERE PatientID = @PatientID 
                ORDER BY SessionDate DESC 
                OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
            ) h ON p.PatientID = h.PatientID
            WHERE p.PatientID = @PatientID";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PatientWithLatestSession>(query, new { PatientID = patientId });
    }

    public async Task<bool> IncrementEquipmentCountersAsync(int patientId)
    {
        var query = @"UPDATE Patients SET 
                     DialyserCount = DialyserCount + 1,
                     BloodTubingCount = BloodTubingCount + 1,
                     TotalDialysisCompleted = TotalDialysisCompleted + 1,
                     UpdatedAt = GETUTCDATE()
                     WHERE PatientID = @PatientID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { PatientID = patientId });
        return affected > 0;
    }
}
