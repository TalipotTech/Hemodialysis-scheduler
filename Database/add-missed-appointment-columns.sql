-- ============================================
-- Add Missed Appointment Tracking to HDSchedule
-- Date: 2026-01-01
-- ============================================

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HDSchedule') AND name = 'IsMissed')
BEGIN
    -- Add columns for tracking missed appointments
    ALTER TABLE HDSchedule
    ADD IsMissed BIT DEFAULT 0 NOT NULL,
        MissedReason NVARCHAR(50) NULL, -- 'Sick', 'Emergency', 'Transportation', 'Unknown', 'Other'
        MissedNotes NVARCHAR(500) NULL,
        MissedDateTime DATETIME NULL,
        MissedMarkedByUserID INT NULL,
        MissedResolvedDateTime DATETIME NULL,
        MissedResolvedByUserID INT NULL,
        MissedResolutionNotes NVARCHAR(500) NULL;
    
    PRINT 'Missed appointment tracking columns added successfully';
END
ELSE
BEGIN
    PRINT 'Columns already exist, skipping...';
END
GO

-- Add index for quick querying of missed appointments (if not exists)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDSchedule_IsMissed' AND object_id = OBJECT_ID('HDSchedule'))
BEGIN
    CREATE INDEX IX_HDSchedule_IsMissed ON HDSchedule(IsMissed, SessionDate) WHERE IsMissed = 1;
    PRINT 'Index IX_HDSchedule_IsMissed created successfully';
END
ELSE
BEGIN
    PRINT 'Index already exists, skipping...';
END
GO

-- Update SessionStatus to include 'Missed' and 'Cancelled' as valid values
-- SessionStatus can now be: 'Pre-Scheduled', 'Active', 'Completed', 'Discharged', 'Missed', 'Cancelled'
PRINT 'Migration completed! SessionStatus can now include: Pre-Scheduled, Active, Completed, Discharged, Missed, Cancelled';
GO

-- Query to verify the new columns
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HDSchedule') AND name = 'IsMissed')
BEGIN
    SELECT TOP 1 
        ScheduleID, 
        PatientID, 
        SessionDate,
        SessionStatus,
        IsMissed,
        MissedReason,
        MissedDateTime
    FROM HDSchedule
    ORDER BY ScheduleID DESC;
    
    PRINT 'Verification query executed successfully';
END
GO
