-- HD Scheduler SQLite Database Schema
-- Version 1.0

-- Drop existing tables if they exist
DROP TABLE IF EXISTS BedAssignments;
DROP TABLE IF EXISTS AuditLogs;
DROP TABLE IF EXISTS HDLogs;
DROP TABLE IF EXISTS IntraDialyticRecords;
DROP TABLE IF EXISTS PostDialysisMedications;
DROP TABLE IF EXISTS Patients;
DROP TABLE IF EXISTS Staff;
DROP TABLE IF EXISTS Slots;
DROP TABLE IF EXISTS Users;

-- Create Users Table
CREATE TABLE Users (
    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL CHECK (Role IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    LastLogin TEXT
);

-- Create Patients Table
CREATE TABLE Patients (
    PatientID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Age INTEGER NOT NULL CHECK (Age > 0 AND Age < 150),
    DryWeight REAL,
    HDStartDate TEXT NOT NULL,
    HDCycle TEXT,
    WeightGain REAL,
    DialyserType TEXT CHECK (DialyserType IN ('HI', 'LO')),
    DialyserReuseCount INTEGER DEFAULT 0,
    BloodTubingReuse INTEGER DEFAULT 0,
    HeparinDose REAL,
    Symptoms TEXT,
    BloodTestDone INTEGER DEFAULT 0,
    BloodPressure TEXT,
    SlotID INTEGER,
    BedNumber INTEGER CHECK (BedNumber BETWEEN 1 AND 10),
    CreatedByStaffName TEXT,
    CreatedByStaffRole TEXT,
    IsDischarged INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now')),
    AssignedDoctor INTEGER,
    AssignedNurse INTEGER,
    MRN TEXT,
    AccessType TEXT,
    PrescribedDuration REAL,
    HDUnitNumber TEXT,
    UFGoal REAL,
    DialysatePrescription TEXT,
    PrescribedBFR INTEGER,
    AnticoagulationType TEXT,
    SyringeType TEXT,
    BolusDose REAL,
    HeparinInfusionRate REAL,
    FOREIGN KEY (AssignedDoctor) REFERENCES Staff(StaffID),
    FOREIGN KEY (AssignedNurse) REFERENCES Staff(StaffID),
    FOREIGN KEY (SlotID) REFERENCES Slots(SlotID)
);

-- Create Staff Table
CREATE TABLE Staff (
    StaffID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Role TEXT NOT NULL CHECK (Role IN ('Doctor', 'Nurse', 'Technician', 'HOD')),
    ContactNumber TEXT,
    StaffSpecialization TEXT,
    AssignedSlot INTEGER,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (AssignedSlot) REFERENCES Slots(SlotID)
);

-- Create Slots Table
CREATE TABLE Slots (
    SlotID INTEGER PRIMARY KEY,
    SlotName TEXT NOT NULL,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    BedCapacity INTEGER DEFAULT 10,
    MaxBeds INTEGER DEFAULT 10,
    IsActive INTEGER DEFAULT 1
);

-- Create BedAssignments Table
CREATE TABLE BedAssignments (
    AssignmentID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SlotID INTEGER NOT NULL,
    BedNumber INTEGER NOT NULL CHECK (BedNumber BETWEEN 1 AND 10),
    AssignmentDate TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    DischargedAt TEXT,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (SlotID) REFERENCES Slots(SlotID)
);

-- Create AuditLogs Table
CREATE TABLE AuditLogs (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID INTEGER,
    Username TEXT,
    Action TEXT NOT NULL,
    EntityType TEXT,
    EntityID INTEGER,
    OldValues TEXT,
    NewValues TEXT,
    IPAddress TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Create HDLogs Table
CREATE TABLE HDLogs (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SessionDate TEXT NOT NULL,
    PreWeight REAL,
    PostWeight REAL,
    WeightLoss REAL,
    BloodPressurePre TEXT,
    BloodPressurePost TEXT,
    Temperature REAL,
    Notes TEXT,
    CreatedBy TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Create IntraDialyticRecords Table
CREATE TABLE IntraDialyticRecords (
    RecordID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SessionDate TEXT NOT NULL,
    TimeRecorded TEXT NOT NULL,
    BloodPressure TEXT,
    PulseRate INTEGER,
    Temperature REAL,
    UFVolume REAL,
    VenousPressure INTEGER,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Create PostDialysisMedications Table
CREATE TABLE PostDialysisMedications (
    MedicationID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SessionDate TEXT NOT NULL,
    MedicationName TEXT NOT NULL,
    Dosage TEXT,
    Route TEXT,
    AdministeredBy TEXT,
    AdministeredAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Create indexes for better performance
CREATE INDEX idx_patients_slotid ON Patients(SlotID);
CREATE INDEX idx_patients_discharged ON Patients(IsDischarged);
CREATE INDEX idx_bedassignments_slotid_date ON BedAssignments(SlotID, AssignmentDate);
CREATE INDEX idx_bedassignments_patientid ON BedAssignments(PatientID);
CREATE INDEX idx_bedassignments_active ON BedAssignments(IsActive);
CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_staff_assignedslot ON Staff(AssignedSlot);
CREATE INDEX idx_auditlogs_userid ON AuditLogs(UserID);
CREATE INDEX idx_hdlogs_patientid ON HDLogs(PatientID);

-- Print success message
SELECT 'SQLite database schema created successfully!' AS Message;
