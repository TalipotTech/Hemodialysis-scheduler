-- Check data for Session 804
-- This will show all the data stored in the database for this session

SELECT 
    ScheduleID,
    SessionDate,
    PatientID,
    BedNumber,
    SlotID,
    DryWeight,
    HDStartDate,
    HDCycle,
    WeightGain,
    DialyserType,
    DialyserModel,
    DialyserReuseCount,
    BloodTubingReuse,
    HDUnitNumber,
    PrescribedDuration,
    UFGoal,
    DialysatePrescription,
    PrescribedBFR,
    AnticoagulationType,
    HeparinDose,
    SyringeType,
    BolusDose,
    HeparinInfusionRate,
    AccessType,
    AccessLocation,
    StartTime,
    PreWeight,
    PreBPSitting,
    PreTemperature,
    AccessBleedingTime,
    AccessStatus,
    Complications,
    PostWeight,
    PostSBP,
    PostDBP,
    PostHR,
    PostAccessStatus,
    TotalFluidRemoved,
    MedicationType,
    MedicationName,
    Dose,
    Route,
    AdministeredAt,
    AlertType,
    AlertMessage,
    Severity,
    Resolution,
    IsDischarged,
    SessionStatus,
    CreatedAt,
    UpdatedAt
FROM HDSchedule
WHERE ScheduleID = 804;

-- Check monitoring records
SELECT COUNT(*) as MonitoringRecordsCount
FROM IntraDialyticRecords
WHERE ScheduleID = 804;

-- Check medications
SELECT COUNT(*) as MedicationsCount
FROM PostDialysisMedications
WHERE ScheduleID = 804;

-- Check alerts (if separate table exists)
SELECT COUNT(*) as AlertsCount
FROM TreatmentAlerts
WHERE ScheduleID = 804;

PRINT 'Data check complete for Session 804';
