using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class HDLogRepository : IHDLogRepository
{
    private readonly DapperContext _context;

    public HDLogRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<HDLog?> GetByIdAsync(int hdLogId)
    {
        const string sql = @"
            SELECT * FROM HDLog WHERE HDLogID = @HDLogID";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<HDLog>(sql, new { HDLogID = hdLogId });
    }

    public async Task<IEnumerable<HDLog>> GetByPatientIdAsync(int patientId)
    {
        const string sql = @"
            SELECT * FROM HDLog 
            WHERE PatientID = @PatientID 
            ORDER BY SessionDate DESC, StartTime DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HDLog>(sql, new { PatientID = patientId });
    }

    public async Task<IEnumerable<HDLog>> GetByDateAsync(DateTime date)
    {
        const string sql = @"
            SELECT hl.*, p.Name as PatientName 
            FROM HDLog hl
            INNER JOIN Patients p ON hl.PatientID = p.PatientID
            WHERE CAST(hl.SessionDate AS DATE) = CAST(@Date AS DATE)
            ORDER BY hl.SlotID, hl.BedNumber";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HDLog>(sql, new { Date = date });
    }

    public async Task<IEnumerable<HDLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT * FROM HDLog 
            WHERE SessionDate BETWEEN @StartDate AND @EndDate
            ORDER BY SessionDate DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HDLog>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<int> CreateAsync(HDLog hdLog)
    {
        const string sql = @"
            INSERT INTO HDLog (
                PatientID, SessionDate, SlotID, BedNumber,
                PreDialysisWeight, PreDialysisBP_Systolic, PreDialysisBP_Diastolic, 
                PreDialysisTemperature, UltrafiltrationVolume, UltrafiltrationRate, DialyzerType,
                PostDialysisWeight, PostDialysisBP_Systolic, PostDialysisBP_Diastolic, 
                PostDialysisHR, AccessBleedingTime, TotalFluidRemoved, PostAccessStatus, 
                SymptomsComplications, StartTime, EndTime, CreatedBy
            )
            VALUES (
                @PatientID, @SessionDate, @SlotID, @BedNumber,
                @PreDialysisWeight, @PreDialysisBP_Systolic, @PreDialysisBP_Diastolic,
                @PreDialysisTemperature, @UltrafiltrationVolume, @UltrafiltrationRate, @DialyzerType,
                @PostDialysisWeight, @PostDialysisBP_Systolic, @PostDialysisBP_Diastolic,
                @PostDialysisHR, @AccessBleedingTime, @TotalFluidRemoved, @PostAccessStatus,
                @SymptomsComplications, @StartTime, @EndTime, @CreatedBy
            );
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, hdLog);
    }

    public async Task<bool> UpdateAsync(HDLog hdLog)
    {
        const string sql = @"
            UPDATE HDLog SET
                PreDialysisWeight = @PreDialysisWeight,
                PreDialysisBP_Systolic = @PreDialysisBP_Systolic,
                PreDialysisBP_Diastolic = @PreDialysisBP_Diastolic,
                PreDialysisTemperature = @PreDialysisTemperature,
                UltrafiltrationVolume = @UltrafiltrationVolume,
                UltrafiltrationRate = @UltrafiltrationRate,
                DialyzerType = @DialyzerType,
                PostDialysisWeight = @PostDialysisWeight,
                PostDialysisBP_Systolic = @PostDialysisBP_Systolic,
                PostDialysisBP_Diastolic = @PostDialysisBP_Diastolic,
                PostDialysisHR = @PostDialysisHR,
                AccessBleedingTime = @AccessBleedingTime,
                TotalFluidRemoved = @TotalFluidRemoved,
                PostAccessStatus = @PostAccessStatus,
                SymptomsComplications = @SymptomsComplications,
                StartTime = @StartTime,
                EndTime = @EndTime,
                UpdatedAt = datetime('now')
            WHERE HDLogID = @HDLogID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, hdLog);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int hdLogId)
    {
        const string sql = "DELETE FROM HDLog WHERE HDLogID = @HDLogID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { HDLogID = hdLogId });
        return rowsAffected > 0;
    }

    // Intra-Dialytic Records
    public async Task<IEnumerable<IntraDialyticRecord>> GetIntraDialyticRecordsAsync(int hdLogId)
    {
        const string sql = @"
            SELECT * FROM IntraDialyticMonitoring 
            WHERE HDLogID = @HDLogID 
            ORDER BY RecordTime";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<IntraDialyticRecord>(sql, new { HDLogID = hdLogId });
    }

    public async Task<int> AddIntraDialyticRecordAsync(IntraDialyticRecord record)
    {
        const string sql = @"
            INSERT INTO IntraDialyticMonitoring (
                HDLogID, RecordTime, BP_Systolic, BP_Diastolic, HeartRate,
                BloodFlowRate, VenousPressure, ArterialPressure, UFRate,
                TotalUF, InitialStaff, Comments
            )
            VALUES (
                @HDLogID, @RecordTime, @BP_Systolic, @BP_Diastolic, @HeartRate,
                @BloodFlowRate, @VenousPressure, @ArterialPressure, @UFRate,
                @TotalUF, @InitialStaff, @Comments
            );
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, record);
    }

    public async Task<bool> UpdateIntraDialyticRecordAsync(IntraDialyticRecord record)
    {
        const string sql = @"
            UPDATE IntraDialyticMonitoring SET
                RecordTime = @RecordTime,
                BP_Systolic = @BP_Systolic,
                BP_Diastolic = @BP_Diastolic,
                HeartRate = @HeartRate,
                BloodFlowRate = @BloodFlowRate,
                VenousPressure = @VenousPressure,
                ArterialPressure = @ArterialPressure,
                UFRate = @UFRate,
                TotalUF = @TotalUF,
                InitialStaff = @InitialStaff,
                Comments = @Comments
            WHERE MonitoringID = @MonitoringID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, record);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteIntraDialyticRecordAsync(int monitoringId)
    {
        const string sql = "DELETE FROM IntraDialyticMonitoring WHERE MonitoringID = @MonitoringID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { MonitoringID = monitoringId });
        return rowsAffected > 0;
    }

    // Post-Dialysis Medications
    public async Task<IEnumerable<PostDialysisMedication>> GetMedicationsAsync(int hdLogId)
    {
        const string sql = @"
            SELECT * FROM PostDialysisMedications 
            WHERE HDLogID = @HDLogID 
            ORDER BY AdministeredAt";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PostDialysisMedication>(sql, new { HDLogID = hdLogId });
    }

    public async Task<int> AddMedicationAsync(PostDialysisMedication medication)
    {
        const string sql = @"
            INSERT INTO PostDialysisMedications (
                HDLogID, MedicationName, Dosage, Route, AdministeredBy
            )
            VALUES (
                @HDLogID, @MedicationName, @Dosage, @Route, @AdministeredBy
            );
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, medication);
    }

    public async Task<bool> DeleteMedicationAsync(int medicationId)
    {
        const string sql = "DELETE FROM PostDialysisMedications WHERE MedicationID = @MedicationID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { MedicationID = medicationId });
        return rowsAffected > 0;
    }
}
