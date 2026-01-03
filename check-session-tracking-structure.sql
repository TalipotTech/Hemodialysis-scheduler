-- Check Session Tracking Structure for Discharge Feature
-- This query helps understand how to track completed sessions vs total scheduled sessions
-- Azure SQL Server Compatible Version

-- 1. Check Patient table structure (session counts)
SELECT TOP 5
    'Patients Table Session Tracking' AS QueryType,
    PatientID,
    Name,
    HDCycle,
    HDFrequency,
    TotalDialysisCompleted,
    HDStartDate,
    CreatedAt
FROM Patients
WHERE IsActive = 1
ORDER BY PatientID DESC;
GO

-- 2. Check HDSchedule table structure (individual sessions)
SELECT TOP 10
    'HDSchedule Sessions Per Patient' AS QueryType,
    PatientID,
    COUNT(*) AS TotalScheduledSessions,
    SUM(CASE WHEN SessionStatus = 'Completed' THEN 1 ELSE 0 END) AS CompletedSessions,
    SUM(CASE WHEN SessionStatus = 'Pre-Scheduled' THEN 1 ELSE 0 END) AS PreScheduledSessions,
    SUM(CASE WHEN SessionStatus = 'Active' THEN 1 ELSE 0 END) AS ActiveSessions,
    SUM(CASE WHEN IsDischarged = 1 THEN 1 ELSE 0 END) AS DischargedSessions
FROM HDSchedule
GROUP BY PatientID
ORDER BY PatientID DESC;
GO

-- 3. Check if there's a way to define "Total Sessions Required" for a patient
-- Currently, HD Cycle (e.g., "3x/week") is just a pattern, not a total count
-- We need to add a field like "TotalScheduledSessions" or "PrescribedSessionCount"
PRINT 'Note: HD Cycle is pattern-based (e.g., "3x/week"), not a total session count.';
PRINT 'Consider adding a PrescribedSessionCount field to Patients table for goal tracking.';
GO

-- 4. Sample patient with their session details
SELECT TOP 10
    'Sample Patient Session Details' AS QueryType,
    h.ScheduleID,
    h.PatientID,
    p.Name,
    p.HDCycle,
    p.TotalDialysisCompleted,
    h.SessionDate,
    h.SessionStatus,
    h.IsDischarged,
    h.TreatmentStartTime,
    h.TreatmentCompletionTime
FROM HDSchedule h
JOIN Patients p ON h.PatientID = p.PatientID
WHERE p.IsActive = 1
ORDER BY h.SessionDate DESC;
GO

-- 5. Identify patients who might be completing their cycle
-- (This is theoretical - we need to add logic to track this)
SELECT 
    'Patients Near Completion' AS QueryType,
    p.PatientID,
    p.Name,
    p.HDCycle,
    p.TotalDialysisCompleted,
    COUNT(h.ScheduleID) AS TotalScheduled,
    SUM(CASE WHEN h.SessionStatus = 'Completed' THEN 1 ELSE 0 END) AS Completed,
    SUM(CASE WHEN h.SessionStatus = 'Pre-Scheduled' THEN 1 ELSE 0 END) AS Remaining
FROM Patients p
LEFT JOIN HDSchedule h ON p.PatientID = h.PatientID
WHERE p.IsActive = 1
GROUP BY p.PatientID, p.Name, p.HDCycle, p.TotalDialysisCompleted
HAVING SUM(CASE WHEN h.SessionStatus = 'Pre-Scheduled' THEN 1 ELSE 0 END) <= 1;  -- Only 1 or fewer sessions remaining
GO

-- 6. Detailed view of sessions for a specific patient (change PatientID as needed)
PRINT 'Change PatientID in the query below to check a specific patient:';
GO

SELECT 
    'Patient Session Breakdown' AS QueryType,
    p.Name,
    p.TotalDialysisCompleted AS CompletedSessions,
    h.SessionDate,
    h.SessionStatus,
    h.IsDischarged,
    h.SlotID,
    h.BedNumber,
    CASE 
        WHEN h.SessionStatus = 'Pre-Scheduled' THEN 'Waiting'
        WHEN h.SessionStatus = 'Active' THEN 'In Progress'
        WHEN h.SessionStatus = 'Completed' THEN 'Done'
        ELSE h.SessionStatus
    END AS StatusDescription
FROM HDSchedule h
JOIN Patients p ON h.PatientID = p.PatientID
WHERE h.PatientID = (SELECT TOP 1 PatientID FROM Patients WHERE IsActive = 1 ORDER BY PatientID DESC)
ORDER BY h.SessionDate;
GO
