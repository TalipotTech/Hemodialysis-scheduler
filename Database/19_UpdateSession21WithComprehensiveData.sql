-- Update Session 21 with comprehensive data to demonstrate all fields

UPDATE HDSchedule
SET
    -- Prescription Details
    DialyserModel = 'FX-80',
    AccessLocation = 'Left Arm AVF',
    StartTime = '10:00:00',
    DryWeight = '77',
    
    -- Pre-Dialysis Assessment
    PreWeight = 80.5,
    PreBPSitting = '140/90',
    PreTemperature = 36.8,
    AccessBleedingTime = '3 minutes',
    AccessStatus = 'Good flow, no complications',
    Complications = 'None reported',
    
    -- Intra-Dialytic Monitoring
    MonitoringTime = '11:30:00',
    HeartRate = 78,
    ActualBFR = 380,
    VenousPressure = 150,
    ArterialPressure = -180,
    CurrentUFR = 0.8,
    TotalUFAchieved = 2.5,
    TmpPressure = 120,
    Interventions = 'Patient complained of mild headache at 2hr mark, BP monitored closely',
    StaffInitials = 'RN-JD',
    
    -- Post-Dialysis Medications
    MedicationType = 'EPO',
    MedicationName = 'Erythropoietin',
    Dose = '4000 IU',
    Route = 'SC',
    AdministeredAt = '2025-11-20 14:30:00',
    
    -- Treatment Alerts
    AlertType = 'Hypotension',
    AlertMessage = 'BP dropped to 100/60 at 2.5 hours into session',
    Severity = 'Medium',
    Resolution = 'Reduced UFR rate, gave 100ml saline bolus. BP stabilized to 115/70',
    
    -- Update timestamp
    UpdatedAt = CURRENT_TIMESTAMP
    
WHERE ScheduleID = 21;

-- Verify the update
SELECT 
    ScheduleID,
    PatientID,
    SessionDate,
    DialyserType,
    DialyserModel,
    AccessType,
    AccessLocation,
    StartTime,
    DryWeight,
    PreWeight,
    PreBPSitting,
    PreTemperature,
    HeartRate,
    ActualBFR,
    MedicationName,
    Dose,
    AlertType,
    Severity
FROM HDSchedule
WHERE ScheduleID = 21;
