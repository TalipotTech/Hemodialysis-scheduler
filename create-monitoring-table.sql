-- Create IntraDialyticMonitoring table if it doesn't exist

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IntraDialyticMonitoring')
BEGIN
    CREATE TABLE IntraDialyticMonitoring (
        MonitoringID INT IDENTITY(1,1) PRIMARY KEY,
        ScheduleID INT NOT NULL,
        TimeRecorded TIME NOT NULL,
        BloodPressure NVARCHAR(20),
        PulseRate INT,
        Temperature DECIMAL(4,1),
        UFVolume DECIMAL(6,2),
        ArterialPressure INT,
        BloodFlowRate INT,
        DialysateFlowRate INT,
        CurrentUFR DECIMAL(6,2),
        TMPPressure INT,
        Interventions NVARCHAR(MAX),
        StaffInitials NVARCHAR(10),
        RecordedBy INT,
        Notes NVARCHAR(MAX),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID) ON DELETE CASCADE
    );
    PRINT 'IntraDialyticMonitoring table created successfully';
END
ELSE
BEGIN
    PRINT 'IntraDialyticMonitoring table already exists';
END
