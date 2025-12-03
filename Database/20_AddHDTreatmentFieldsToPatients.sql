-- Migration: Add HD Treatment Fields to Patients Table
-- Date: 2025-11-28
-- Description: Add fields for dry weight, HD cycle, dialyser info, equipment counts, and total dialysis sessions

-- Add DryWeight column
ALTER TABLE Patients ADD COLUMN DryWeight REAL;

-- Add HDCycle column (e.g., "3x/week", "2x/week")
ALTER TABLE Patients ADD COLUMN HDCycle TEXT;

-- Add HDStartDate column (when patient started HD treatment)
ALTER TABLE Patients ADD COLUMN HDStartDate TEXT;

-- Add DialyserType column (Hi Flux or Lo Flux)
ALTER TABLE Patients ADD COLUMN DialyserType TEXT CHECK (DialyserType IN ('Hi Flux', 'Lo Flux', NULL));

-- Add DialyserCount column (current usage count)
ALTER TABLE Patients ADD COLUMN DialyserCount INTEGER DEFAULT 0;

-- Add BloodTubingCount column (current usage count)
ALTER TABLE Patients ADD COLUMN BloodTubingCount INTEGER DEFAULT 0;

-- Add TotalDialysisCompleted column (total number of sessions completed)
ALTER TABLE Patients ADD COLUMN TotalDialysisCompleted INTEGER DEFAULT 0;

-- Add comments for documentation
-- DryWeight: Target weight in kg after dialysis
-- HDCycle: Frequency pattern (3x/week, 2x/week, etc.)
-- HDStartDate: Date when patient first started HD treatment
-- DialyserType: Type of dialyser (Hi Flux or Lo Flux)
-- DialyserCount: Current dialyser usage count (auto-incremented after each session)
-- BloodTubingCount: Current blood tubing usage count (auto-incremented after each session)
-- TotalDialysisCompleted: Total number of dialysis sessions completed by patient

-- Verify the changes
SELECT name, type FROM pragma_table_info('Patients') WHERE name IN (
    'DryWeight', 'HDCycle', 'HDStartDate', 'DialyserType', 
    'DialyserCount', 'BloodTubingCount', 'TotalDialysisCompleted'
);
