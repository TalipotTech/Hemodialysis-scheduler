-- Add missing prescription fields to Patients table
-- Run this with: sqlite3 HDScheduler.db < add-patient-prescription-fields.sql

-- Add DialyserModel column
ALTER TABLE Patients ADD COLUMN DialyserModel TEXT;

-- Add PrescribedDuration column
ALTER TABLE Patients ADD COLUMN PrescribedDuration REAL;

-- Add PrescribedBFR column
ALTER TABLE Patients ADD COLUMN PrescribedBFR INTEGER;

-- Add DialysatePrescription column
ALTER TABLE Patients ADD COLUMN DialysatePrescription TEXT;

-- Verify the changes
SELECT sql FROM sqlite_master WHERE type='table' AND name='Patients';
