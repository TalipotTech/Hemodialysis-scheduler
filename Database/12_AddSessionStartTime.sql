-- Migration 12: Add SessionStartTime column to HDSchedule table
-- This enables tracking when dialysis sessions actually start for auto-status calculation

-- Add SessionStartTime column to HDSchedule table
ALTER TABLE HDSchedule ADD COLUMN SessionStartTime DATETIME NULL;

-- Update existing records to set SessionStartTime based on ScheduleDate and ScheduleTime
UPDATE HDSchedule 
SET SessionStartTime = datetime(ScheduleDate || ' ' || ScheduleTime)
WHERE Status = 'In Progress' OR Status = 'Completed';

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_hdschedule_sessionstarttime 
ON HDSchedule(SessionStartTime);

SELECT 'Migration 12 completed: SessionStartTime column added to HDSchedule table' AS Result;
