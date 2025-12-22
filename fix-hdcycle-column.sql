-- Increase HDCycle column size in Patients table

-- Check current size
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Patients' 
AND COLUMN_NAME = 'HDCycle';

-- Increase HDCycle to NVARCHAR(100) to accommodate full day names
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Patients' AND COLUMN_NAME = 'HDCycle'
)
BEGIN
    ALTER TABLE Patients
    ALTER COLUMN HDCycle NVARCHAR(100) NULL;
    PRINT 'HDCycle column increased to NVARCHAR(100)';
END
ELSE
BEGIN
    PRINT 'HDCycle column does not exist';
END
