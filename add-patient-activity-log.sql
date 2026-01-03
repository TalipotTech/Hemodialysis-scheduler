-- Migration: Add PatientActivityLog table for tracking patient events
-- Purpose: Record late arrivals, missed appointments, rescheduling, discharge reasons
-- Date: January 2, 2026

CREATE TABLE IF NOT EXISTS PatientActivityLog (
    ActivityID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    ScheduleID INTEGER NULL,
    ActivityDate TEXT NOT NULL,
    ActivityType TEXT NOT NULL, -- LATE, MISSED, RESCHEDULED, DISCHARGED, NOTE
    Reason TEXT,
    Details TEXT,
    RecordedBy TEXT,
    OldDateTime TEXT, -- For rescheduling
    NewDateTime TEXT, -- For rescheduling
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_patient_activity_patientid ON PatientActivityLog(PatientID);
CREATE INDEX IF NOT EXISTS idx_patient_activity_date ON PatientActivityLog(ActivityDate);
CREATE INDEX IF NOT EXISTS idx_patient_activity_type ON PatientActivityLog(ActivityType);

-- Sample data for testing
INSERT INTO PatientActivityLog (PatientID, ScheduleID, ActivityDate, ActivityType, Reason, Details, RecordedBy, CreatedAt)
VALUES 
    (1, NULL, '2026-01-01', 'LATE', 'Traffic delay', 'Patient called 30 minutes late', 'Nurse Sarah', datetime('now')),
    (1, 123, '2025-12-28', 'MISSED', 'No-Show', 'Patient did not arrive, no call', 'Admin John', datetime('now', '-5 days')),
    (2, NULL, '2025-12-25', 'RESCHEDULED', 'Personal appointment', 'Changed from Morning to Afternoon', 'Receptionist Lisa', datetime('now', '-8 days'));

SELECT 'PatientActivityLog table created successfully!' as Result;
