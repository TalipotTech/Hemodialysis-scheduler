using HDScheduler.API.Data;
using HDScheduler.API.Models;
using Dapper;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<PatientHistoryRepository> _logger;

    public PatientHistoryRepository(DapperContext context, ILogger<PatientHistoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PatientTreatmentHistory?> GetPatientHistoryAsync(int patientId)
    {
        using var connection = _context.CreateConnection();
        
        // Get patient info with all fields including purchase tracking
        var patientQuery = @"
            SELECT PatientID, MRN, Name, Age, Gender, ContactNumber, EmergencyContact, Address, GuardianName,
                   HDCycle, HDFrequency, DryWeight, HDStartDate, DialyserType,
                   DialyserCount, BloodTubingCount, TotalDialysisCompleted,
                   DialysersPurchased, BloodTubingPurchased,
                   IsActive, CreatedAt, UpdatedAt
            FROM Patients 
            WHERE PatientID = @PatientID";
        var patient = await connection.QueryFirstOrDefaultAsync<Patient>(patientQuery, new { PatientID = patientId });
        
        if (patient == null) return null;

        // Get all sessions with related data - only select columns that exist in HDSchedule table
        // Show only completed/discharged sessions for patient history
        var sessionsQuery = @"
            SELECT 
                h.ScheduleID, h.SessionDate, h.PatientID, h.SlotID, h.BedNumber,
                h.DryWeight, h.HDStartDate, h.HDCycle, h.WeightGain,
                h.DialyserType, h.DialyserModel, h.DialyserReuseCount, h.BloodTubingReuse, h.HDUnitNumber,
                h.PrescribedDuration, h.UFGoal, h.DialysatePrescription, h.PrescribedBFR,
                h.AnticoagulationType, h.HeparinDose, h.SyringeType, h.BolusDose, h.HeparinInfusionRate,
                h.AccessType, h.AccessLocation,
                h.BloodPressure, h.Symptoms, h.BloodTestDone,
                h.SessionStatus, h.TreatmentStartTime, h.TreatmentCompletionTime, h.DischargeTime,
                h.IsDischarged, h.IsMovedToHistory, h.CreatedAt, h.UpdatedAt,
                h.AssignedDoctor, h.AssignedNurse, h.CreatedByStaffName, h.CreatedByStaffRole,
                p.Name as PatientName,
                s.SlotName,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName,
                l.PreWeight, l.PostWeight, l.PreBP, l.PostBP, l.PrePulse, l.PostPulse,
                l.StartTime, l.EndTime, l.TotalUF, l.BloodFlowRate, l.DialysateFlow, l.Remarks,
                (SELECT COUNT(*) FROM IntraDialyticRecords WHERE ScheduleID = h.ScheduleID) as IntraDialyticRecordsCount,
                (SELECT COUNT(*) FROM PostDialysisMedications WHERE ScheduleID = h.ScheduleID) as MedicationsCount
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Slots s ON h.SlotID = s.SlotID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.PatientID = @PatientID
                AND h.IsDischarged = 1
            ORDER BY h.SessionDate DESC";

        var sessions = (await connection.QueryAsync<TreatmentSessionSummary>(sessionsQuery, new { PatientID = patientId })).ToList();
        
        _logger.LogInformation("Patient {PatientId} history: Found {SessionCount} sessions. Today is {Today}", 
            patientId, sessions.Count, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        
        foreach (var session in sessions.Take(5))
        {
            _logger.LogInformation("  Session {ScheduleId}: Date={SessionDate}, Status={SessionStatus}, Discharged={IsDischarged}", 
                session.ScheduleID, session.SessionDate, session.SessionStatus, session.IsDischarged);
        }

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
                h.ScheduleID, h.SessionDate, h.PatientID, h.SlotID, h.BedNumber,
                h.DryWeight, h.HDStartDate, h.HDCycle, h.WeightGain,
                h.DialyserType, h.DialyserModel, h.DialyserReuseCount, h.BloodTubingReuse, h.HDUnitNumber,
                h.PrescribedDuration, h.UFGoal, h.DialysatePrescription, h.PrescribedBFR,
                h.AnticoagulationType, h.HeparinDose, h.SyringeType, h.BolusDose, h.HeparinInfusionRate,
                h.AccessType, h.AccessLocation,
                h.BloodPressure, h.Symptoms, h.BloodTestDone,
                h.SessionStatus, h.TreatmentStartTime, h.TreatmentCompletionTime, h.DischargeTime,
                h.IsDischarged, h.IsMovedToHistory, h.CreatedAt, h.UpdatedAt,
                h.AssignedDoctor, h.AssignedNurse, h.CreatedByStaffName, h.CreatedByStaffRole,
                p.Name as PatientName, p.MRN, p.Age, p.Gender,
                s.SlotName,
                d.Name as AssignedDoctorName,
                n.Name as AssignedNurseName,
                l.PreWeight, l.PostWeight, l.PreBP, l.PostBP, l.PrePulse, l.PostPulse,
                l.StartTime, l.EndTime, l.TotalUF, l.BloodFlowRate, l.DialysateFlow, l.Remarks
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Slots s ON h.SlotID = s.SlotID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.ScheduleID = @ScheduleID";

        var session = await connection.QueryFirstOrDefaultAsync<TreatmentSessionSummary>(sessionQuery, new { ScheduleID = scheduleId });
        
        if (session == null) return null;

        // Get intra-dialytic records - only query columns that actually exist in the table
        List<object> intraRecords = new List<object>();
        try
        {
            // First check total count in table for debugging
            var totalCountQuery = "SELECT COUNT(*) FROM IntraDialyticRecords";
            var totalCount = await connection.ExecuteScalarAsync<int>(totalCountQuery);
            Console.WriteLine($"Total monitoring records in database: {totalCount}");
            
            // Check if any records exist for this schedule
            var scheduleCountQuery = "SELECT COUNT(*) FROM IntraDialyticRecords WHERE ScheduleID = @ScheduleID";
            var scheduleCount = await connection.ExecuteScalarAsync<int>(scheduleCountQuery, new { ScheduleID = scheduleId });
            Console.WriteLine($"Monitoring records for ScheduleID {scheduleId}: {scheduleCount}");
            
            var intraRecordsQuery = @"
                SELECT 
                    RecordID, ScheduleID, PatientID, SessionDate, RecordTime,
                    BP, Pulse, UFRate, VenousPressure, BloodFlow,
                    Symptoms, Intervention, CreatedAt
                FROM IntraDialyticRecords 
                WHERE ScheduleID = @ScheduleID 
                ORDER BY RecordTime";
            var records = await connection.QueryAsync(intraRecordsQuery, new { ScheduleID = scheduleId });
            intraRecords = records.Cast<object>().ToList();
            Console.WriteLine($"✓ Loaded {intraRecords.Count} intra-dialytic records for ScheduleID: {scheduleId}");
            
            if (intraRecords.Count > 0)
            {
                Console.WriteLine($"First record: {System.Text.Json.JsonSerializer.Serialize(intraRecords[0])}");
            }
        }
        catch (Exception ex)
        {
            // Table might not exist, use empty list
            Console.WriteLine($"❌ Error loading intra-dialytic records: {ex.Message}");
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
        
        var topClause = lastNSessions.HasValue ? $"TOP {lastNSessions.Value}" : "";
        
        var query = $@"
            SELECT {topClause}
                h.SessionDate,
                l.PreWeight,
                l.PostWeight,
                CASE 
                    WHEN l.PreWeight IS NOT NULL AND l.PostWeight IS NOT NULL 
                    THEN l.PreWeight - l.PostWeight 
                    ELSE NULL 
                END as WeightLoss,
                h.UFGoal,
                l.PreBP as BloodPressurePre,
                l.PostBP as BloodPressurePost
            FROM HDSchedule h
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.PatientID = @PatientID AND h.IsDischarged = 1
            ORDER BY h.SessionDate DESC";

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
                    Value = Convert.ToDecimal(row.PreWeight), 
                    Label = "Pre-Weight" 
                });
            }

            if (row.WeightLoss != null)
            {
                trends.WeightLossTrend.Add(new VitalTrend 
                { 
                    SessionDate = sessionDate, 
                    Value = Convert.ToDecimal(row.WeightLoss), 
                    Label = "Weight Loss" 
                });
            }

            if (row.UFGoal != null)
            {
                trends.UFGoalTrend.Add(new VitalTrend 
                { 
                    SessionDate = sessionDate, 
                    Value = Convert.ToDecimal(row.UFGoal), 
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
                AVG(CASE 
                    WHEN l.PreWeight IS NOT NULL AND l.PostWeight IS NOT NULL 
                    THEN l.PreWeight - l.PostWeight 
                    ELSE NULL 
                END) as AverageWeightLoss,
                AVG(l.PreWeight) as AveragePreWeight,
                AVG(l.PostWeight) as AveragePostWeight,
                (SELECT COUNT(*) FROM PostDialysisMedications m 
                 INNER JOIN HDSchedule s ON m.ScheduleID = s.ScheduleID 
                 WHERE s.PatientID = @PatientID) as TotalMedicationsAdministered
            FROM HDSchedule h
            LEFT JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
            WHERE h.PatientID = @PatientID AND h.IsDischarged = 1";

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
                OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY";

            var commonMeds = await connection.QueryAsync<string>(medicationsQuery, new { PatientID = patientId });
            stats.CommonMedications = commonMeds.ToList();
        }

        return stats;
    }
}
