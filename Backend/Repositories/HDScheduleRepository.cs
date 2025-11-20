using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class HDScheduleRepository : IHDScheduleRepository
{
    private readonly DapperContext _context;

    public HDScheduleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<HDSchedule>> GetAllAsync()
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            ORDER BY h.SessionDate DESC, h.CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query);
        return schedules.ToList();
    }

    public async Task<HDSchedule?> GetByIdAsync(int scheduleId)
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.ScheduleID = @ScheduleID";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<HDSchedule>(query, new { ScheduleID = scheduleId });
    }

    public async Task<List<HDSchedule>> GetByPatientIdAsync(int patientId)
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.PatientID = @PatientID
            ORDER BY h.SessionDate DESC";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query, new { PatientID = patientId });
        return schedules.ToList();
    }

    public async Task<List<HDSchedule>> GetBySlotAsync(int slotId)
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.SlotID = @SlotID AND h.IsDischarged = 0
            ORDER BY h.BedNumber";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query, new { SlotID = slotId });
        return schedules.ToList();
    }

    public async Task<List<HDSchedule>> GetBySlotAndDateAsync(int slotId, DateTime date)
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.SlotID = @SlotID 
              AND DATE(h.SessionDate) = DATE(@Date)
            ORDER BY h.BedNumber";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query, new { SlotID = slotId, Date = date });
        return schedules.ToList();
    }

    public async Task<List<HDSchedule>> GetActiveAsync()
    {
        // First, move completed sessions to history
        await MoveCompletedSessionsToHistoryAsync();
        
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.IsDischarged = 0 AND h.IsMovedToHistory = 0
            ORDER BY h.SlotID, h.BedNumber";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query);
        return schedules.ToList();
    }

    public async Task<List<HDSchedule>> GetTodaySchedulesAsync()
    {
        // First, move completed sessions to history
        await MoveCompletedSessionsToHistoryAsync();
        
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE date(h.SessionDate) = date('now') AND h.IsDischarged = 0 AND h.IsMovedToHistory = 0
            ORDER BY h.SlotID, h.BedNumber";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query);
        return schedules.ToList();
    }

    public async Task<int> CreateAsync(HDSchedule schedule)
    {
        var query = @"
            INSERT INTO HDSchedule 
            (PatientID, SessionDate, DryWeight, HDStartDate, HDCycle, WeightGain,
             DialyserType, DialyserReuseCount, BloodTubingReuse, HDUnitNumber,
             PrescribedDuration, UFGoal, DialysatePrescription, PrescribedBFR,
             AnticoagulationType, HeparinDose, SyringeType, BolusDose, HeparinInfusionRate,
             AccessType, BloodPressure, Symptoms, BloodTestDone,
             SlotID, BedNumber, AssignedDoctor, AssignedNurse, 
             CreatedByStaffName, CreatedByStaffRole, IsDischarged, CreatedAt, UpdatedAt)
            VALUES 
            (@PatientID, @SessionDate, @DryWeight, @HDStartDate, @HDCycle, @WeightGain,
             @DialyserType, @DialyserReuseCount, @BloodTubingReuse, @HDUnitNumber,
             @PrescribedDuration, @UFGoal, @DialysatePrescription, @PrescribedBFR,
             @AnticoagulationType, @HeparinDose, @SyringeType, @BolusDose, @HeparinInfusionRate,
             @AccessType, @BloodPressure, @Symptoms, @BloodTestDone,
             @SlotID, @BedNumber, @AssignedDoctor, @AssignedNurse,
             @CreatedByStaffName, @CreatedByStaffRole, 0, datetime('now'), datetime('now'));
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, schedule);
    }

    public async Task<bool> UpdateAsync(HDSchedule schedule)
    {
        var query = @"
            UPDATE HDSchedule SET
                SessionDate = @SessionDate,
                DryWeight = @DryWeight,
                HDStartDate = @HDStartDate,
                HDCycle = @HDCycle,
                WeightGain = @WeightGain,
                DialyserType = @DialyserType,
                DialyserReuseCount = @DialyserReuseCount,
                BloodTubingReuse = @BloodTubingReuse,
                HDUnitNumber = @HDUnitNumber,
                PrescribedDuration = @PrescribedDuration,
                UFGoal = @UFGoal,
                DialysatePrescription = @DialysatePrescription,
                PrescribedBFR = @PrescribedBFR,
                AnticoagulationType = @AnticoagulationType,
                HeparinDose = @HeparinDose,
                SyringeType = @SyringeType,
                BolusDose = @BolusDose,
                HeparinInfusionRate = @HeparinInfusionRate,
                AccessType = @AccessType,
                BloodPressure = @BloodPressure,
                Symptoms = @Symptoms,
                BloodTestDone = @BloodTestDone,
                SlotID = @SlotID,
                BedNumber = @BedNumber,
                AssignedDoctor = @AssignedDoctor,
                AssignedNurse = @AssignedNurse,
                IsDischarged = @IsDischarged,
                UpdatedAt = datetime('now')
            WHERE ScheduleID = @ScheduleID";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, schedule);
        return affected > 0;
    }

    public async Task<bool> PartialUpdateAsync(int scheduleId, Dictionary<string, object> updates)
    {
        if (updates == null || updates.Count == 0)
            return false;

        // Build dynamic UPDATE query
        var setClauses = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("@ScheduleID", scheduleId);

        // Map of allowed fields for security (only fields that exist in HDSchedule table)
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Basic HD Info
            "DryWeight", "HDStartDate", "HDCycle", "WeightGain",
            // Equipment
            "DialyserType", "DialyserModel", "DialyserReuseCount", "BloodTubingReuse", "HDUnitNumber",
            // Prescription
            "PrescribedDuration", "UFGoal", "DialysatePrescription", "PrescribedBFR",
            // Anticoagulation
            "AnticoagulationType", "HeparinDose", "SyringeType", "BolusDose", "HeparinInfusionRate",
            // Access
            "AccessType", "AccessLocation",
            // Vitals & Symptoms
            "BloodPressure", "Symptoms", "BloodTestDone",
            // Staff Assignment
            "AssignedDoctor", "AssignedNurse",
            // HDTreatmentSession fields
            "StartTime", "PreWeight", "PreBPSitting", "PreTemperature", "AccessBleedingTime", 
            "AccessStatus", "Complications",
            // IntraDialyticMonitoring fields
            "MonitoringTime", "HeartRate", "ActualBFR", "VenousPressure", "ArterialPressure",
            "CurrentUFR", "TotalUFAchieved", "TmpPressure", "Interventions", "StaffInitials",
            // PostDialysisMedications fields
            "MedicationType", "MedicationName", "Dose", "Route", "AdministeredAt",
            // TreatmentAlerts fields
            "AlertType", "AlertMessage", "Severity", "Resolution"
        };

        foreach (var kvp in updates)
        {
            if (allowedFields.Contains(kvp.Key))
            {
                setClauses.Add($"{kvp.Key} = @{kvp.Key}");
                parameters.Add($"@{kvp.Key}", kvp.Value);
            }
        }

        if (setClauses.Count == 0)
            return false;

        var query = $@"
            UPDATE HDSchedule SET
                {string.Join(", ", setClauses)},
                UpdatedAt = datetime('now')
            WHERE ScheduleID = @ScheduleID";

        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, parameters);
        return affected > 0;
    }

    public async Task<bool> DischargeAsync(int scheduleId)
    {
        using var connection = _context.CreateConnection();
        
        // Mark schedule as discharged
        var updateScheduleQuery = @"
            UPDATE HDSchedule SET 
                IsDischarged = 1, 
                UpdatedAt = datetime('now') 
            WHERE ScheduleID = @ScheduleID";
        
        var affected = await connection.ExecuteAsync(updateScheduleQuery, new { ScheduleID = scheduleId });
        
        // Release bed in BedAssignments if exists
        if (affected > 0)
        {
            var releaseBedQuery = @"
                UPDATE BedAssignments SET 
                    IsActive = 0,
                    DischargedAt = datetime('now')
                WHERE EXISTS (
                    SELECT 1 FROM HDSchedule h
                    WHERE h.ScheduleID = @ScheduleID
                    AND BedAssignments.PatientID = h.PatientID
                    AND BedAssignments.SlotID = h.SlotID
                    AND BedAssignments.BedNumber = h.BedNumber
                    AND date(BedAssignments.AssignmentDate) = date(h.SessionDate)
                    AND BedAssignments.IsActive = 1
                )";
            
            await connection.ExecuteAsync(releaseBedQuery, new { ScheduleID = scheduleId });
        }
        
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int scheduleId)
    {
        var query = "DELETE FROM HDSchedule WHERE ScheduleID = @ScheduleID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { ScheduleID = scheduleId });
        return affected > 0;
    }

    public async Task<int> CreateHDLogAsync(HDLog log)
    {
        var query = @"
            INSERT INTO HDLogs 
            (PatientID, ScheduleID, SessionDate, PreWeight, PostWeight, WeightLoss,
             BloodPressurePre, BloodPressurePost, Temperature, Notes, CreatedBy, CreatedAt)
            VALUES 
            (@PatientID, @ScheduleID, @SessionDate, @PreWeight, @PostWeight, @WeightLoss,
             @BloodPressurePre, @BloodPressurePost, @Temperature, @Notes, @CreatedBy, datetime('now'));
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, log);
    }

    public async Task<int> CreateIntraDialyticRecordAsync(object record)
    {
        var query = @"
            INSERT INTO IntraDialyticRecords 
            (PatientID, ScheduleID, SessionDate, TimeRecorded, BloodPressure, PulseRate,
             Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
            VALUES 
            (@PatientID, @ScheduleID, @SessionDate, @TimeRecorded, @BloodPressure, @PulseRate,
             @Temperature, @UFVolume, @VenousPressure, @Notes, datetime('now'));
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, record);
    }

    public async Task<int> CreatePostDialysisMedicationAsync(object medication)
    {
        var query = @"
            INSERT INTO PostDialysisMedications 
            (PatientID, ScheduleID, SessionDate, MedicationName, Dosage, Route, 
             AdministeredBy, AdministeredAt)
            VALUES 
            (@PatientID, @ScheduleID, @SessionDate, @MedicationName, @Dosage, @Route,
             @AdministeredBy, @AdministeredAt);
            SELECT last_insert_rowid()";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, medication);
    }

    public async Task<List<HDSchedule>> GetHistorySessionsAsync()
    {
        var query = @"
            SELECT h.*, p.Name as PatientName,
                   d.Name as AssignedDoctorName,
                   n.Name as AssignedNurseName
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            LEFT JOIN Staff d ON h.AssignedDoctor = d.StaffID
            LEFT JOIN Staff n ON h.AssignedNurse = n.StaffID
            WHERE h.IsMovedToHistory = 1
            ORDER BY h.SessionDate DESC, h.UpdatedAt DESC";
        
        using var connection = _context.CreateConnection();
        var schedules = await connection.QueryAsync<HDSchedule>(query);
        return schedules.ToList();
    }

    public async Task<bool> MoveCompletedSessionsToHistoryAsync()
    {
        // Slot timings:
        // Slot 1: Morning (6:00 AM - 10:00 AM) - 4 hours
        // Slot 2: Afternoon (11:00 AM - 3:00 PM) - 4 hours
        // Slot 3: Evening (4:00 PM - 8:00 PM) - 4 hours
        // Slot 4: Night (9:00 PM - 1:00 AM) - 4 hours
        
        // After 5 hours from session start, mark as complete
        // If SessionStartTime exists, use it. Otherwise, use slot start time + session date
        
        var query = @"
            UPDATE HDSchedule
            SET IsMovedToHistory = 1,
                IsDischarged = 1,
                UpdatedAt = datetime('now')
            WHERE IsDischarged = 0 
              AND IsMovedToHistory = 0
              AND (
                  -- Slot 1: Morning (6:00 AM - 10:00 AM) - move after 11:00 AM (5 hours from 6 AM)
                  (SlotID = 1 AND date(SessionDate) = date('now') AND time('now') > '11:00:00')
                  OR
                  -- Slot 2: Afternoon (11:00 AM - 3:00 PM) - move after 4:00 PM (5 hours from 11 AM)
                  (SlotID = 2 AND date(SessionDate) = date('now') AND time('now') > '16:00:00')
                  OR
                  -- Slot 3: Evening (4:00 PM - 8:00 PM) - move after 9:00 PM (5 hours from 4 PM)
                  (SlotID = 3 AND date(SessionDate) = date('now') AND time('now') > '21:00:00')
                  OR
                  -- Slot 4: Night (9:00 PM - 1:00 AM) - move after 2:00 AM next day (5 hours from 9 PM)
                  (SlotID = 4 AND (
                      (date(SessionDate) = date('now', '-1 day') AND time('now') > '02:00:00')
                      OR date(SessionDate) < date('now', '-1 day')
                  ))
                  OR
                  -- Any session from previous days (more than 24 hours old)
                  (date(SessionDate) < date('now'))
              )";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query);
        return affected > 0;
    }

    public async Task<(int dialyserCount, int bloodTubingCount)> GetLatestEquipmentCountsAsync(int patientId)
    {
        // Get the most recent session for this patient to retrieve current equipment usage
        var query = @"
            SELECT DialyserReuseCount, BloodTubingReuse
            FROM HDSchedule
            WHERE PatientID = @PatientID
            ORDER BY SessionDate DESC, CreatedAt DESC
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<(int DialyserReuseCount, int BloodTubingReuse)>(
            query, 
            new { PatientID = patientId }
        );
        
        // If no previous session found, start from 0
        // Otherwise, increment the counts for the new session
        if (result.DialyserReuseCount == 0 && result.BloodTubingReuse == 0)
        {
            // Check if record exists at all
            var hasRecord = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM HDSchedule WHERE PatientID = @PatientID",
                new { PatientID = patientId }
            );
            
            if (hasRecord == 0)
            {
                // First session ever - start at 1
                return (1, 1);
            }
        }
        
        // Increment counts for new session
        return (result.DialyserReuseCount + 1, result.BloodTubingReuse + 1);
    }
}
