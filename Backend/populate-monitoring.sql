-- Populate monitoring data for all active HD sessions
-- This will add 4 monitoring records to each session that doesn't have any

-- First, let's see what we have
SELECT '=== Current State ===' as Info;
SELECT 'Total monitoring records: ' || COUNT(*) as Info FROM IntraDialyticRecords;
SELECT 'Sessions with monitoring: ' || COUNT(DISTINCT ScheduleID) as Info FROM IntraDialyticRecords;

-- Add monitoring records to sessions that don't have any
INSERT INTO IntraDialyticRecords 
(PatientID, ScheduleID, SessionDate, TimeRecorded, BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
SELECT 
    h.PatientID,
    h.ScheduleID,
    date(h.SessionDate) as SessionDate,
    datetime(h.SessionDate, '+1 hour') as TimeRecorded,
    '130/80' as BloodPressure,
    70 as PulseRate,
    36.5 as Temperature,
    0.5 as UFVolume,
    120 as VenousPressure,
    'First hourly check' as Notes,
    datetime('now') as CreatedAt
FROM HDSchedule h
WHERE date(h.SessionDate) >= date('now', '-30 days')
AND NOT EXISTS (
    SELECT 1 FROM IntraDialyticRecords i 
    WHERE i.ScheduleID = h.ScheduleID
);

INSERT INTO IntraDialyticRecords 
(PatientID, ScheduleID, SessionDate, TimeRecorded, BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
SELECT 
    h.PatientID,
    h.ScheduleID,
    date(h.SessionDate) as SessionDate,
    datetime(h.SessionDate, '+2 hours') as TimeRecorded,
    '135/85' as BloodPressure,
    72 as PulseRate,
    36.7 as Temperature,
    1.2 as UFVolume,
    125 as VenousPressure,
    'Second hourly check' as Notes,
    datetime('now') as CreatedAt
FROM HDSchedule h
WHERE date(h.SessionDate) >= date('now', '-30 days')
AND EXISTS (
    SELECT 1 FROM IntraDialyticRecords i 
    WHERE i.ScheduleID = h.ScheduleID 
    AND i.Notes = 'First hourly check'
);

INSERT INTO IntraDialyticRecords 
(PatientID, ScheduleID, SessionDate, TimeRecorded, BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
SELECT 
    h.PatientID,
    h.ScheduleID,
    date(h.SessionDate) as SessionDate,
    datetime(h.SessionDate, '+3 hours') as TimeRecorded,
    '140/90' as BloodPressure,
    75 as PulseRate,
    36.8 as Temperature,
    1.8 as UFVolume,
    130 as VenousPressure,
    'Third hourly check' as Notes,
    datetime('now') as CreatedAt
FROM HDSchedule h
WHERE date(h.SessionDate) >= date('now', '-30 days')
AND EXISTS (
    SELECT 1 FROM IntraDialyticRecords i 
    WHERE i.ScheduleID = h.ScheduleID 
    AND i.Notes = 'Second hourly check'
);

INSERT INTO IntraDialyticRecords 
(PatientID, ScheduleID, SessionDate, TimeRecorded, BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
SELECT 
    h.PatientID,
    h.ScheduleID,
    date(h.SessionDate) as SessionDate,
    datetime(h.SessionDate, '+4 hours') as TimeRecorded,
    '138/88' as BloodPressure,
    73 as PulseRate,
    36.9 as Temperature,
    2.4 as UFVolume,
    128 as VenousPressure,
    'Final check' as Notes,
    datetime('now') as CreatedAt
FROM HDSchedule h
WHERE date(h.SessionDate) >= date('now', '-30 days')
AND EXISTS (
    SELECT 1 FROM IntraDialyticRecords i 
    WHERE i.ScheduleID = h.ScheduleID 
    AND i.Notes = 'Third hourly check'
);

-- Show results
SELECT '=== After Populating ===' as Info;
SELECT 'Total monitoring records: ' || COUNT(*) as Info FROM IntraDialyticRecords;
SELECT 'Sessions with monitoring: ' || COUNT(DISTINCT ScheduleID) as Info FROM IntraDialyticRecords;
SELECT '=== Sample Records ===' as Info;
SELECT 
    'ScheduleID ' || ScheduleID || ' (Patient ' || PatientID || '): ' || 
    COUNT(*) || ' records' as Info
FROM IntraDialyticRecords
GROUP BY ScheduleID, PatientID
LIMIT 10;
