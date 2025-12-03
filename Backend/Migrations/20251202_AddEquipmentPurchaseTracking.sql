-- Migration: Add Equipment Purchase Tracking to Patients
-- Date: 2025-12-02
-- Description: Adds DialysersPurchased and BloodTubingPurchased columns to track lifetime equipment purchases

-- Check if columns already exist and add them if they don't
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND name = 'DialysersPurchased')
BEGIN
    ALTER TABLE Patients ADD DialysersPurchased INT NOT NULL DEFAULT 0;
    PRINT 'Added DialysersPurchased column';
END
ELSE
BEGIN
    PRINT 'DialysersPurchased column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND name = 'BloodTubingPurchased')
BEGIN
    ALTER TABLE Patients ADD BloodTubingPurchased INT NOT NULL DEFAULT 0;
    PRINT 'Added BloodTubingPurchased column';
END
ELSE
BEGIN
    PRINT 'BloodTubingPurchased column already exists';
END

GO

PRINT 'Migration completed: Equipment purchase tracking columns added to Patients table';
