-- Add Medication and Alert columns to HDSchedule table
-- These fields are used for Step 4 (Medications) and Step 5 (Alerts)

USE [HDSchedulerDB];
GO

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'MedicationType')
BEGIN
    ALTER TABLE HDSchedule ADD MedicationType NVARCHAR(100) NULL;
    PRINT 'Added MedicationType column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'MedicationName')
BEGIN
    ALTER TABLE HDSchedule ADD MedicationName NVARCHAR(200) NULL;
    PRINT 'Added MedicationName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Dose')
BEGIN
    ALTER TABLE HDSchedule ADD Dose NVARCHAR(100) NULL;
    PRINT 'Added Dose column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Route')
BEGIN
    ALTER TABLE HDSchedule ADD Route NVARCHAR(50) NULL;
    PRINT 'Added Route column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AdministeredAt')
BEGIN
    ALTER TABLE HDSchedule ADD AdministeredAt NVARCHAR(50) NULL;
    PRINT 'Added AdministeredAt column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AlertType')
BEGIN
    ALTER TABLE HDSchedule ADD AlertType NVARCHAR(100) NULL;
    PRINT 'Added AlertType column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AlertMessage')
BEGIN
    ALTER TABLE HDSchedule ADD AlertMessage NVARCHAR(MAX) NULL;
    PRINT 'Added AlertMessage column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Severity')
BEGIN
    ALTER TABLE HDSchedule ADD Severity NVARCHAR(50) NULL;
    PRINT 'Added Severity column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Resolution')
BEGIN
    ALTER TABLE HDSchedule ADD Resolution NVARCHAR(MAX) NULL;
    PRINT 'Added Resolution column';
END

PRINT 'Migration complete! All medication and alert columns have been added to HDSchedule table.';

-- Verify columns were added
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HDSchedule'
    AND COLUMN_NAME IN ('MedicationType', 'MedicationName', 'Dose', 'Route', 'AdministeredAt', 
                         'AlertType', 'AlertMessage', 'Severity', 'Resolution')
ORDER BY COLUMN_NAME;
