-- Add Post-Dialysis Vitals columns to HDSchedule table
-- These fields are collected at the end of dialysis session

USE [hds-dev-db];
GO

-- Post-Dialysis Weight
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PostWeight')
BEGIN
    ALTER TABLE HDSchedule ADD PostWeight DECIMAL(5, 2) NULL;
    PRINT 'Added PostWeight column';
END
GO

-- Post-Dialysis Systolic BP
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PostSBP')
BEGIN
    ALTER TABLE HDSchedule ADD PostSBP INT NULL;
    PRINT 'Added PostSBP column';
END
GO

-- Post-Dialysis Diastolic BP
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PostDBP')
BEGIN
    ALTER TABLE HDSchedule ADD PostDBP INT NULL;
    PRINT 'Added PostDBP column';
END
GO

-- Post-Dialysis Heart Rate
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PostHR')
BEGIN
    ALTER TABLE HDSchedule ADD PostHR INT NULL;
    PRINT 'Added PostHR column';
END
GO

-- Total Fluid Removed
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'TotalFluidRemoved')
BEGIN
    ALTER TABLE HDSchedule ADD TotalFluidRemoved DECIMAL(5, 2) NULL;
    PRINT 'Added TotalFluidRemoved column';
END
GO

-- Post-Access Status (condition of vascular access after dialysis)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PostAccessStatus')
BEGIN
    ALTER TABLE HDSchedule ADD PostAccessStatus NVARCHAR(200) NULL;
    PRINT 'Added PostAccessStatus column';
END
GO

-- Verify columns were added
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HDSchedule' 
AND COLUMN_NAME IN ('PostWeight', 'PostSBP', 'PostDBP', 'PostHR', 'TotalFluidRemoved', 'PostAccessStatus')
ORDER BY COLUMN_NAME;
GO

PRINT 'Post-dialysis vitals columns added successfully!';
