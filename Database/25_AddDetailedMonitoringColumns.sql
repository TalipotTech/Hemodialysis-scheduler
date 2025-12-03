-- Migration: Add detailed monitoring columns to IntraDialyticRecords table
-- This allows storing all vital monitoring data captured during treatment

-- Check if columns exist, if not add them
-- SQLite doesn't support IF NOT EXISTS for ALTER TABLE, so we'll check in code

-- Add additional monitoring columns
ALTER TABLE IntraDialyticRecords ADD COLUMN ArterialPressure INTEGER;
ALTER TABLE IntraDialyticRecords ADD COLUMN CurrentUFR REAL;
ALTER TABLE IntraDialyticRecords ADD COLUMN TMPPressure INTEGER;
ALTER TABLE IntraDialyticRecords ADD COLUMN Symptoms TEXT;
ALTER TABLE IntraDialyticRecords ADD COLUMN Interventions TEXT;
ALTER TABLE IntraDialyticRecords ADD COLUMN StaffInitials TEXT;
ALTER TABLE IntraDialyticRecords ADD COLUMN RecordedBy TEXT;

-- Note: BloodFlowRate and DialysateFlowRate should already exist from migration 07
-- If they don't exist, they will be added by the DatabaseInitializer
