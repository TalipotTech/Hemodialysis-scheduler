-- Add HDFrequency column to Patients table
-- This stores the number of dialysis sessions per week

ALTER TABLE Patients ADD COLUMN HDFrequency INTEGER;

-- Set default frequency of 3 sessions per week for existing patients with HDCycle
UPDATE Patients 
SET HDFrequency = 3 
WHERE HDCycle IS NOT NULL AND HDFrequency IS NULL;
