-- Check patient slot assignments and today's sessions
-- Run this in Azure Data Studio connected to hds-dev-db

DECLARE @today DATE = '2026-01-01';

-- 1. Patients with their preferred slots and today's sessions
SELECT 
    p.PatientID,
    p.Name,
    p.MRN,
    p.Age,
    p.Gender,
    p.HDCycle,
    p.PreferredSlotID,
    s.SlotName as PreferredSlot,
    p.IsActive,
    -- Today's session info
    hs.ScheduleID,
    CAST(hs.SessionDate AS DATE) as SessionDate,
    hs.SlotID as TodaySlotID,
    slots_today.SlotName as TodaySlot,
    hs.BedNumber as TodayBedNumber,
    hs.SessionStatus,
    hs.IsDischarged,
    hs.IsMovedToHistory,
    -- Categorization
    CASE 
        WHEN hs.ScheduleID IS NOT NULL AND hs.SessionStatus = 'Active' THEN 'Active Today'
        WHEN hs.ScheduleID IS NOT NULL AND hs.SessionStatus = 'Pre-Scheduled' THEN 'Pre-Scheduled Today'
        WHEN hs.ScheduleID IS NOT NULL AND hs.SessionStatus = 'In Progress' THEN 'In Progress Today'
        WHEN p.PreferredSlotID IS NOT NULL THEN 'Has Preferred Slot'
        ELSE 'No Slot Assignment'
    END as PatientCategory
FROM Patients p
LEFT JOIN Slots s ON p.PreferredSlotID = s.SlotID
LEFT JOIN HDSchedule hs ON p.PatientID = hs.PatientID 
    AND CAST(hs.SessionDate AS DATE) = @today
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
LEFT JOIN Slots slots_today ON hs.SlotID = slots_today.SlotID
WHERE p.IsActive = 1
ORDER BY 
    CASE 
        WHEN hs.ScheduleID IS NOT NULL THEN 0  -- Patients with today's session first
        ELSE 1
    END,
    ISNULL(hs.SlotID, p.PreferredSlotID),
    hs.BedNumber,
    p.Name;

-- 2. Summary statistics
SELECT 
    'Total Active Patients' as Statistic,
    COUNT(*) as Count
FROM Patients WHERE IsActive = 1
UNION ALL
SELECT 
    'Patients with Sessions Today',
    COUNT(DISTINCT hs.PatientID)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = @today
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
UNION ALL
SELECT 
    'Pre-Scheduled for Today',
    COUNT(*)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = @today
    AND hs.SessionStatus = 'Pre-Scheduled'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
UNION ALL
SELECT 
    'Active Sessions Today',
    COUNT(*)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = @today
    AND hs.SessionStatus = 'Active'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0;

-- 3. Show slot distribution
SELECT 
    ISNULL(s.SlotName, 'Unassigned') as SlotName,
    COUNT(*) as PatientCount
FROM Patients p
LEFT JOIN Slots s ON p.PreferredSlotID = s.SlotID
WHERE p.IsActive = 1
GROUP BY s.SlotName
ORDER BY s.SlotName;
