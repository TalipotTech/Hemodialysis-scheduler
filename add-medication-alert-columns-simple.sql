-- Simple version - Add Medication and Alert columns to HDSchedule table
USE [HDSchedulerDB];
GO

ALTER TABLE HDSchedule ADD MedicationType NVARCHAR(100) NULL;
ALTER TABLE HDSchedule ADD MedicationName NVARCHAR(200) NULL;
ALTER TABLE HDSchedule ADD Dose NVARCHAR(100) NULL;
ALTER TABLE HDSchedule ADD Route NVARCHAR(50) NULL;
ALTER TABLE HDSchedule ADD AdministeredAt NVARCHAR(50) NULL;
ALTER TABLE HDSchedule ADD AlertType NVARCHAR(100) NULL;
ALTER TABLE HDSchedule ADD AlertMessage NVARCHAR(MAX) NULL;
ALTER TABLE HDSchedule ADD Severity NVARCHAR(50) NULL;
ALTER TABLE HDSchedule ADD Resolution NVARCHAR(MAX) NULL;
GO

SELECT 'Migration completed successfully!' AS Status;
