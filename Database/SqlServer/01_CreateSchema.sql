-- ============================================
-- Hemodialysis Scheduler Database Schema
-- SQL Server Version
-- ============================================

USE [hds-dev-db];
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [UserID] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) UNIQUE NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL CHECK ([Role] IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        [LastLogin] DATETIME2 NULL
    );
END
GO

-- Slots Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Slots]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Slots] (
        [SlotID] INT PRIMARY KEY,
        [SlotName] NVARCHAR(50) NOT NULL,
        [StartTime] TIME NOT NULL,
        [EndTime] TIME NOT NULL,
        [BedCapacity] INT DEFAULT 10,
        [MaxBeds] INT DEFAULT 10,
        [IsActive] BIT DEFAULT 1
    );
END
GO

-- Staff Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Staff]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Staff] (
        [StaffID] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL CHECK ([Role] IN ('Doctor', 'Nurse', 'Technician', 'HOD')),
        [ContactNumber] NVARCHAR(20),
        [StaffSpecialization] NVARCHAR(100),
        [AssignedSlot] INT,
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([AssignedSlot]) REFERENCES [dbo].[Slots]([SlotID])
    );
END
GO

-- Patients Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Patients] (
        [PatientID] INT IDENTITY(1,1) PRIMARY KEY,
        [MRN] NVARCHAR(50) UNIQUE,
        [Name] NVARCHAR(200) NOT NULL,
        [Age] INT NOT NULL CHECK ([Age] > 0 AND [Age] < 150),
        [Gender] NVARCHAR(10) CHECK ([Gender] IN ('Male', 'Female', 'Other')),
        [ContactNumber] NVARCHAR(20) NOT NULL,
        [EmergencyContact] NVARCHAR(20),
        [Address] NVARCHAR(500),
        [GuardianName] NVARCHAR(200),
        [DryWeight] DECIMAL(5,2),
        [HDCycle] NVARCHAR(20),
        [HDFrequency] INT,
        [HDStartDate] DATE,
        [DialyserType] NVARCHAR(20),
        [DialyserModel] NVARCHAR(100),
        [PrescribedDuration] DECIMAL(3,1),
        [PrescribedBFR] INT,
        [DialysatePrescription] NVARCHAR(100),
        [DialyserCount] INT DEFAULT 0,
        [BloodTubingCount] INT DEFAULT 0,
        [TotalDialysisCompleted] INT DEFAULT 0,
        [DialysersPurchased] INT DEFAULT 0,
        [BloodTubingPurchased] INT DEFAULT 0,
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
    );
END
GO

-- HDSchedule Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HDSchedule] (
        [ScheduleID] INT IDENTITY(1,1) PRIMARY KEY,
        [PatientID] INT NOT NULL,
        [SessionDate] DATE NOT NULL,
        [DryWeight] DECIMAL(5,2),
        [HDStartDate] DATE,
        [HDCycle] NVARCHAR(20),
        [WeightGain] DECIMAL(4,2),
        [DialyserType] NVARCHAR(20) CHECK ([DialyserType] IN ('HI', 'LO')),
        [DialyserModel] NVARCHAR(100),
        [DialyserReuseCount] INT DEFAULT 0,
        [BloodTubingReuse] INT DEFAULT 0,
        [HDUnitNumber] NVARCHAR(20),
        [PrescribedDuration] DECIMAL(3,1),
        [UFGoal] DECIMAL(5,2),
        [DialysatePrescription] NVARCHAR(100),
        [PrescribedBFR] INT,
        [AnticoagulationType] NVARCHAR(50),
        [HeparinDose] DECIMAL(7,2),
        [SyringeType] NVARCHAR(50),
        [BolusDose] DECIMAL(7,2),
        [HeparinInfusionRate] DECIMAL(5,2),
        [AccessType] NVARCHAR(50),
        [BloodPressure] NVARCHAR(20),
        [Symptoms] NVARCHAR(500),
        [BloodTestDone] BIT DEFAULT 0,
        [SlotID] INT,
        [BedNumber] INT CHECK ([BedNumber] BETWEEN 1 AND 10),
        [AssignedDoctor] INT,
        [AssignedNurse] INT,
        [CreatedByStaffName] NVARCHAR(200),
        [CreatedByStaffRole] NVARCHAR(50),
        [SessionStatus] NVARCHAR(50),
        [TreatmentStartTime] DATETIME2,
        [TreatmentCompletionTime] DATETIME2,
        [DischargeTime] DATETIME2,
        [IsMovedToHistory] BIT DEFAULT 0,
        [IsDischarged] BIT DEFAULT 0,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID]),
        FOREIGN KEY ([SlotID]) REFERENCES [dbo].[Slots]([SlotID]),
        FOREIGN KEY ([AssignedDoctor]) REFERENCES [dbo].[Staff]([StaffID]),
        FOREIGN KEY ([AssignedNurse]) REFERENCES [dbo].[Staff]([StaffID])
    );
END
GO

-- BedAssignments Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BedAssignments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BedAssignments] (
        [AssignmentID] INT IDENTITY(1,1) PRIMARY KEY,
        [PatientID] INT NOT NULL,
        [SlotID] INT NOT NULL,
        [BedNumber] INT NOT NULL CHECK ([BedNumber] BETWEEN 1 AND 10),
        [AssignmentDate] DATE NOT NULL,
        [IsActive] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        [DischargedAt] DATETIME2,
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID]),
        FOREIGN KEY ([SlotID]) REFERENCES [dbo].[Slots]([SlotID])
    );
END
GO

-- HDLogs Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HDLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HDLogs] (
        [LogID] INT IDENTITY(1,1) PRIMARY KEY,
        [ScheduleID] INT,
        [PatientID] INT NOT NULL,
        [SessionDate] DATE NOT NULL,
        [PreWeight] DECIMAL(5,2),
        [PostWeight] DECIMAL(5,2),
        [PreBP] NVARCHAR(20),
        [PostBP] NVARCHAR(20),
        [PrePulse] INT,
        [PostPulse] INT,
        [StartTime] TIME,
        [EndTime] TIME,
        [TotalUF] DECIMAL(5,2),
        [BloodFlowRate] INT,
        [DialysateFlow] INT,
        [Remarks] NVARCHAR(1000),
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([ScheduleID]) REFERENCES [dbo].[HDSchedule]([ScheduleID]),
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID])
    );
END
GO

-- IntraDialyticRecords Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntraDialyticRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[IntraDialyticRecords] (
        [RecordID] INT IDENTITY(1,1) PRIMARY KEY,
        [ScheduleID] INT,
        [PatientID] INT NOT NULL,
        [SessionDate] DATE NOT NULL,
        [RecordTime] TIME NOT NULL,
        [BP] NVARCHAR(20),
        [Pulse] INT,
        [UFRate] DECIMAL(5,2),
        [VenousPressure] INT,
        [BloodFlow] INT,
        [Symptoms] NVARCHAR(500),
        [Intervention] NVARCHAR(500),
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([ScheduleID]) REFERENCES [dbo].[HDSchedule]([ScheduleID]),
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID])
    );
END
GO

-- PostDialysisMedications Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostDialysisMedications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostDialysisMedications] (
        [MedicationID] INT IDENTITY(1,1) PRIMARY KEY,
        [ScheduleID] INT,
        [PatientID] INT NOT NULL,
        [SessionDate] DATE NOT NULL,
        [MedicationName] NVARCHAR(200) NOT NULL,
        [Dosage] NVARCHAR(50),
        [Route] NVARCHAR(50),
        [GivenTime] TIME,
        [GivenBy] NVARCHAR(200),
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([ScheduleID]) REFERENCES [dbo].[HDSchedule]([ScheduleID]),
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID])
    );
END
GO

-- PatientHistory Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PatientHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PatientHistory] (
        [HistoryID] INT IDENTITY(1,1) PRIMARY KEY,
        [PatientID] INT NOT NULL,
        [SessionDate] DATE NOT NULL,
        [SlotID] INT,
        [BedNumber] INT,
        [DryWeight] DECIMAL(5,2),
        [HDStartDate] DATE,
        [HDCycle] NVARCHAR(20),
        [HDFrequency] INT,
        [WeightGain] DECIMAL(4,2),
        [DialyserType] NVARCHAR(20),
        [DialyserModel] NVARCHAR(100),
        [DialyserReuseCount] INT,
        [BloodTubingReuse] INT,
        [HDUnitNumber] NVARCHAR(20),
        [PrescribedDuration] DECIMAL(3,1),
        [UFGoal] DECIMAL(5,2),
        [DialysatePrescription] NVARCHAR(100),
        [PrescribedBFR] INT,
        [AnticoagulationType] NVARCHAR(50),
        [HeparinDose] DECIMAL(7,2),
        [SyringeType] NVARCHAR(50),
        [BolusDose] DECIMAL(7,2),
        [HeparinInfusionRate] DECIMAL(5,2),
        [AccessType] NVARCHAR(50),
        [BloodPressure] NVARCHAR(20),
        [Symptoms] NVARCHAR(500),
        [BloodTestDone] BIT,
        [AssignedDoctor] INT,
        [AssignedNurse] INT,
        [SessionStatus] NVARCHAR(50),
        [TreatmentStartTime] DATETIME2,
        [TreatmentCompletionTime] DATETIME2,
        [DischargeTime] DATETIME2,
        [MovedToHistoryAt] DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients]([PatientID]),
        FOREIGN KEY ([SlotID]) REFERENCES [dbo].[Slots]([SlotID])
    );
END
GO

-- AuditLogs Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [LogID] INT IDENTITY(1,1) PRIMARY KEY,
        [Timestamp] DATETIME2 DEFAULT GETUTCDATE(),
        [Username] NVARCHAR(100),
        [Role] NVARCHAR(50),
        [Action] NVARCHAR(200) NOT NULL,
        [EntityType] NVARCHAR(100),
        [EntityID] INT,
        [Details] NVARCHAR(MAX),
        [IPAddress] NVARCHAR(50)
    );
END
GO

-- Create Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDSchedule_PatientID' AND object_id = OBJECT_ID('HDSchedule'))
    CREATE INDEX IX_HDSchedule_PatientID ON HDSchedule(PatientID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDSchedule_SessionDate' AND object_id = OBJECT_ID('HDSchedule'))
    CREATE INDEX IX_HDSchedule_SessionDate ON HDSchedule(SessionDate);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDSchedule_SlotID' AND object_id = OBJECT_ID('HDSchedule'))
    CREATE INDEX IX_HDSchedule_SlotID ON HDSchedule(SlotID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BedAssignments_PatientID' AND object_id = OBJECT_ID('BedAssignments'))
    CREATE INDEX IX_BedAssignments_PatientID ON BedAssignments(PatientID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDLogs_PatientID' AND object_id = OBJECT_ID('HDLogs'))
    CREATE INDEX IX_HDLogs_PatientID ON HDLogs(PatientID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HDLogs_ScheduleID' AND object_id = OBJECT_ID('HDLogs'))
    CREATE INDEX IX_HDLogs_ScheduleID ON HDLogs(ScheduleID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PatientHistory_PatientID' AND object_id = OBJECT_ID('PatientHistory'))
    CREATE INDEX IX_PatientHistory_PatientID ON PatientHistory(PatientID);
GO

PRINT 'Database schema created successfully!';
GO
