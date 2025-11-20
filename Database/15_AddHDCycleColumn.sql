-- Add HDCycle column to Patients table
-- This column stores the dialysis schedule pattern (e.g., "Mon-Wed-Fri", "Tue-Thu-Sat")

-- Check if column exists before adding
SELECT CASE 
    WHEN COUNT(*) > 0 THEN 'Column HDCycle already exists'
    ELSE 'Adding HDCycle column'
END as Status
FROM pragma_table_info('Patients') 
WHERE name = 'HDCycle';

-- Add the column if it doesn't exist
ALTER TABLE Patients ADD COLUMN HDCycle VARCHAR(50);

-- Update existing patients with default cycle
UPDATE Patients 
SET HDCycle = 'Mon-Wed-Fri' 
WHERE HDCycle IS NULL;

PRAGMA table_info(Patients);
