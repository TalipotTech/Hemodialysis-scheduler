-- SQL Server Database Schema for HD Scheduler
-- Execute this on your Azure SQL Server database: hds-dev-db

-- Users Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(100) UNIQUE NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        LastLogin DATETIME
    );
END;

-- Slots Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Slots')
BEGIN
    CREATE TABLE Slots (
        SlotID INT PRIMARY KEY,
        SlotName NVARCHAR(50) NOT NULL,
        StartTime NVARCHAR(20) NOT NULL,
        EndTime NVARCHAR(20) NOT NULL,
        BedCapacity INT DEFAULT 10,
        MaxBeds INT DEFAULT 10,
        IsActive BIT DEFAULT 1
    );
END;

-- Staff Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Staff')
BEGIN
    CREATE TABLE Staff (
        StaffID INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Doctor', 'Nurse', 'Technician', 'HOD')),
        ContactNumber NVARCHAR(20),
        StaffSpecialization NVARCHAR(100),
        AssignedSlot INT,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (AssignedSlot) REFERENCES Slots(SlotID)
    );
END;

-- Patients Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Patients')
BEGIN
    CREATE TABLE Patients (
        PatientID INT PRIMARY KEY IDENTITY(1,1),
        MRN NVARCHAR(50) UNIQUE,
        Name NVARCHAR(200) NOT NULL,
        Age INT NOT NULL CHECK (Age > 0 AND Age < 150),
        Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
        ContactNumber NVARCHAR(20) NOT NULL,
        EmergencyContact NVARCHAR(20),
        Address NVARCHAR(MAX),
        GuardianName NVARCHAR(200),
        DryWeight FLOAT,
        HDCycle NVARCHAR(50),
        HDFrequency INT,
        HDStartDate NVARCHAR(50),
        DialyserType NVARCHAR(50),
        DialyserModel NVARCHAR(100),
        PrescribedDuration FLOAT,
        PrescribedBFR INT,
        DialysatePrescription NVARCHAR(200),
        DialyserCount INT DEFAULT 0,
        BloodTubingCount INT DEFAULT 0,
        TotalDialysisCompleted INT DEFAULT 0,
        DialysersPurchased INT DEFAULT 0,
        BloodTubingPurchased INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE()
    );
END;

-- HDSchedule Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HDSchedule')
BEGIN
    CREATE TABLE HDSchedule (
        ScheduleID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        SessionDate NVARCHAR(50) NOT NULL,
        DryWeight FLOAT,
        HDStartDate NVARCHAR(50),
        HDCycle NVARCHAR(50),
        WeightGain FLOAT,
        DialyserType NVARCHAR(50) CHECK (DialyserType IN ('HI', 'LO')),
        DialyserReuseCount INT DEFAULT 0,
        BloodTubingReuse INT DEFAULT 0,
        HDUnitNumber NVARCHAR(50),
        PrescribedDuration FLOAT,
        UFGoal FLOAT,
        DialysatePrescription NVARCHAR(200),
        PrescribedBFR INT,
        AnticoagulationType NVARCHAR(100),
        HeparinDose FLOAT,
        SyringeType NVARCHAR(50),
        BolusDose FLOAT,
        HeparinInfusionRate FLOAT,
        AccessType NVARCHAR(100),
        BloodPressure NVARCHAR(50),
        Symptoms NVARCHAR(MAX),
        BloodTestDone BIT DEFAULT 0,
        SlotID INT,
        BedNumber INT CHECK (BedNumber BETWEEN 1 AND 10),
        AssignedDoctor INT,
        AssignedNurse INT,
        CreatedByStaffName NVARCHAR(200),
        CreatedByStaffRole NVARCHAR(50),
        SessionStatus NVARCHAR(50),
        TreatmentStartTime NVARCHAR(50),
        TreatmentCompletionTime NVARCHAR(50),
        DischargeTime NVARCHAR(50),
        IsMovedToHistory BIT DEFAULT 0,
        IsDischarged BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (SlotID) REFERENCES Slots(SlotID),
        FOREIGN KEY (AssignedDoctor) REFERENCES Staff(StaffID),
        FOREIGN KEY (AssignedNurse) REFERENCES Staff(StaffID)
    );
END;

-- BedAssignments Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BedAssignments')
BEGIN
    CREATE TABLE BedAssignments (
        AssignmentID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        SlotID INT NOT NULL,
        BedNumber INT NOT NULL CHECK (BedNumber BETWEEN 1 AND 10),
        AssignmentDate NVARCHAR(50) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        DischargedAt NVARCHAR(50),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (SlotID) REFERENCES Slots(SlotID)
    );
END;

-- AuditLogs Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        LogID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT,
        Username NVARCHAR(100),
        Action NVARCHAR(200) NOT NULL,
        EntityType NVARCHAR(100),
        EntityID INT,
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        IPAddress NVARCHAR(50),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- HDLogs Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HDLogs')
BEGIN
    CREATE TABLE HDLogs (
        LogID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT,
        SessionDate NVARCHAR(50) NOT NULL,
        PreWeight FLOAT,
        PostWeight FLOAT,
        WeightLoss FLOAT,
        BloodPressurePre NVARCHAR(50),
        BloodPressurePost NVARCHAR(50),
        Temperature FLOAT,
        Notes NVARCHAR(MAX),
        CreatedBy NVARCHAR(200),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
    );
END;

-- PatientActivityLog Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PatientActivityLog')
BEGIN
    CREATE TABLE PatientActivityLog (
        ActivityID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT NULL,
        ActivityDate NVARCHAR(50) NOT NULL,
        ActivityType NVARCHAR(50) NOT NULL, -- LATE, MISSED, RESCHEDULED, DISCHARGED, NOTE
        Reason NVARCHAR(MAX),
        Details NVARCHAR(MAX),
        RecordedBy NVARCHAR(200),
        OldDateTime NVARCHAR(50),
        NewDateTime NVARCHAR(50),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
    );
END;

-- IntraDialyticRecords Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntraDialyticRecords')
BEGIN
    CREATE TABLE IntraDialyticRecords (
        RecordID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT,
        SessionDate NVARCHAR(50) NOT NULL,
        TimeRecorded NVARCHAR(50) NOT NULL,
        BloodPressure NVARCHAR(50),
        PulseRate INT,
        Temperature FLOAT,
        UFVolume FLOAT,
        VenousPressure INT,
        ArterialPressure INT,
        BloodFlowRate INT,
        DialysateFlowRate INT,
        CurrentUFR FLOAT,
        TMPPressure INT,
        Symptoms NVARCHAR(MAX),
        Interventions NVARCHAR(MAX),
        StaffInitials NVARCHAR(50),
        RecordedBy INT,
        Notes NVARCHAR(MAX),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID),
        FOREIGN KEY (RecordedBy) REFERENCES Staff(StaffID)
    );
END;

-- PostDialysisMedications Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PostDialysisMedications')
BEGIN
    CREATE TABLE PostDialysisMedications (
        MedicationID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT,
        SessionDate NVARCHAR(50) NOT NULL,
        MedicationName NVARCHAR(200) NOT NULL,
        Dosage NVARCHAR(100),
        Route NVARCHAR(50),
        AdministeredBy NVARCHAR(200),
        AdministeredAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
    );
END;

-- EquipmentUsageAlerts Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EquipmentUsageAlerts')
BEGIN
    CREATE TABLE EquipmentUsageAlerts (
        AlertID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT,
        EquipmentType NVARCHAR(100) NOT NULL,
        CurrentUsageCount INT NOT NULL,
        MaxUsageLimit INT NOT NULL,
        Severity NVARCHAR(50) NOT NULL,
        AlertMessage NVARCHAR(MAX) NOT NULL,
        IsAcknowledged BIT NOT NULL DEFAULT 0,
        AcknowledgedBy NVARCHAR(200),
        AcknowledgedAt NVARCHAR(50),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
    );
END;

-- TreatmentAlerts Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TreatmentAlerts')
BEGIN
    CREATE TABLE TreatmentAlerts (
        AlertID INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        ScheduleID INT NOT NULL,
        SessionDate NVARCHAR(50) NOT NULL,
        AlertType NVARCHAR(100) NOT NULL,
        AlertMessage NVARCHAR(MAX),
        Severity NVARCHAR(50) CHECK (Severity IN ('Low', 'Medium', 'High', 'Critical')),
        OccurredAt DATETIME DEFAULT GETDATE(),
        Resolution NVARCHAR(MAX),
        ResolvedAt NVARCHAR(50),
        ResolvedBy NVARCHAR(200),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
    );
END;

-- Create Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patients_mrn')
    CREATE INDEX idx_patients_mrn ON Patients(MRN);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patients_contact')
    CREATE INDEX idx_patients_contact ON Patients(ContactNumber);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patients_name')
    CREATE INDEX idx_patients_name ON Patients(Name);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdschedule_patientid')
    CREATE INDEX idx_hdschedule_patientid ON HDSchedule(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdschedule_sessiondate')
    CREATE INDEX idx_hdschedule_sessiondate ON HDSchedule(SessionDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdschedule_slotid')
    CREATE INDEX idx_hdschedule_slotid ON HDSchedule(SlotID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdschedule_discharged')
    CREATE INDEX idx_hdschedule_discharged ON HDSchedule(IsDischarged);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bedassignments_slotid_date')
    CREATE INDEX idx_bedassignments_slotid_date ON BedAssignments(SlotID, AssignmentDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bedassignments_patientid')
    CREATE INDEX idx_bedassignments_patientid ON BedAssignments(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bedassignments_active')
    CREATE INDEX idx_bedassignments_active ON BedAssignments(IsActive);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_username')
    CREATE INDEX idx_users_username ON Users(Username);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_staff_assignedslot')
    CREATE INDEX idx_staff_assignedslot ON Staff(AssignedSlot);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_auditlogs_userid')
    CREATE INDEX idx_auditlogs_userid ON AuditLogs(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdlogs_patientid')
    CREATE INDEX idx_hdlogs_patientid ON HDLogs(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_hdlogs_scheduleid')
    CREATE INDEX idx_hdlogs_scheduleid ON HDLogs(ScheduleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_intradialytic_scheduleid')
    CREATE INDEX idx_intradialytic_scheduleid ON IntraDialyticRecords(ScheduleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patient_activity_patientid')
    CREATE INDEX idx_patient_activity_patientid ON PatientActivityLog(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patient_activity_date')
    CREATE INDEX idx_patient_activity_date ON PatientActivityLog(ActivityDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_patient_activity_type')
    CREATE INDEX idx_patient_activity_type ON PatientActivityLog(ActivityType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_medications_scheduleid')
    CREATE INDEX idx_medications_scheduleid ON PostDialysisMedications(ScheduleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_equipment_alerts_patient')
    CREATE INDEX idx_equipment_alerts_patient ON EquipmentUsageAlerts(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_equipment_alerts_schedule')
    CREATE INDEX idx_equipment_alerts_schedule ON EquipmentUsageAlerts(ScheduleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_equipment_alerts_unacknowledged')
    CREATE INDEX idx_equipment_alerts_unacknowledged ON EquipmentUsageAlerts(IsAcknowledged, PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_equipment_alerts_severity')
    CREATE INDEX idx_equipment_alerts_severity ON EquipmentUsageAlerts(Severity, IsAcknowledged);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_treatment_alerts_patientid')
    CREATE INDEX idx_treatment_alerts_patientid ON TreatmentAlerts(PatientID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_treatment_alerts_scheduleid')
    CREATE INDEX idx_treatment_alerts_scheduleid ON TreatmentAlerts(ScheduleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_treatment_alerts_severity')
    CREATE INDEX idx_treatment_alerts_severity ON TreatmentAlerts(Severity);

PRINT 'SQL Server schema created successfully!';
