-- Add missing columns to HDSchedule table for HD session data
-- Missing columns identified: PreBPSitting, StartTime, PreWeight, PreTemperature, AccessBleedingTime, AccessStatus, Complications

USE HDSchedulerDB;
GO

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreBPSitting')
BEGIN
    ALTER TABLE HDSchedule ADD PreBPSitting NVARCHAR(50) NULL;
    PRINT 'Added PreBPSitting column';
END
ELSE
BEGIN
    PRINT 'PreBPSitting column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'StartTime')
BEGIN
    ALTER TABLE HDSchedule ADD StartTime TIME NULL;
    PRINT 'Added StartTime column';
END
ELSE
BEGIN
    PRINT 'StartTime column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreWeight')
BEGIN
    ALTER TABLE HDSchedule ADD PreWeight DECIMAL(5,2) NULL;
    PRINT 'Added PreWeight column';
END
ELSE
BEGIN
    PRINT 'PreWeight column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreTemperature')
BEGIN
    ALTER TABLE HDSchedule ADD PreTemperature DECIMAL(4,1) NULL;
    PRINT 'Added PreTemperature column';
END
ELSE
BEGIN
    PRINT 'PreTemperature column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'AccessBleedingTime')
BEGIN
    ALTER TABLE HDSchedule ADD AccessBleedingTime INT NULL;
    PRINT 'Added AccessBleedingTime column';
END
ELSE
BEGIN
    PRINT 'AccessBleedingTime column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'AccessStatus')
BEGIN
    ALTER TABLE HDSchedule ADD AccessStatus NVARCHAR(50) NULL;
    PRINT 'Added AccessStatus column';
END
ELSE
BEGIN
    PRINT 'AccessStatus column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'Complications')
BEGIN
    ALTER TABLE HDSchedule ADD Complications NVARCHAR(MAX) NULL;
    PRINT 'Added Complications column';
END
ELSE
BEGIN
    PRINT 'Complications column already exists';
END
GO

PRINT 'Migration completed successfully!';
GO
