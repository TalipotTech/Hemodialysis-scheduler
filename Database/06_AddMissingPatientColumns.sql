-- Add missing columns to Patients table
-- This script adds AssignedDoctor, AssignedNurse, and HD Log fields

USE HDScheduler;
GO

-- Add staff assignment columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'AssignedDoctor')
BEGIN
    ALTER TABLE Patients ADD AssignedDoctor INT NULL;
    PRINT 'Added AssignedDoctor column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'AssignedNurse')
BEGIN
    ALTER TABLE Patients ADD AssignedNurse INT NULL;
    PRINT 'Added AssignedNurse column';
END
GO

-- Add HD Log columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'MRN')
BEGIN
    ALTER TABLE Patients ADD MRN NVARCHAR(50) NULL;
    PRINT 'Added MRN column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'AccessType')
BEGIN
    ALTER TABLE Patients ADD AccessType NVARCHAR(20) NULL;
    PRINT 'Added AccessType column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'PrescribedDuration')
BEGIN
    ALTER TABLE Patients ADD PrescribedDuration DECIMAL(5,2) NULL;
    PRINT 'Added PrescribedDuration column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'HDUnitNumber')
BEGIN
    ALTER TABLE Patients ADD HDUnitNumber NVARCHAR(20) NULL;
    PRINT 'Added HDUnitNumber column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'UFGoal')
BEGIN
    ALTER TABLE Patients ADD UFGoal DECIMAL(5,2) NULL;
    PRINT 'Added UFGoal column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'DialysatePrescription')
BEGIN
    ALTER TABLE Patients ADD DialysatePrescription NVARCHAR(50) NULL;
    PRINT 'Added DialysatePrescription column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'PrescribedBFR')
BEGIN
    ALTER TABLE Patients ADD PrescribedBFR INT NULL;
    PRINT 'Added PrescribedBFR column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'AnticoagulationType')
BEGIN
    ALTER TABLE Patients ADD AnticoagulationType NVARCHAR(50) NULL;
    PRINT 'Added AnticoagulationType column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'SyringeType')
BEGIN
    ALTER TABLE Patients ADD SyringeType NVARCHAR(20) NULL;
    PRINT 'Added SyringeType column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'BolusDose')
BEGIN
    ALTER TABLE Patients ADD BolusDose DECIMAL(5,2) NULL;
    PRINT 'Added BolusDose column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'HeparinInfusionRate')
BEGIN
    ALTER TABLE Patients ADD HeparinInfusionRate DECIMAL(5,2) NULL;
    PRINT 'Added HeparinInfusionRate column';
END
GO

-- Add foreign key constraints for staff assignments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Patients_AssignedDoctor')
BEGIN
    ALTER TABLE Patients 
    ADD CONSTRAINT FK_Patients_AssignedDoctor 
    FOREIGN KEY (AssignedDoctor) REFERENCES Staff(StaffID);
    PRINT 'Added FK_Patients_AssignedDoctor foreign key';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Patients_AssignedNurse')
BEGIN
    ALTER TABLE Patients 
    ADD CONSTRAINT FK_Patients_AssignedNurse 
    FOREIGN KEY (AssignedNurse) REFERENCES Staff(StaffID);
    PRINT 'Added FK_Patients_AssignedNurse foreign key';
END
GO

PRINT 'All missing columns have been added successfully!';
