-- Discharge all sessions for Patient 33 that have treatment data
-- This will make them appear in the Patient History page

USE HDSchedulerDB;
GO

-- Check sessions for Patient 33 before discharge
SELECT ScheduleID, SessionDate, IsDischarged, SessionStatus, TreatmentStartTime, TreatmentCompletionTime
FROM HDSchedule
WHERE PatientID = 33
ORDER BY SessionDate DESC;
GO

-- Discharge sessions that have been completed (have treatment times)
UPDATE HDSchedule
SET 
    IsDischarged = 1,
    DischargedAt = GETDATE(),
    SessionStatus = 'Completed'
WHERE PatientID = 33
  AND IsDischarged = 0
  AND TreatmentStartTime IS NOT NULL;
GO

-- Verify the update
SELECT ScheduleID, SessionDate, IsDischarged, DischargedAt, SessionStatus, TreatmentStartTime
FROM HDSchedule
WHERE PatientID = 33
ORDER BY SessionDate DESC;
GO
