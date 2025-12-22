-- Create TreatmentAlerts table for tracking alerts and incidents during dialysis

USE [hds-dev-db];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TreatmentAlerts')
BEGIN
    CREATE TABLE TreatmentAlerts (
        AlertID INT PRIMARY KEY IDENTITY(1,1),
        ScheduleID INT NOT NULL,
        PatientID INT NOT NULL,
        SessionDate DATE NOT NULL,
        AlertType NVARCHAR(100) NOT NULL,
        AlertMessage NVARCHAR(MAX) NOT NULL,
        Severity NVARCHAR(20) NOT NULL, -- Low, Medium, High, Critical
        Resolution NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        ResolvedAt DATETIME2 NULL,
        CreatedBy INT NULL, -- Staff ID who created the alert
        ResolvedBy INT NULL, -- Staff ID who resolved the alert
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID) ON DELETE CASCADE,
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
        FOREIGN KEY (CreatedBy) REFERENCES Staff(StaffID),
        FOREIGN KEY (ResolvedBy) REFERENCES Staff(StaffID)
    );
    
    PRINT 'TreatmentAlerts table created successfully!';
END
ELSE
BEGIN
    PRINT 'TreatmentAlerts table already exists.';
END
GO

-- Create index for faster queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreatmentAlerts_ScheduleID' AND object_id = OBJECT_ID('TreatmentAlerts'))
BEGIN
    CREATE INDEX IX_TreatmentAlerts_ScheduleID ON TreatmentAlerts(ScheduleID);
    PRINT 'Index IX_TreatmentAlerts_ScheduleID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreatmentAlerts_PatientID' AND object_id = OBJECT_ID('TreatmentAlerts'))
BEGIN
    CREATE INDEX IX_TreatmentAlerts_PatientID ON TreatmentAlerts(PatientID);
    PRINT 'Index IX_TreatmentAlerts_PatientID created';
END
GO

-- Verify table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TreatmentAlerts'
ORDER BY ORDINAL_POSITION;
GO
