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
                h.ScheduleID, h.SlotID, h.BedNumber, h.DialyserType,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT * FROM HDSchedule
                WHERE (PatientID, SessionDate) IN (
                    SELECT PatientID, MAX(SessionDate)
                    FROM HDSchedule
                    GROUP BY PatientID
                )
            ) h ON p.PatientID = h.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE p.IsActive = 1 
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
                h.ScheduleID, h.SlotID, h.BedNumber, h.DialyserType,
                h.AssignedDoctor, h.AssignedNurse, h.IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT * FROM HDSchedule
                WHERE (PatientID, SessionDate) IN (
                    SELECT PatientID, MAX(SessionDate)
                    FROM HDSchedule
                    GROUP BY PatientID
                )
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
                h.ScheduleID, h.SlotID, h.BedNumber, h.DialyserType,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT * FROM HDSchedule
                WHERE PatientID = @PatientID
                ORDER BY SessionDate DESC
                LIMIT 1
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
                h.SlotID, h.BedNumber, h.DialyserType,
                h.AssignedDoctor, h.AssignedNurse, 
                CASE WHEN (h.IsDischarged = 1 OR h.IsMovedToHistory = 1) THEN 1 ELSE 0 END as IsDischarged,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName
            FROM Patients p
            LEFT JOIN (
                SELECT * FROM HDSchedule
                WHERE (PatientID, SessionDate) IN (
                    SELECT PatientID, MAX(SessionDate)
                    FROM HDSchedule
                    GROUP BY PatientID
                )
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
                      HDCycle, HDFrequency, IsActive, CreatedAt, UpdatedAt)
                     VALUES 
                     (@MRN, @Name, @Age, @Gender, @ContactNumber, @EmergencyContact, @Address, @GuardianName,
                      @HDCycle, @HDFrequency, 1, datetime('now'), datetime('now'));
                     SELECT last_insert_rowid()";
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
                     UpdatedAt = datetime('now')
                     WHERE PatientID = @PatientID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, patient);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int patientId)
    {
        // Soft delete
        var query = @"UPDATE Patients SET IsActive = 0, UpdatedAt = datetime('now') 
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
                LIMIT 1
            ) h ON p.PatientID = h.PatientID
            WHERE p.PatientID = @PatientID";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PatientWithLatestSession>(query, new { PatientID = patientId });
    }
}
