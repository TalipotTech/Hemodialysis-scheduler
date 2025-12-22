-- Script to verify and add missing fields for Patient History display
-- Run this to ensure all frontend fields are captured in the database

USE HDSchedulerDB;
GO

-- Check existing columns in HDSchedule table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HDSchedule'
ORDER BY ORDINAL_POSITION;
GO

-- Add missing columns if they don't exist (these are the fields shown in your frontend)

-- Check and add ActualBFR if missing (already exists based on model)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'ActualBFR')
BEGIN
    ALTER TABLE HDSchedule ADD ActualBFR INT NULL;
    PRINT 'Added ActualBFR column';
END
ELSE
    PRINT 'ActualBFR already exists';
GO

-- Check and add TotalUFAchieved if missing  
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'TotalUFAchieved')
BEGIN
    ALTER TABLE HDSchedule ADD TotalUFAchieved DECIMAL(5,2) NULL;
    PRINT 'Added TotalUFAchieved column';
END
ELSE
    PRINT 'TotalUFAchieved already exists';
GO

-- Check and add Notes field for session notes
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'Notes')
BEGIN
    ALTER TABLE HDSchedule ADD Notes NVARCHAR(MAX) NULL;
    PRINT 'Added Notes column';
END
ELSE
    PRINT 'Notes already exists';
GO

-- Verify IntraDialyticRecords table structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntraDialyticRecords')
BEGIN
    PRINT 'IntraDialyticRecords table exists';
    SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'IntraDialyticRecords'
    ORDER BY ORDINAL_POSITION;
END
ELSE
    PRINT 'WARNING: IntraDialyticRecords table does NOT exist!';
GO

-- Verify PostDialysisMedications table structure  
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PostDialysisMedications')
BEGIN
    PRINT 'PostDialysisMedications table exists';
    SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PostDialysisMedications'
    ORDER BY ORDINAL_POSITION;
END
ELSE
    PRINT 'WARNING: PostDialysisMedications table does NOT exist!';
GO

PRINT 'Verification complete!';
