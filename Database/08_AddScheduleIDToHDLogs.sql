-- Migration: Add ScheduleID to HDLogs table
-- This creates a proper foreign key relationship between HDLogs and HDSchedule

-- Add ScheduleID column to HDLogs
ALTER TABLE HDLogs ADD COLUMN ScheduleID INTEGER;

-- Create index for better query performance
CREATE INDEX IF NOT EXISTS idx_hdlogs_scheduleid ON HDLogs(ScheduleID);

-- Add foreign key constraint (SQLite requires recreating the table for this)
-- For now, we'll just add the column and update existing data

-- Update existing HDLogs records to match with HDSchedule records
-- This matches based on PatientID and SessionDate
UPDATE HDLogs
SET ScheduleID = (
    SELECT h.ScheduleID 
    FROM HDSchedule h 
    WHERE h.PatientID = HDLogs.PatientID 
    AND DATE(h.SessionDate) = DATE(HDLogs.SessionDate)
    LIMIT 1
)
WHERE ScheduleID IS NULL;

-- Similarly for IntraDialyticRecords
ALTER TABLE IntraDialyticRecords ADD COLUMN ScheduleID INTEGER;
CREATE INDEX IF NOT EXISTS idx_intradialytic_scheduleid ON IntraDialyticRecords(ScheduleID);

UPDATE IntraDialyticRecords
SET ScheduleID = (
    SELECT h.ScheduleID 
    FROM HDSchedule h 
    WHERE h.PatientID = IntraDialyticRecords.PatientID 
    AND DATE(h.SessionDate) = DATE(IntraDialyticRecords.SessionDate)
    LIMIT 1
)
WHERE ScheduleID IS NULL;

-- Similarly for PostDialysisMedications
ALTER TABLE PostDialysisMedications ADD COLUMN ScheduleID INTEGER;
CREATE INDEX IF NOT EXISTS idx_medications_scheduleid ON PostDialysisMedications(ScheduleID);

UPDATE PostDialysisMedications
SET ScheduleID = (
    SELECT h.ScheduleID 
    FROM HDSchedule h 
    WHERE h.PatientID = PostDialysisMedications.PatientID 
    AND DATE(h.SessionDate) = DATE(PostDialysisMedications.SessionDate)
    LIMIT 1
)
WHERE ScheduleID IS NULL;
