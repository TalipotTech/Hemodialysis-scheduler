-- Populate demo historical HD sessions for patient Achu
-- This creates completed sessions for the past week

DECLARE @PatientID INT = 1;
DECLARE @StartDate DATE = DATEADD(DAY, -7, GETDATE());
DECLARE @DaysBack INT = 0;

-- Create 7 historical sessions (one per day for the past week)
WHILE @DaysBack < 7
BEGIN
    DECLARE @SessionDate DATE = DATEADD(DAY, @DaysBack, @StartDate);
    DECLARE @SlotID INT = ((@DaysBack % 3) + 1); -- Rotate through slots 1, 2, 3
    DECLARE @BedNumber INT = ((@DaysBack % 10) + 1); -- Rotate through beds 1-10
    
    -- Insert HD Session
    INSERT INTO HDSchedule (
        PatientID, SessionDate, SlotID, BedNumber,
        DryWeight, HDCycle, WeightGain,
        DialyserType, DialyserModel, DialyserReuseCount, BloodTubingReuse,
        PrescribedDuration, UFGoal, DialysatePrescription, PrescribedBFR,
        AnticoagulationType, HeparinDose, SyringeType, BolusDose, HeparinInfusionRate,
        AccessType, AccessLocation,
        BloodPressure, Symptoms, BloodTestDone,
        SessionStatus, IsDischarged, IsMovedToHistory,
        CreatedByStaffName, CreatedByStaffRole,
        TreatmentStartTime, TreatmentCompletionTime, DischargeTime,
        CreatedAt, UpdatedAt
    )
    VALUES (
        @PatientID, @SessionDate, @SlotID, @BedNumber,
        52.5, 'Every day', 2.5,
        'HI', 'Fresenius F8', @DaysBack, @DaysBack,
        4.0, 2.5, 'Normal', 300,
        'Heparin', 5000, '10ml', 1000, 800,
        'AVF', 'Left forearm',
        '120/80', 'None', 0,
        'Completed', 1, 1,
        'Dr. Smith', 'Doctor',
        DATEADD(HOUR, 6, CAST(@SessionDate AS DATETIME)),
        DATEADD(HOUR, 10, CAST(@SessionDate AS DATETIME)),
        DATEADD(HOUR, 10, CAST(@SessionDate AS DATETIME)),
        DATEADD(HOUR, 10, CAST(@SessionDate AS DATETIME)),
        DATEADD(HOUR, 10, CAST(@SessionDate AS DATETIME))
    );
    
    DECLARE @ScheduleID INT = SCOPE_IDENTITY();
    
    -- Insert corresponding HDLog
    INSERT INTO HDLogs (
        ScheduleID, PatientID, SessionDate,
        PreWeight, PostWeight,
        PreBP, PostBP,
        PrePulse, PostPulse,
        StartTime, EndTime,
        TotalUF, BloodFlowRate, DialysateFlow,
        Remarks,
        CreatedAt
    )
    VALUES (
        @ScheduleID, @PatientID, @SessionDate,
        55.0, 52.5,
        '130/85', '120/80',
        78, 72,
        '06:00', '10:00',
        2.5, 300, 500,
        'Session completed without complications',
        DATEADD(HOUR, 10, CAST(@SessionDate AS DATETIME))
    );
    
    SET @DaysBack = @DaysBack + 1;
END

-- Verify the data was inserted
SELECT 
    'Sessions Created' as Result,
    COUNT(*) as Count
FROM HDSchedule
WHERE PatientID = @PatientID;

SELECT 
    'Logs Created' as Result,
    COUNT(*) as Count
FROM HDLogs
WHERE PatientID = @PatientID;
