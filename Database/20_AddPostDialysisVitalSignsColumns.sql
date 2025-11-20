-- Migration: Add Post-Dialysis Vital Signs Columns to HDSchedule
-- Date: 2025-11-20
-- Description: Adds columns for post-dialysis assessment data

-- Add Post-Dialysis Vital Signs columns
ALTER TABLE HDSchedule ADD COLUMN PostWeight REAL;
ALTER TABLE HDSchedule ADD COLUMN PostSBP INTEGER;
ALTER TABLE HDSchedule ADD COLUMN PostDBP INTEGER;
ALTER TABLE HDSchedule ADD COLUMN PostHR INTEGER;
ALTER TABLE HDSchedule ADD COLUMN PostAccessStatus TEXT;
ALTER TABLE HDSchedule ADD COLUMN TotalFluidRemoved REAL;
ALTER TABLE HDSchedule ADD COLUMN Notes TEXT;

-- Verify the columns were added
SELECT 'Post-Dialysis Vital Signs columns added successfully' AS Status;
