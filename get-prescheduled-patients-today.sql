-- Query to get all pre-scheduled patients for TODAY
-- These are patients with sessions scheduled for today but not yet activated

SELECT 
    p.PatientID,
    p.Name AS PatientName,
    p.MRN,
    p.Age,
    p.Gender,
    p.ContactNumber,
    s.ScheduleID,
    s.SessionDate,
    s.SessionStatus,
    s.SlotID,
    slot.SlotName,
    slot.StartTime,
    slot.EndTime,
    s.BedNumber,
    s.IsMissed,
    s.IsDischarged,
    s.IsMovedToHistory,
    -- Calculate how late the patient is (in minutes)
    CASE 
        WHEN slot.StartTime IS NOT NULL THEN
            DATEDIFF(MINUTE, 
                DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), 
                    CAST(s.SessionDate AS DATETIME)), 
                GETDATE())
        ELSE NULL
    END AS MinutesLate,
    -- Flag if patient should show as late (more than 60 minutes past start time)
    CASE 
        WHEN slot.StartTime IS NOT NULL 
            AND DATEDIFF(MINUTE, 
                DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), 
                    CAST(s.SessionDate AS DATETIME)), 
                GETDATE()) > 60
        THEN 'YES - LATE'
        ELSE 'No'
    END AS IsLate
FROM HDSchedule s
INNER JOIN Patients p ON s.PatientID = p.PatientID
LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
WHERE 
    -- Today's sessions only
    CAST(s.SessionDate AS DATE) = CAST(GETDATE() AS DATE)
    -- Pre-scheduled status (not activated yet)
    AND s.SessionStatus = 'Pre-Scheduled'
    -- Not discharged
    AND s.IsDischarged = 0
    -- Not moved to history
    AND s.IsMovedToHistory = 0
    -- Not already marked as missed
    AND ISNULL(s.IsMissed, 0) = 0
ORDER BY 
    CASE 
        WHEN s.SlotID = 1 THEN 1  -- Morning
        WHEN s.SlotID = 2 THEN 2  -- Afternoon
        WHEN s.SlotID = 3 THEN 3  -- Evening
        WHEN s.SlotID = 4 THEN 4  -- Night
        ELSE 5                     -- Unassigned
    END,
    s.BedNumber,
    p.Name;

-- Summary by time slot
SELECT 
    COALESCE(slot.SlotName, 'Unassigned') AS TimeSlot,
    COUNT(*) AS PatientCount,
    COUNT(CASE 
        WHEN DATEDIFF(MINUTE, 
            DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), 
                CAST(s.SessionDate AS DATETIME)), 
            GETDATE()) > 60 
        THEN 1 
    END) AS LatePatients
FROM HDSchedule s
INNER JOIN Patients p ON s.PatientID = p.PatientID
LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
WHERE 
    CAST(s.SessionDate AS DATE) = CAST(GETDATE() AS DATE)
    AND s.SessionStatus = 'Pre-Scheduled'
    AND s.IsDischarged = 0
    AND s.IsMovedToHistory = 0
    AND ISNULL(s.IsMissed, 0) = 0
GROUP BY slot.SlotName, s.SlotID
ORDER BY s.SlotID;

-- Show current server time for reference
SELECT 
    GETDATE() AS CurrentServerTime,
    CAST(GETDATE() AS DATE) AS CurrentDate;
