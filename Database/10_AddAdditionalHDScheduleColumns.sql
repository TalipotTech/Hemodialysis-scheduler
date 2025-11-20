-- Migration Script: Add additional HD monitoring columns to HDSchedule table
-- These columns are for HDTreatmentSession, IntraDialyticMonitoring, PostDialysisMedications, and TreatmentAlerts

-- Add DialyserModel column (if not exists)
ALTER TABLE HDSchedule ADD COLUMN DialyserModel TEXT;

-- Add AccessLocation column (if not exists)
ALTER TABLE HDSchedule ADD COLUMN AccessLocation TEXT;

-- HDTreatmentSession fields
ALTER TABLE HDSchedule ADD COLUMN StartTime TEXT;
ALTER TABLE HDSchedule ADD COLUMN PreWeight REAL;
ALTER TABLE HDSchedule ADD COLUMN PreBPSitting TEXT;
ALTER TABLE HDSchedule ADD COLUMN PreTemperature REAL;
ALTER TABLE HDSchedule ADD COLUMN AccessBleedingTime TEXT;
ALTER TABLE HDSchedule ADD COLUMN AccessStatus TEXT;
ALTER TABLE HDSchedule ADD COLUMN Complications TEXT;

-- IntraDialyticMonitoring fields
ALTER TABLE HDSchedule ADD COLUMN MonitoringTime TEXT;
ALTER TABLE HDSchedule ADD COLUMN HeartRate INTEGER;
ALTER TABLE HDSchedule ADD COLUMN ActualBFR INTEGER;
ALTER TABLE HDSchedule ADD COLUMN VenousPressure INTEGER;
ALTER TABLE HDSchedule ADD COLUMN ArterialPressure INTEGER;
ALTER TABLE HDSchedule ADD COLUMN CurrentUFR REAL;
ALTER TABLE HDSchedule ADD COLUMN TotalUFAchieved REAL;
ALTER TABLE HDSchedule ADD COLUMN TmpPressure INTEGER;
ALTER TABLE HDSchedule ADD COLUMN Interventions TEXT;
ALTER TABLE HDSchedule ADD COLUMN StaffInitials TEXT;

-- PostDialysisMedications fields
ALTER TABLE HDSchedule ADD COLUMN MedicationType TEXT;
ALTER TABLE HDSchedule ADD COLUMN MedicationName TEXT;
ALTER TABLE HDSchedule ADD COLUMN Dose TEXT;
ALTER TABLE HDSchedule ADD COLUMN Route TEXT;
ALTER TABLE HDSchedule ADD COLUMN AdministeredAt TEXT;

-- TreatmentAlerts fields
ALTER TABLE HDSchedule ADD COLUMN AlertType TEXT;
ALTER TABLE HDSchedule ADD COLUMN AlertMessage TEXT;
ALTER TABLE HDSchedule ADD COLUMN Severity TEXT;
ALTER TABLE HDSchedule ADD COLUMN Resolution TEXT;

-- Verify migration
SELECT 'Additional HDSchedule columns added successfully' as Status;

-- Show table info
PRAGMA table_info(HDSchedule);
