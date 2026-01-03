-- Check if PatientActivityLog table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'PatientActivityLog';

-- If it exists, show its structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PatientActivityLog'
ORDER BY ORDINAL_POSITION;

-- Show sample data
SELECT TOP 5 * FROM PatientActivityLog ORDER BY CreatedAt DESC;
