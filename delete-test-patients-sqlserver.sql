-- ============================================
-- SQL Server: Delete Test Patients Script
-- Execute in SQL Server Management Studio
-- ============================================

USE [hds-dev-db];
GO

-- 1. Delete specific patients by name
PRINT 'Deleting Achu and mayavi m...';
DELETE FROM Patients WHERE Name IN ('Achu', 'mayavi m');

-- 2. Delete patients by ID range (14-37)
PRINT 'Deleting PatientID 14-37...';
DELETE FROM Patients WHERE PatientID BETWEEN 14 AND 37;

-- 3. Verify deletions
PRINT 'Remaining patients:';
SELECT PatientID, Name, MRN, Age, Gender, ContactNumber, IsActive 
FROM Patients 
ORDER BY PatientID;

-- 4. Show count
PRINT 'Total patient count:';
SELECT COUNT(*) as TotalPatients FROM Patients WHERE IsActive = 1;

GO
