-- Add missing columns to IntraDialyticMonitoring table

-- Check if VenousPressure column exists, if not add it
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('IntraDialyticMonitoring') 
    AND name = 'VenousPressure'
)
BEGIN
    ALTER TABLE IntraDialyticMonitoring
    ADD VenousPressure INT NULL;
    
    PRINT 'VenousPressure column added to IntraDialyticMonitoring table';
END
ELSE
BEGIN
    PRINT 'VenousPressure column already exists in IntraDialyticMonitoring table';
END

-- Check if Symptoms column exists, if not add it
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('IntraDialyticMonitoring') 
    AND name = 'Symptoms'
)
BEGIN
    ALTER TABLE IntraDialyticMonitoring
    ADD Symptoms NVARCHAR(MAX) NULL;
    
    PRINT 'Symptoms column added to IntraDialyticMonitoring table';
END
ELSE
BEGIN
    PRINT 'Symptoms column already exists in IntraDialyticMonitoring table';
END

