-- Fix sessions from yesterday (Nov 18, 2025) that should be marked as completed
-- Run this to immediately mark yesterday's sessions as discharged/completed

UPDATE HDSchedule
SET IsDischarged = 1,
    IsMovedToHistory = 1,
    UpdatedAt = datetime('now')
WHERE IsDischarged = 0 
  AND date(SessionDate) < date('now');

-- Check the results
SELECT 
    ScheduleID,
    PatientID,
    SessionDate,
    SlotID,
    BedNumber,
    IsDischarged,
    IsMovedToHistory
FROM HDSchedule
WHERE date(SessionDate) = date('now', '-1 day');
