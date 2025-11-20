-- Migration 14: Add HD Cycle and Frequency columns to Patients table
-- This allows tracking of dialysis frequency patterns for each patient

-- Add HDCycle column (pattern like MWF, TTS, Daily, etc.)
ALTER TABLE Patients ADD COLUMN HDCycle TEXT;

-- Add HDFrequency column (number of sessions per week)
ALTER TABLE Patients ADD COLUMN HDFrequency INTEGER;

-- Add comments/index if needed
-- UPDATE Patients SET HDCycle = 'MWF', HDFrequency = 3 WHERE HDCycle IS NULL;
