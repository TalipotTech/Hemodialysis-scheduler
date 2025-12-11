-- ============================================
-- Migration: Add Missing HD Schedule Columns for Comprehensive Treatment Tracking
-- SQL Server Version for Azure SQL
-- ============================================

USE [hds-dev-db];
GO

PRINT 'Starting migration: Adding missing HD Schedule columns...';
GO

-- Check and add StartTime column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'StartTime')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [StartTime] NVARCHAR(50) NULL;
    PRINT '✓ Added column: StartTime';
END
ELSE
    PRINT '⊘ Column already exists: StartTime';
GO

-- Check and add AccessLocation column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AccessLocation')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AccessLocation] NVARCHAR(100) NULL;
    PRINT '✓ Added column: AccessLocation';
END
ELSE
    PRINT '⊘ Column already exists: AccessLocation';
GO

-- Pre-Dialysis Assessment Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PreWeight')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PreWeight] DECIMAL(5,2) NULL;
    PRINT '✓ Added column: PreWeight';
END
ELSE
    PRINT '⊘ Column already exists: PreWeight';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PreBPSitting')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PreBPSitting] NVARCHAR(20) NULL;
    PRINT '✓ Added column: PreBPSitting';
END
ELSE
    PRINT '⊘ Column already exists: PreBPSitting';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PreTemperature')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PreTemperature] DECIMAL(4,1) NULL;
    PRINT '✓ Added column: PreTemperature';
END
ELSE
    PRINT '⊘ Column already exists: PreTemperature';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AccessBleedingTime')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AccessBleedingTime] NVARCHAR(50) NULL;
    PRINT '✓ Added column: AccessBleedingTime';
END
ELSE
    PRINT '⊘ Column already exists: AccessBleedingTime';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AccessStatus')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AccessStatus] NVARCHAR(100) NULL;
    PRINT '✓ Added column: AccessStatus';
END
ELSE
    PRINT '⊘ Column already exists: AccessStatus';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Complications')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Complications] NVARCHAR(500) NULL;
    PRINT '✓ Added column: Complications';
END
ELSE
    PRINT '⊘ Column already exists: Complications';
GO

-- Intra-Dialytic Monitoring Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'MonitoringTime')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [MonitoringTime] NVARCHAR(50) NULL;
    PRINT '✓ Added column: MonitoringTime';
END
ELSE
    PRINT '⊘ Column already exists: MonitoringTime';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'HeartRate')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [HeartRate] INT NULL;
    PRINT '✓ Added column: HeartRate';
END
ELSE
    PRINT '⊘ Column already exists: HeartRate';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'ActualBFR')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [ActualBFR] INT NULL;
    PRINT '✓ Added column: ActualBFR';
END
ELSE
    PRINT '⊘ Column already exists: ActualBFR';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'VenousPressure')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [VenousPressure] INT NULL;
    PRINT '✓ Added column: VenousPressure';
END
ELSE
    PRINT '⊘ Column already exists: VenousPressure';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'ArterialPressure')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [ArterialPressure] INT NULL;
    PRINT '✓ Added column: ArterialPressure';
END
ELSE
    PRINT '⊘ Column already exists: ArterialPressure';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'CurrentUFR')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [CurrentUFR] DECIMAL(5,2) NULL;
    PRINT '✓ Added column: CurrentUFR';
END
ELSE
    PRINT '⊘ Column already exists: CurrentUFR';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'TotalUFAchieved')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [TotalUFAchieved] DECIMAL(5,2) NULL;
    PRINT '✓ Added column: TotalUFAchieved';
END
ELSE
    PRINT '⊘ Column already exists: TotalUFAchieved';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'TmpPressure')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [TmpPressure] INT NULL;
    PRINT '✓ Added column: TmpPressure';
END
ELSE
    PRINT '⊘ Column already exists: TmpPressure';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Interventions')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Interventions] NVARCHAR(1000) NULL;
    PRINT '✓ Added column: Interventions';
END
ELSE
    PRINT '⊘ Column already exists: Interventions';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'StaffInitials')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [StaffInitials] NVARCHAR(50) NULL;
    PRINT '✓ Added column: StaffInitials';
END
ELSE
    PRINT '⊘ Column already exists: StaffInitials';
GO

-- Post-Dialysis Medications Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'MedicationType')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [MedicationType] NVARCHAR(100) NULL;
    PRINT '✓ Added column: MedicationType';
END
ELSE
    PRINT '⊘ Column already exists: MedicationType';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'MedicationName')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [MedicationName] NVARCHAR(200) NULL;
    PRINT '✓ Added column: MedicationName';
END
ELSE
    PRINT '⊘ Column already exists: MedicationName';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Dose')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Dose] NVARCHAR(100) NULL;
    PRINT '✓ Added column: Dose';
END
ELSE
    PRINT '⊘ Column already exists: Dose';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Route')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Route] NVARCHAR(100) NULL;
    PRINT '✓ Added column: Route';
END
ELSE
    PRINT '⊘ Column already exists: Route';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AdministeredAt')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AdministeredAt] NVARCHAR(50) NULL;
    PRINT '✓ Added column: AdministeredAt';
END
ELSE
    PRINT '⊘ Column already exists: AdministeredAt';
GO

-- Treatment Alerts Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AlertType')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AlertType] NVARCHAR(100) NULL;
    PRINT '✓ Added column: AlertType';
END
ELSE
    PRINT '⊘ Column already exists: AlertType';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'AlertMessage')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [AlertMessage] NVARCHAR(500) NULL;
    PRINT '✓ Added column: AlertMessage';
END
ELSE
    PRINT '⊘ Column already exists: AlertMessage';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Severity')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Severity] NVARCHAR(50) NULL;
    PRINT '✓ Added column: Severity';
END
ELSE
    PRINT '⊘ Column already exists: Severity';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Resolution')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Resolution] NVARCHAR(500) NULL;
    PRINT '✓ Added column: Resolution';
END
ELSE
    PRINT '⊘ Column already exists: Resolution';
GO

-- Post-Dialysis Vital Signs Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PostWeight')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PostWeight] DECIMAL(5,2) NULL;
    PRINT '✓ Added column: PostWeight';
END
ELSE
    PRINT '⊘ Column already exists: PostWeight';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PostSBP')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PostSBP] INT NULL;
    PRINT '✓ Added column: PostSBP';
END
ELSE
    PRINT '⊘ Column already exists: PostSBP';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PostDBP')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PostDBP] INT NULL;
    PRINT '✓ Added column: PostDBP';
END
ELSE
    PRINT '⊘ Column already exists: PostDBP';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PostHR')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PostHR] INT NULL;
    PRINT '✓ Added column: PostHR';
END
ELSE
    PRINT '⊘ Column already exists: PostHR';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'PostAccessStatus')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [PostAccessStatus] NVARCHAR(100) NULL;
    PRINT '✓ Added column: PostAccessStatus';
END
ELSE
    PRINT '⊘ Column already exists: PostAccessStatus';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'TotalFluidRemoved')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [TotalFluidRemoved] DECIMAL(5,2) NULL;
    PRINT '✓ Added column: TotalFluidRemoved';
END
ELSE
    PRINT '⊘ Column already exists: TotalFluidRemoved';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'Notes')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [Notes] NVARCHAR(2000) NULL;
    PRINT '✓ Added column: Notes';
END
ELSE
    PRINT '⊘ Column already exists: Notes';
GO

-- Session History Tracking Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'IsAutoGenerated')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [IsAutoGenerated] BIT DEFAULT 0;
    PRINT '✓ Added column: IsAutoGenerated';
END
ELSE
    PRINT '⊘ Column already exists: IsAutoGenerated';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') AND name = 'ParentScheduleID')
BEGIN
    ALTER TABLE [dbo].[HDSchedule] ADD [ParentScheduleID] INT NULL;
    PRINT '✓ Added column: ParentScheduleID';
END
ELSE
    PRINT '⊘ Column already exists: ParentScheduleID';
GO

PRINT '';
PRINT '============================================';
PRINT 'Migration completed successfully!';
PRINT 'Added 33 new columns to HDSchedule table';
PRINT '============================================';
GO
