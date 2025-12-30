-- Check monitoring records for session 848
SELECT 
    im.*,
    hs.PatientID,
    hs.SessionDate
FROM IntraDialyticMonitoring im
INNER JOIN HDSchedule hs ON im.ScheduleID = hs.ScheduleID
WHERE im.ScheduleID = 848
ORDER BY im.TimeRecorded;

-- Count total monitoring records for session 848
SELECT COUNT(*) as TotalMonitoringRecords
FROM IntraDialyticMonitoring
WHERE ScheduleID = 848;
