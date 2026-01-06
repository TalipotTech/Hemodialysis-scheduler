-- Clean up corrupted session dates (AUTOMATIC FIX)
-- This will DELETE any HDSchedule records with dates before 2020

USE HDSchedulerDB;
GO

PRINT '========================================';
PRINT 'CLEANING CORRUPTED SESSION DATES';
PRINT '========================================';

BEGIN TRANSACTION;

DECLARE @CorruptedCount INT;

-- Count corrupted records
SELECT @CorruptedCount = COUNT(*)
FROM HDSchedule
WHERE SessionDate < '2020-01-01';

PRINT CONCAT('Found ', @CorruptedCount, ' corrupted records with dates before 2020');
PRINT '';

IF @CorruptedCount > 0
BEGIN
    PRINT 'Showing corrupted records before deletion:';
    SELECT 
        ScheduleID,
        PatientID,
        p.Name AS PatientName,
        SessionDate,
        SessionStatus,
        SlotID,
        BedNumber
    FROM HDSchedule s
    LEFT JOIN Patients p ON s.PatientID = p.PatientID
    WHERE SessionDate < '2020-01-01'
    ORDER BY SessionDate ASC;
    
    PRINT '';
    PRINT 'Deleting corrupted records...';
    
    -- Delete corrupted records
    DELETE FROM HDSchedule
    WHERE SessionDate < '2020-01-01';
    
    PRINT CONCAT('✓ Deleted ', @CorruptedCount, ' corrupted records');
    PRINT '';
    PRINT '========================================';
    PRINT 'Verifying cleanup...';
    PRINT '========================================';
    
    -- Verify no corrupted records remain
    DECLARE @RemainingCorrupted INT;
    SELECT @RemainingCorrupted = COUNT(*)
    FROM HDSchedule
    WHERE SessionDate < '2020-01-01';
    
    IF @RemainingCorrupted = 0
    BEGIN
        PRINT '✓ SUCCESS: All corrupted records have been removed!';
        COMMIT TRANSACTION;
        PRINT '✓ Transaction committed';
    END
    ELSE
    BEGIN
        PRINT '✗ ERROR: Some corrupted records still remain!';
        ROLLBACK TRANSACTION;
        PRINT '✗ Transaction rolled back';
    END
END
ELSE
BEGIN
    PRINT '✓ No corrupted records found. Database is clean!';
    COMMIT TRANSACTION;
END

PRINT '';
PRINT '========================================';
PRINT 'Current valid sessions for Patient 39:';
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
  AND SessionDate >= '2020-01-01'
ORDER BY SessionDate DESC;

PRINT '';
PRINT 'Done! You can now try activating the patient again.';
