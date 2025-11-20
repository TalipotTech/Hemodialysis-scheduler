-- Create IntraDialyticMonitoring table for vital signs recording during dialysis
-- This table stores multiple vital sign readings taken during a single HD session

CREATE TABLE IF NOT EXISTS IntraDialyticMonitoring (
    MonitoringID INTEGER PRIMARY KEY AUTOINCREMENT,
    HDLogID INTEGER NOT NULL,
    TimeRecorded DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    BloodPressure VARCHAR(20),
    PulseRate DECIMAL(5,1),
    Temperature DECIMAL(4,1),
    UFVolume DECIMAL(5,2),
    ActualBFR DECIMAL(5,1),
    VenousPressure DECIMAL(6,1),
    ArterialPressure DECIMAL(6,1),
    TMPPressure DECIMAL(6,1),
    Symptoms TEXT,
    Interventions TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (HDLogID) REFERENCES HDLog(LogID) ON DELETE CASCADE
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_intradialytic_hdlogid ON IntraDialyticMonitoring(HDLogID);
CREATE INDEX IF NOT EXISTS idx_intradialytic_time ON IntraDialyticMonitoring(TimeRecorded);
