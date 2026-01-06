-- Fix corrupted dates in HDSchedule table
-- Find sessions with dates before 2020 (clearly invalid)

USE HDSchedulerDB;
GO

PRINT '========================================';
PRINT 'Finding corrupted session dates...';
PRINT '========================================';

-- Check for sessions with dates before 2020
SELECT 
    ScheduleID,
    PatientID,
    p.Name AS PatientName,
    SessionDate,
    SessionStatus,
    SlotID,
    BedNumber,
    IsDischarged,
    IsMovedToHistory
FROM HDSchedule s
LEFT JOIN Patients p ON s.PatientID = p.PatientID
WHERE SessionDate < '2020-01-01'
ORDER BY SessionDate ASC;

PRINT '';
PRINT 'Corrupted records found above (if any)';
PRINT '';

-- Show patient ID 39's sessions (the "add patient" with issue)
PRINT '========================================';
PRINT 'Patient ID 39 sessions:';
PRINT '========================================';
SELECT 
    ScheduleID,
    PatientID,
    p.Name AS PatientName,
    SessionDate,
    SessionStatus,
    SlotID,
    BedNumber,
    IsDischarged,
    IsMovedToHistory
FROM HDSchedule s
LEFT JOIN Patients p ON s.PatientID = p.PatientID
WHERE s.PatientID = 39
ORDER BY SessionDate DESC;

PRINT '';
PRINT '========================================';
PRINT 'SOLUTION OPTIONS:';
PRINT '========================================';
PRINT '1. DELETE corrupted records (recommended):';
PRINT '   DELETE FROM HDSchedule WHERE SessionDate < ''2020-01-01'';';
PRINT '';
PRINT '2. UPDATE to today''s date:';
PRINT '   UPDATE HDSchedule SET SessionDate = ''2026-01-06'' WHERE SessionDate < ''2020-01-01'';';
PRINT '';
PRINT '3. Mark as moved to history:';
PRINT '   UPDATE HDSchedule SET IsMovedToHistory = 1 WHERE SessionDate < ''2020-01-01'';';
PRINT '';
PRINT 'Run one of the above commands to fix the issue.';
