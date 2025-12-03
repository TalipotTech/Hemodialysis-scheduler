-- Add treatment timing columns for hybrid discharge approach
-- This enables automatic session completion with manual discharge confirmation

-- Add new columns to HDSchedule table
ALTER TABLE HDSchedule ADD COLUMN TreatmentStartTime TEXT NULL;
ALTER TABLE HDSchedule ADD COLUMN TreatmentCompletionTime TEXT NULL;
ALTER TABLE HDSchedule ADD COLUMN DischargeTime TEXT NULL;

-- Update SessionStatus to include new statuses
-- Possible values: Pre-Scheduled, Active, Completed, Ready-For-Discharge, Discharged, Missed, Cancelled

-- Update existing active sessions to set TreatmentStartTime if they have a StartTime
UPDATE HDSchedule 
SET TreatmentStartTime = datetime('now')
WHERE SessionStatus = 'Active' 
  AND TreatmentStartTime IS NULL
  AND StartTime IS NOT NULL;

-- Mark sessions as Ready-For-Discharge if prescribed duration has elapsed
UPDATE HDSchedule
SET SessionStatus = 'Ready-For-Discharge',
    TreatmentCompletionTime = datetime(TreatmentStartTime, '+' || CAST(PrescribedDuration AS TEXT) || ' hours')
WHERE SessionStatus = 'Active'
  AND TreatmentStartTime IS NOT NULL
  AND PrescribedDuration IS NOT NULL
  AND datetime('now') >= datetime(TreatmentStartTime, '+' || CAST(PrescribedDuration AS TEXT) || ' hours');

PRAGMA foreign_keys=off;
PRAGMA foreign_keys=on;
