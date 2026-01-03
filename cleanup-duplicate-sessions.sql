-- Fix duplicate sessions for mayavi m (ID: 26) on 2026-01-02
-- Keep only ONE session per patient per day

-- Step 1: View the duplicate sessions
SELECT 
    ScheduleID, PatientID, SessionDate, SlotID, BedNumber, SessionStatus, 
    IsDischarged, CreatedAt
FROM HDSchedule
WHERE PatientID = 26 
    AND SessionDate = '2026-01-02'
ORDER BY ScheduleID;

-- Step 2: Keep the ACTIVE session (1177), delete the others
-- Delete Pre-Scheduled session in Slot 2 with no bed
DELETE FROM HDSchedule 
WHERE ScheduleID = 903 
    AND PatientID = 26 
    AND SessionStatus = 'Pre-Scheduled';

-- Delete Pre-Scheduled session in Slot 1, Bed 1
DELETE FROM HDSchedule 
WHERE ScheduleID = 232 
    AND PatientID = 26 
    AND SessionStatus = 'Pre-Scheduled';

-- Step 3: Verify only one session remains
SELECT 
    ScheduleID, PatientID, SessionDate, SlotID, BedNumber, SessionStatus
FROM HDSchedule
WHERE PatientID = 26 
    AND SessionDate = '2026-01-02';

-- Expected result: Only ScheduleID 1177 should remain (Active, Slot 2, Bed 5)
