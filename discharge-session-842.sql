-- Discharge Session 842 to make it appear in Patient History
-- This marks the session as completed so it shows in the history view

UPDATE HDSchedule
SET 
    IsDischarged = 1,
    SessionStatus = 'Completed',
    DischargeTime = GETUTCDATE(),
    UpdatedAt = GETUTCDATE()
WHERE ScheduleID = 842;

-- Verify the update
SELECT 
    ScheduleID,
    PatientID,
    SessionDate,
    IsDischarged,
    SessionStatus,
    DischargeTime
FROM HDSchedule
WHERE ScheduleID = 842;

PRINT 'Session 842 has been discharged and will now appear in Patient History';
