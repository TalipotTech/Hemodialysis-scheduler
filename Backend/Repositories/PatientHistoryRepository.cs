using HDScheduler.API.Data;
using HDScheduler.API.Models;
using Dapper;

namespace HDScheduler.API.Repositories;

public interface IPatientHistoryRepository
{
    Task<PatientTreatmentHistory?> GetPatientHistoryAsync(int patientId);
    Task<DetailedSessionView?> GetSessionDetailsAsync(int scheduleId);
    Task<PatientVitalTrends?> GetVitalTrendsAsync(int patientId, int? lastNSessions = null);
    Task<TreatmentStatistics?> GetTreatmentStatisticsAsync(int patientId);
}

public class PatientHistoryRepository : IPatientHistoryRepository
{
    private readonly DapperContext _context;

    public PatientHistoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<PatientTreatmentHistory?> GetPatientHistoryAsync(int patientId)
    {
        using var connection = _context.CreateConnection();
        
        // Get patient info
        var patientQuery = "SELECT * FROM Patients WHERE PatientID = @PatientID";
        var patient = await connection.QueryFirstOrDefaultAsync<Patient>(patientQuery, new { PatientID = patientId });
        
        if (patient == null) return null;

        // Get all sessions with related data - include ALL fields from HDSchedule
        var sessionsQuery = @"
            SELECT 
                h.*, 
                p.Name as PatientName,
                s.SlotName,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName,
                l.PreWeight, l.PostWeight, l.WeightLoss, l.BloodPressurePre, l.BloodPressurePost,
                l.Temperature, l.Notes,
                (SELECT COUNT(*) FROM IntraDialyticRecords WHERE ScheduleID = h.ScheduleID) as IntraDialyticRecordsCount,
                (SELECT COUNT(*) FROM PostDialysisMedications WHERE ScheduleID = h.ScheduleID) as MedicationsCount
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Slots s ON h.SlotID = s.SlotID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.PatientID = @PatientID
            ORDER BY h.SessionDate DESC";

        var sessions = (await connection.QueryAsync<TreatmentSessionSummary>(sessionsQuery, new { PatientID = patientId })).ToList();

        // Get statistics
        var statistics = await GetTreatmentStatisticsAsync(patientId);

        return new PatientTreatmentHistory
        {
            PatientInfo = patient,
            Sessions = sessions,
            Statistics = statistics
        };
    }

    public async Task<DetailedSessionView?> GetSessionDetailsAsync(int scheduleId)
    {
        using var connection = _context.CreateConnection();
        
        // Get session summary with all HDSchedule fields
        var sessionQuery = @"
            SELECT 
                h.*, 
                p.Name as PatientName,
                s.SlotName,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName,
                l.PreWeight, l.PostWeight, l.WeightLoss, l.BloodPressurePre, l.BloodPressurePost,
                l.Temperature, l.Notes
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Slots s ON h.SlotID = s.SlotID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.ScheduleID = @ScheduleID";

        var session = await connection.QueryFirstOrDefaultAsync<TreatmentSessionSummary>(sessionQuery, new { ScheduleID = scheduleId });
        
        if (session == null) return null;

        // Get intra-dialytic records - use empty list if table doesn't exist
        List<IntraDialyticRecord> intraRecords = new List<IntraDialyticRecord>();
        try
        {
            var intraRecordsQuery = @"
                SELECT * FROM IntraDialyticRecords 
                WHERE ScheduleID = @ScheduleID 
                ORDER BY TimeRecorded";
            intraRecords = (await connection.QueryAsync<IntraDialyticRecord>(intraRecordsQuery, new { ScheduleID = scheduleId })).ToList();
        }
        catch (Exception)
        {
            // Table might not exist, use empty list
        }

        // Get medications - use empty list if table doesn't exist or has no data
        List<PostDialysisMedication> medications = new List<PostDialysisMedication>();
        try
        {
            var medicationsQuery = @"
                SELECT 
                    MedicationID, 
                    0 as HDLogID,
                    MedicationName, 
                    Dosage, 
                    Route, 
                    1 as AdministeredBy, 
                    AdministeredAt
                FROM PostDialysisMedications 
                WHERE ScheduleID = @ScheduleID 
                ORDER BY AdministeredAt";
            medications = (await connection.QueryAsync<PostDialysisMedication>(medicationsQuery, new { ScheduleID = scheduleId })).ToList();
        }
        catch (Exception)
        {
            // Table might not exist or have different structure, use empty list
        }

        // Get session log
        var logQuery = "SELECT * FROM HDLogs WHERE ScheduleID = @ScheduleID";
        var log = await connection.QueryFirstOrDefaultAsync<HDLog>(logQuery, new { ScheduleID = scheduleId });

        return new DetailedSessionView
        {
            SessionInfo = session,
            IntraDialyticRecords = intraRecords,
            Medications = medications,
            SessionLog = log
        };
    }

    public async Task<PatientVitalTrends?> GetVitalTrendsAsync(int patientId, int? lastNSessions = null)
    {
        using var connection = _context.CreateConnection();
        
        var limitClause = lastNSessions.HasValue ? $"LIMIT {lastNSessions.Value}" : "";
        
        var query = $@"
            SELECT 
                h.SessionDate,
                h.PreWeight,
                h.PostWeight,
                CASE 
                    WHEN h.PreWeight IS NOT NULL AND h.PostWeight IS NOT NULL 
                    THEN h.PreWeight - h.PostWeight 
                    ELSE NULL 
                END as WeightLoss,
                h.UFGoal,
                h.PreBPSitting as BloodPressurePre,
                CASE 
                    WHEN h.PostSBP IS NOT NULL AND h.PostDBP IS NOT NULL 
                    THEN h.PostSBP || '/' || h.PostDBP 
                    ELSE NULL 
                END as BloodPressurePost
            FROM HDSchedule h
            WHERE h.PatientID = @PatientID
            ORDER BY h.SessionDate DESC
            {limitClause}";

        var data = await connection.QueryAsync<dynamic>(query, new { PatientID = patientId });

        var trends = new PatientVitalTrends();

        foreach (var row in data)
        {
            // Handle SessionDate which might be string or DateTime
            DateTime sessionDate;
            if (row.SessionDate is string sessionDateStr)
            {
                sessionDate = DateTime.Parse(sessionDateStr);
            }
            else
            {
                sessionDate = (DateTime)row.SessionDate;
            }
            
            // Weight trends
            if (row.PreWeight != null)
            {
                trends.WeightTrend.Add(new VitalTrend 
                { 
                    SessionDate = sessionDate, 
                    Value = row.PreWeight, 
                    Label = "Pre-Weight" 
                });
            }

            if (row.WeightLoss != null)
            {
                trends.WeightLossTrend.Add(new VitalTrend 
                { 
                    SessionDate = sessionDate, 
                    Value = row.WeightLoss, 
                    Label = "Weight Loss" 
                });
            }

            if (row.UFGoal != null)
            {
                trends.UFGoalTrend.Add(new VitalTrend 
                { 
                    SessionDate = sessionDate, 
                    Value = row.UFGoal, 
                    Label = "UF Goal" 
                });
            }

            // BP trends
            trends.BloodPressureTrend.Add(new BPTrend
            {
                SessionDate = sessionDate,
                PreBP = row.BloodPressurePre,
                PostBP = row.BloodPressurePost
            });
        }

        return trends;
    }

    public async Task<TreatmentStatistics?> GetTreatmentStatisticsAsync(int patientId)
    {
        using var connection = _context.CreateConnection();
        
        var query = @"
            SELECT 
                COUNT(h.ScheduleID) as TotalSessions,
                MIN(h.SessionDate) as FirstSessionDate,
                MAX(h.SessionDate) as LastSessionDate,
                AVG(l.WeightLoss) as AverageWeightLoss,
                AVG(l.PreWeight) as AveragePreWeight,
                AVG(l.PostWeight) as AveragePostWeight,
                (SELECT COUNT(*) FROM PostDialysisMedications m 
                 INNER JOIN HDSchedule s ON m.ScheduleID = s.ScheduleID 
                 WHERE s.PatientID = @PatientID) as TotalMedicationsAdministered
            FROM HDSchedule h
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.PatientID = @PatientID";

        var stats = await connection.QueryFirstOrDefaultAsync<TreatmentStatistics>(query, new { PatientID = patientId });

        if (stats != null)
        {
            // Get common medications
            var medicationsQuery = @"
                SELECT m.MedicationName, COUNT(*) as Count
                FROM PostDialysisMedications m
                INNER JOIN HDSchedule s ON m.ScheduleID = s.ScheduleID
                WHERE s.PatientID = @PatientID
                GROUP BY m.MedicationName
                ORDER BY Count DESC
                LIMIT 5";

            var commonMeds = await connection.QueryAsync<string>(medicationsQuery, new { PatientID = patientId });
            stats.CommonMedications = commonMeds.ToList();
        }

        return stats;
    }
}
