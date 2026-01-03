-- Check which patients should be flagged as late/no-show
-- Current time: 2026-01-01 12:39 PM (approximate from logs)

DECLARE @currentTime DATETIME = GETDATE();
DECLARE @minutesThreshold INT = 60;

SELECT 
    s.ScheduleID,
    s.PatientID,
    p.Name AS PatientName,
    s.SessionDate,
    s.SlotID,
    slot.SlotName,
    slot.StartTime AS SlotStartTime,
    s.SessionStatus,
    -- Calculate session start datetime
    DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), CAST(s.SessionDate AS DATETIME)) AS SessionStartDateTime,
    -- Calculate minutes late
    DATEDIFF(MINUTE, 
        DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), CAST(s.SessionDate AS DATETIME)), 
        @currentTime) AS MinutesLate,
    -- Is it past threshold?
    CASE 
        WHEN DATEDIFF(MINUTE, 
            DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), CAST(s.SessionDate AS DATETIME)), 
            @currentTime) > @minutesThreshold THEN 'YES - SHOULD SHOW AS LATE'
        ELSE 'NO - Not late yet'
    END AS ShouldShowAsLate,
    @currentTime AS CurrentServerTime
FROM HDSchedule s
INNER JOIN Patients p ON s.PatientID = p.PatientID
LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
WHERE CAST(s.SessionDate AS DATE) = CAST(@currentTime AS DATE)
    AND s.SessionStatus = 'Pre-Scheduled'
    AND s.IsDischarged = 0
    AND s.IsMovedToHistory = 0
    AND ISNULL(s.IsMissed, 0) = 0
    AND slot.StartTime IS NOT NULL
ORDER BY MinutesLate DESC;

-- Show current server time
SELECT GETDATE() AS CurrentServerTime;

-- Show slot start times
SELECT SlotID, SlotName, StartTime, EndTime FROM Slots ORDER BY SlotID;
