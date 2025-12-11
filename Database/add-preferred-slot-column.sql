-- Add PreferredSlotID column to Patients table
-- This stores the patient's preferred time slot for recurring HD sessions

-- Check if column exists before adding
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Patients' 
    AND COLUMN_NAME = 'PreferredSlotID'
)
BEGIN
    ALTER TABLE Patients
    ADD PreferredSlotID INT NULL;
    
    PRINT 'PreferredSlotID column added to Patients table successfully.';
END
ELSE
BEGIN
    PRINT 'PreferredSlotID column already exists in Patients table.';
END
GO

-- Add a check constraint to ensure PreferredSlotID is between 1 and 4 (Morning, Afternoon, Evening, Night)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_Patients_PreferredSlotID'
)
BEGIN
    ALTER TABLE Patients
    ADD CONSTRAINT CK_Patients_PreferredSlotID 
    CHECK (PreferredSlotID IS NULL OR (PreferredSlotID >= 1 AND PreferredSlotID <= 4));
    
    PRINT 'Check constraint for PreferredSlotID added successfully.';
END
ELSE
BEGIN
    PRINT 'Check constraint for PreferredSlotID already exists.';
END
GO
