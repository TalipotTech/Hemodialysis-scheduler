-- Hemodialysis Scheduler Database Schema
-- Version 1.0

-- Drop existing tables if they exist (for clean install)
IF OBJECT_ID('BedAssignments', 'U') IS NOT NULL DROP TABLE BedAssignments;
IF OBJECT_ID('Patients', 'U') IS NOT NULL DROP TABLE Patients;
IF OBJECT_ID('Staff', 'U') IS NOT NULL DROP TABLE Staff;
IF OBJECT_ID('Slots', 'U') IS NOT NULL DROP TABLE Slots;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

-- Create Users Table
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);
GO

-- Create Patients Table
CREATE TABLE Patients (
    PatientID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Age INT NOT NULL CHECK (Age > 0 AND Age < 150),
    DryWeight DECIMAL(5,2),
    HDStartDate DATE NOT NULL,
    HDCycle NVARCHAR(50),
    WeightGain DECIMAL(5,2),
    DialyserType NVARCHAR(2) CHECK (DialyserType IN ('HI', 'LO')),
    DialyserReuseCount INT DEFAULT 0,
    BloodTubingReuse BIT DEFAULT 0,
    HeparinDose DECIMAL(5,2),
    Symptoms NVARCHAR(MAX),
    BloodTestDone BIT DEFAULT 0,
    BloodPressure NVARCHAR(20),
    SlotID INT,
    BedNumber INT CHECK (BedNumber BETWEEN 1 AND 10),
    CreatedByStaffName NVARCHAR(100),
    CreatedByStaffRole NVARCHAR(20),
    IsDischarged BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Create Staff Table
CREATE TABLE Staff (
    StaffID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Doctor', 'Nurse', 'Technician', 'HOD')),
    AssignedSlot INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Create Slots Table
CREATE TABLE Slots (
    SlotID INT PRIMARY KEY,
    SlotName NVARCHAR(50) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    BedCapacity INT DEFAULT 10
);
GO

-- Create BedAssignments Table
CREATE TABLE BedAssignments (
    AssignmentID INT PRIMARY KEY IDENTITY(1,1),
    PatientID INT NOT NULL,
    SlotID INT NOT NULL,
    BedNumber INT NOT NULL CHECK (BedNumber BETWEEN 1 AND 10),
    AssignmentDate DATE NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    DischargedAt DATETIME NULL,
    CONSTRAINT FK_BedAssignments_Patients FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    CONSTRAINT FK_BedAssignments_Slots FOREIGN KEY (SlotID) REFERENCES Slots(SlotID)
);
GO

-- Create indexes for better performance
CREATE INDEX IX_Patients_SlotID ON Patients(SlotID);
CREATE INDEX IX_Patients_IsDischarged ON Patients(IsDischarged);
CREATE INDEX IX_BedAssignments_SlotID_Date ON BedAssignments(SlotID, AssignmentDate);
CREATE INDEX IX_BedAssignments_PatientID ON BedAssignments(PatientID);
CREATE INDEX IX_BedAssignments_IsActive ON BedAssignments(IsActive);
CREATE INDEX IX_Users_Username ON Users(Username);
GO

PRINT 'Database schema created successfully!';
