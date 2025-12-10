-- Add demo medications for the historical HD sessions
-- This adds common post-dialysis medications for each session

DECLARE @PatientID INT = 1;
DECLARE @SessionCount INT;

-- Get all completed sessions for this patient
SELECT @SessionCount = COUNT(*) FROM HDSchedule WHERE PatientID = @PatientID AND IsDischarged = 1;

-- Add medications for each session
INSERT INTO PostDialysisMedications (
    ScheduleID, PatientID, SessionDate, MedicationName, Dosage, Route, GivenTime, GivenBy, CreatedAt
)
SELECT 
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    'Erythropoietin (EPO)',
    '4000 IU',
    'IV',
    CAST('10:30' AS TIME),
    'Nurse Sarah',
    h.DischargeTime
FROM HDSchedule h
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1 AND h.ScheduleID % 2 = 1; -- Every other session

INSERT INTO PostDialysisMedications (
    ScheduleID, PatientID, SessionDate, MedicationName, Dosage, Route, GivenTime, GivenBy, CreatedAt
)
SELECT 
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    'Iron Sucrose',
    '100 mg',
    'IV',
    CAST('10:45' AS TIME),
    'Nurse Sarah',
    h.DischargeTime
FROM HDSchedule h
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1 AND h.ScheduleID % 3 = 0; -- Every third session

INSERT INTO PostDialysisMedications (
    ScheduleID, PatientID, SessionDate, MedicationName, Dosage, Route, GivenTime, GivenBy, CreatedAt
)
SELECT 
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    'Heparin Flush',
    '5 ml',
    'IV',
    CAST('10:15' AS TIME),
    'Nurse John',
    h.DischargeTime
FROM HDSchedule h
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1; -- All sessions

-- Add occasional antiemetic for symptomatic sessions
INSERT INTO PostDialysisMedications (
    ScheduleID, PatientID, SessionDate, MedicationName, Dosage, Route, GivenTime, GivenBy, CreatedAt
)
SELECT TOP 2
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    'Ondansetron',
    '4 mg',
    'IV',
    CAST('09:30' AS TIME),
    'Nurse Sarah',
    h.DischargeTime
FROM HDSchedule h
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1
ORDER BY h.SessionDate DESC;

-- Verify medications were added
SELECT 
    'Total Medications Added' as Result,
    COUNT(*) as Count
FROM PostDialysisMedications
WHERE PatientID = @PatientID;

SELECT 
    'Medications by Session' as Info,
    h.SessionDate,
    COUNT(m.MedicationID) as MedicationCount,
    STRING_AGG(m.MedicationName, ', ') as Medications
FROM HDSchedule h
LEFT JOIN PostDialysisMedications m ON h.ScheduleID = m.ScheduleID
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1
GROUP BY h.SessionDate
ORDER BY h.SessionDate DESC;
