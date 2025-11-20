-- Migration 13: Add Bed Usage Tracking for Smart Bed Assignment
-- This enables bed rotation tracking, infection control, and usage analytics

-- Create BedUsageTracking table
CREATE TABLE IF NOT EXISTS BedUsageTracking (
    BedNumber INTEGER PRIMARY KEY,
    TotalUsageCount INTEGER DEFAULT 0,
    LastUsedDate DATETIME NULL,
    LastPatientID INTEGER NULL,
    IsUnderMaintenance BOOLEAN DEFAULT 0,
    MaintenanceNotes TEXT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (LastPatientID) REFERENCES Patients(PatientID)
);

-- Initialize bed tracking for all existing beds (1-20)
INSERT OR IGNORE INTO BedUsageTracking (BedNumber, TotalUsageCount, LastUsedDate)
SELECT DISTINCT BedNumber, 0, NULL
FROM HDSchedule
WHERE BedNumber IS NOT NULL
UNION
SELECT 1, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 1)
UNION
SELECT 2, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 2)
UNION
SELECT 3, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 3)
UNION
SELECT 4, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 4)
UNION
SELECT 5, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 5)
UNION
SELECT 6, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 6)
UNION
SELECT 7, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 7)
UNION
SELECT 8, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 8)
UNION
SELECT 9, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 9)
UNION
SELECT 10, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 10)
UNION
SELECT 11, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 11)
UNION
SELECT 12, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 12)
UNION
SELECT 13, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 13)
UNION
SELECT 14, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 14)
UNION
SELECT 15, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 15)
UNION
SELECT 16, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 16)
UNION
SELECT 17, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 17)
UNION
SELECT 18, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 18)
UNION
SELECT 19, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 19)
UNION
SELECT 20, 0, NULL WHERE NOT EXISTS (SELECT 1 FROM BedUsageTracking WHERE BedNumber = 20);

-- Populate initial usage data from existing schedules
UPDATE BedUsageTracking
SET TotalUsageCount = (
    SELECT COUNT(*) 
    FROM HDSchedule 
    WHERE HDSchedule.BedNumber = BedUsageTracking.BedNumber
),
LastUsedDate = (
    SELECT MAX(SessionDate)
    FROM HDSchedule
    WHERE HDSchedule.BedNumber = BedUsageTracking.BedNumber
),
LastPatientID = (
    SELECT PatientID
    FROM HDSchedule
    WHERE HDSchedule.BedNumber = BedUsageTracking.BedNumber
    ORDER BY SessionDate DESC
    LIMIT 1
);

-- Create indexes for faster queries
CREATE INDEX IF NOT EXISTS idx_bedusage_lastused 
ON BedUsageTracking(LastUsedDate);

CREATE INDEX IF NOT EXISTS idx_bedusage_count 
ON BedUsageTracking(TotalUsageCount);

-- Add trigger to auto-update bed usage when schedule is created
CREATE TRIGGER IF NOT EXISTS trg_update_bed_usage_on_schedule_insert
AFTER INSERT ON HDSchedule
WHEN NEW.BedNumber IS NOT NULL
BEGIN
    INSERT INTO BedUsageTracking (BedNumber, TotalUsageCount, LastUsedDate, LastPatientID)
    VALUES (NEW.BedNumber, 1, NEW.SessionDate, NEW.PatientID)
    ON CONFLICT(BedNumber) DO UPDATE SET
        TotalUsageCount = TotalUsageCount + 1,
        LastUsedDate = NEW.SessionDate,
        LastPatientID = NEW.PatientID,
        UpdatedAt = CURRENT_TIMESTAMP;
END;

-- Add trigger to update bed usage when schedule is updated
CREATE TRIGGER IF NOT EXISTS trg_update_bed_usage_on_schedule_update
AFTER UPDATE OF BedNumber ON HDSchedule
WHEN NEW.BedNumber IS NOT NULL AND NEW.BedNumber != OLD.BedNumber
BEGIN
    -- Decrement old bed
    UPDATE BedUsageTracking
    SET TotalUsageCount = TotalUsageCount - 1,
        UpdatedAt = CURRENT_TIMESTAMP
    WHERE BedNumber = OLD.BedNumber;
    
    -- Increment new bed
    INSERT INTO BedUsageTracking (BedNumber, TotalUsageCount, LastUsedDate, LastPatientID)
    VALUES (NEW.BedNumber, 1, NEW.SessionDate, NEW.PatientID)
    ON CONFLICT(BedNumber) DO UPDATE SET
        TotalUsageCount = TotalUsageCount + 1,
        LastUsedDate = NEW.SessionDate,
        LastPatientID = NEW.PatientID,
        UpdatedAt = CURRENT_TIMESTAMP;
END;

SELECT 'Migration 13 completed: Bed usage tracking system created with auto-update triggers' AS Result;
