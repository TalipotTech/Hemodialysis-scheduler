-- Add demo intra-dialytic monitoring records
-- This adds hourly monitoring data for each HD session

DECLARE @PatientID INT = 1;

-- Add monitoring records for each session (4 records per 4-hour session)
INSERT INTO IntraDialyticRecords (
    ScheduleID, PatientID, SessionDate, RecordTime, 
    BP, Pulse, UFRate, VenousPressure, BloodFlow, Symptoms, Intervention, CreatedAt
)
SELECT 
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    DATEADD(HOUR, n.HourOffset, l.StartTime),
    CASE n.HourOffset
        WHEN 0 THEN '130/85'
        WHEN 1 THEN '125/82'
        WHEN 2 THEN '122/80'
        WHEN 3 THEN '120/78'
    END,
    CASE n.HourOffset
        WHEN 0 THEN 78
        WHEN 1 THEN 76
        WHEN 2 THEN 74
        WHEN 3 THEN 72
    END,
    0.625, -- UFRate (2.5L / 4 hours = 0.625 L/hr)
    CASE n.HourOffset
        WHEN 0 THEN 120
        WHEN 1 THEN 125
        WHEN 2 THEN 128
        WHEN 3 THEN 130
    END,
    300, -- BloodFlow
    CASE n.HourOffset
        WHEN 0 THEN 'Patient comfortable'
        WHEN 1 THEN 'No complaints'
        WHEN 2 THEN 'Mild fatigue'
        WHEN 3 THEN 'Ready for discharge'
    END,
    CASE n.HourOffset
        WHEN 2 THEN 'Reduced UF rate slightly'
        ELSE NULL
    END,
    DATEADD(HOUR, n.HourOffset, CAST(h.SessionDate AS DATETIME) + CAST(l.StartTime AS DATETIME))
FROM HDSchedule h
INNER JOIN HDLogs l ON h.ScheduleID = l.ScheduleID
CROSS JOIN (
    SELECT 0 AS HourOffset UNION ALL
    SELECT 1 UNION ALL
    SELECT 2 UNION ALL
    SELECT 3
) n
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1;

-- Verify records were added
SELECT 
    'Total Monitoring Records Added' as Result,
    COUNT(*) as Count
FROM IntraDialyticRecords
WHERE PatientID = @PatientID;

SELECT 
    'Records by Session' as Info,
    h.SessionDate,
    COUNT(i.RecordID) as RecordCount
FROM HDSchedule h
LEFT JOIN IntraDialyticRecords i ON h.ScheduleID = i.ScheduleID
WHERE h.PatientID = @PatientID AND h.IsDischarged = 1
GROUP BY h.SessionDate
ORDER BY h.SessionDate DESC;
