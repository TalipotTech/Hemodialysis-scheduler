-- Update Treatment Tables to Link to HDSchedule Sessions
-- This adds ScheduleID foreign key to all treatment-related tables

-- Step 1: Create new tables with proper foreign keys
-- ===================================================

-- New HDLogs table with ScheduleID foreign key
CREATE TABLE HDLogs_New (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    ScheduleID INTEGER NOT NULL,  -- Link to HDSchedule session
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
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID) ON DELETE CASCADE,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- New IntraDialyticRecords table with ScheduleID foreign key
CREATE TABLE IntraDialyticRecords_New (
    RecordID INTEGER PRIMARY KEY AUTOINCREMENT,
    ScheduleID INTEGER NOT NULL,  -- Link to HDSchedule session
    PatientID INTEGER NOT NULL,
    SessionDate TEXT NOT NULL,
    TimeRecorded TEXT NOT NULL,
    BloodPressure TEXT,
    PulseRate INTEGER,
    Temperature REAL,
    UFVolume REAL,
    VenousPressure INTEGER,
    BloodFlowRate INTEGER,
    DialysateFlowRate INTEGER,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID) ON DELETE CASCADE,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- New PostDialysisMedications table with ScheduleID foreign key
CREATE TABLE PostDialysisMedications_New (
    MedicationID INTEGER PRIMARY KEY AUTOINCREMENT,
    ScheduleID INTEGER NOT NULL,  -- Link to HDSchedule session
    PatientID INTEGER NOT NULL,
    SessionDate TEXT NOT NULL,
    MedicationName TEXT NOT NULL,
    Dosage TEXT,
    Route TEXT,
    AdministeredBy TEXT,
    AdministeredAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID) ON DELETE CASCADE,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Step 2: Copy data from old tables (if any exists)
-- ===================================================
INSERT INTO HDLogs_New (LogID, ScheduleID, PatientID, SessionDate, PreWeight, PostWeight, WeightLoss, 
                        BloodPressurePre, BloodPressurePost, Temperature, Notes, CreatedBy, CreatedAt)
SELECT LogID, 
       COALESCE((SELECT ScheduleID FROM HDSchedule WHERE HDSchedule.PatientID = HDLogs.PatientID 
                AND HDSchedule.SessionDate = HDLogs.SessionDate LIMIT 1), 0) as ScheduleID,
       PatientID, SessionDate, PreWeight, PostWeight, WeightLoss, 
       BloodPressurePre, BloodPressurePost, Temperature, Notes, CreatedBy, CreatedAt
FROM HDLogs
WHERE EXISTS (SELECT 1 FROM HDLogs);

INSERT INTO IntraDialyticRecords_New (RecordID, ScheduleID, PatientID, SessionDate, TimeRecorded, 
                                     BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt)
SELECT RecordID,
       COALESCE((SELECT ScheduleID FROM HDSchedule WHERE HDSchedule.PatientID = IntraDialyticRecords.PatientID 
                AND HDSchedule.SessionDate = IntraDialyticRecords.SessionDate LIMIT 1), 0) as ScheduleID,
       PatientID, SessionDate, TimeRecorded, 
       BloodPressure, PulseRate, Temperature, UFVolume, VenousPressure, Notes, CreatedAt
FROM IntraDialyticRecords
WHERE EXISTS (SELECT 1 FROM IntraDialyticRecords);

INSERT INTO PostDialysisMedications_New (MedicationID, ScheduleID, PatientID, SessionDate, 
                                        MedicationName, Dosage, Route, AdministeredBy, AdministeredAt)
SELECT MedicationID,
       COALESCE((SELECT ScheduleID FROM HDSchedule WHERE HDSchedule.PatientID = PostDialysisMedications.PatientID 
                AND HDSchedule.SessionDate = PostDialysisMedications.SessionDate LIMIT 1), 0) as ScheduleID,
       PatientID, SessionDate, 
       MedicationName, Dosage, Route, AdministeredBy, AdministeredAt
FROM PostDialysisMedications
WHERE EXISTS (SELECT 1 FROM PostDialysisMedications);

-- Step 3: Drop old tables and rename new ones
-- ===================================================
DROP TABLE IF EXISTS HDLogs;
DROP TABLE IF EXISTS IntraDialyticRecords;
DROP TABLE IF EXISTS PostDialysisMedications;

ALTER TABLE HDLogs_New RENAME TO HDLogs;
ALTER TABLE IntraDialyticRecords_New RENAME TO IntraDialyticRecords;
ALTER TABLE PostDialysisMedications_New RENAME TO PostDialysisMedications;

-- Step 4: Create indexes for performance
-- ===================================================
CREATE INDEX idx_hdlogs_scheduleid ON HDLogs(ScheduleID);
CREATE INDEX idx_hdlogs_patientid_date ON HDLogs(PatientID, SessionDate);
CREATE INDEX idx_intrarecords_scheduleid ON IntraDialyticRecords(ScheduleID);
CREATE INDEX idx_intrarecords_patientid_date ON IntraDialyticRecords(PatientID, SessionDate);
CREATE INDEX idx_medications_scheduleid ON PostDialysisMedications(ScheduleID);
CREATE INDEX idx_medications_patientid_date ON PostDialysisMedications(PatientID, SessionDate);

SELECT 'Treatment tables updated with ScheduleID foreign keys successfully!' AS Message;
