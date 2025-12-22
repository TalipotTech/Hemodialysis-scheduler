-- Test query for today's completed sessions
-- Run this to check what data exists

USE HDSchedulerDB;
GO

-- Check today's date
SELECT GETDATE() as CurrentDateTime, CAST(GETDATE() AS DATE) as TodayDate;
GO

-- Check all discharged sessions
SELECT 
    h.ScheduleID,
    h.PatientID,
    h.SessionDate,
    CAST(h.SessionDate AS DATE) as SessionDateOnly,
    h.IsDischarged,
    h.DischargedAt,
    h.SessionStatus,
    CASE 
        WHEN CAST(h.SessionDate AS DATE) = CAST(GETDATE() AS DATE) THEN 'TODAY'
        ELSE 'NOT TODAY'
    END as IsToday
FROM HDSchedule h
WHERE h.IsDischarged = 1
ORDER BY h.SessionDate DESC;
GO

-- Get today's completed sessions with patient info
SELECT 
    p.PatientID, p.MRN, p.Name, p.Age, p.Gender, 
    p.ContactNumber,
    h.ScheduleID, h.SlotID, h.BedNumber, h.SessionDate,
    h.IsDischarged, h.DischargedAt, h.SessionStatus
FROM Patients p
INNER JOIN HDSchedule h ON p.PatientID = h.PatientID
WHERE h.IsDischarged = 1 
  AND CAST(h.SessionDate AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY h.DischargedAt DESC;
GO
