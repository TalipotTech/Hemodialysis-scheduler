-- Migration 11: Add Equipment Usage Alerts Table
-- Purpose: Track equipment usage (Dialyser and Blood Tubing) and alert when limits are reached
-- Dialyser Max Usage: 7 times
-- Blood Tubing Max Usage: 12 times

-- Create EquipmentUsageAlerts table
CREATE TABLE IF NOT EXISTS EquipmentUsageAlerts (
    AlertID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    ScheduleID INTEGER,
    EquipmentType TEXT NOT NULL, -- 'Dialyser' or 'Blood Tubing'
    CurrentUsageCount INTEGER NOT NULL,
    MaxUsageLimit INTEGER NOT NULL,
    Severity TEXT NOT NULL, -- 'Warning', 'Critical', 'Expired'
    AlertMessage TEXT NOT NULL,
    IsAcknowledged INTEGER NOT NULL DEFAULT 0, -- 0 = false, 1 = true
    AcknowledgedBy TEXT,
    AcknowledgedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedules(ScheduleID)
);

-- Create indexes for faster queries
CREATE INDEX IF NOT EXISTS idx_equipment_alerts_patient 
ON EquipmentUsageAlerts(PatientID);

CREATE INDEX IF NOT EXISTS idx_equipment_alerts_schedule 
ON EquipmentUsageAlerts(ScheduleID);

CREATE INDEX IF NOT EXISTS idx_equipment_alerts_unacknowledged 
ON EquipmentUsageAlerts(IsAcknowledged, PatientID);

CREATE INDEX IF NOT EXISTS idx_equipment_alerts_severity 
ON EquipmentUsageAlerts(Severity, IsAcknowledged);

-- Add comments (SQLite doesn't support comments, but useful for documentation)
-- This table tracks when equipment (Dialyser or Blood Tubing) is nearing or has exceeded its safe usage limit
-- Medical staff should receive alerts to inform patients to bring new equipment

PRAGMA table_info(EquipmentUsageAlerts);
