-- Update Patient table with new fields for HD Log
ALTER TABLE Patients ADD 
    MRN NVARCHAR(50) NULL,
    AccessType NVARCHAR(20) NULL, -- AVF/AVG/CVC
    PrescribedDuration DECIMAL(3,1) NULL, -- in hours
    HDUnitNumber NVARCHAR(50) NULL, -- Hemo Unit 1/2/3 #
    UFGoal DECIMAL(5,2) NULL, -- Weight to Lose in liters
    DialysatePrescription NVARCHAR(50) NULL, -- Normal / K+ Free / Ca++ / Dextrose
    PrescribedBFR INT NULL, -- Prescribed Blood Flow Rate in mL/min
    AnticoagulationType NVARCHAR(50) NULL, -- Heparin / Without Heparin
    SyringeType NVARCHAR(20) NULL, -- 10/20 ml
    BolusDose DECIMAL(5,2) NULL, -- in ml
    HeparinInfusionRate DECIMAL(5,2) NULL; -- in ml/hr
GO

-- Create HDLog table for treatment sessions
CREATE TABLE HDLog (
    HDLogID INT PRIMARY KEY IDENTITY(1,1),
    PatientID INT NOT NULL,
    SessionDate DATE NOT NULL,
    SlotID INT NOT NULL,
    BedNumber INT NOT NULL,
    
    -- Pre-Dialysis Assessment
    PreDialysisWeight DECIMAL(5,2) NULL,
    PreDialysisBP_Systolic INT NULL,
    PreDialysisBP_Diastolic INT NULL,
    PreDialysisTemperature DECIMAL(4,2) NULL,
    UltrafiltrationVolume INT NULL, -- in ml
    UltrafiltrationRate INT NULL, -- in ml/hr
    DialyzerType NVARCHAR(50) NULL,
    
    -- Post-Dialysis
    PostDialysisWeight DECIMAL(5,2) NULL,
    PostDialysisBP_Systolic INT NULL,
    PostDialysisBP_Diastolic INT NULL,
    PostDialysisHR INT NULL,
    AccessBleedingTime INT NULL, -- in minutes
    TotalFluidRemoved DECIMAL(5,2) NULL, -- in liters
    PostAccessStatus NVARCHAR(200) NULL,
    SymptomsComplications NVARCHAR(500) NULL,
    
    -- Session metadata
    StartTime DATETIME NULL,
    EndTime DATETIME NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (SlotID) REFERENCES Slots(SlotID),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);
GO

-- Create IntraDialyticMonitoring table for 30-minute readings
CREATE TABLE IntraDialyticMonitoring (
    MonitoringID INT PRIMARY KEY IDENTITY(1,1),
    HDLogID INT NOT NULL,
    RecordTime TIME NOT NULL,
    BP_Systolic INT NULL,
    BP_Diastolic INT NULL,
    HeartRate INT NULL,
    BloodFlowRate INT NULL, -- Qb in mL/min
    VenousPressure INT NULL, -- VP in mmHg
    ArterialPressure INT NULL, -- AP in mmHg
    UFRate INT NULL, -- in mL/hr
    TotalUF DECIMAL(5,3) NULL, -- in liters
    InitialStaff NVARCHAR(10) NULL, -- Staff initials
    Comments NVARCHAR(500) NULL,
    
    FOREIGN KEY (HDLogID) REFERENCES HDLog(HDLogID) ON DELETE CASCADE
);
GO

-- Create PostDialysisMedications table
CREATE TABLE PostDialysisMedications (
    MedicationID INT PRIMARY KEY IDENTITY(1,1),
    HDLogID INT NOT NULL,
    MedicationName NVARCHAR(100) NOT NULL,
    Dosage NVARCHAR(50) NULL,
    Route NVARCHAR(20) NULL, -- SC/IV/PO
    AdministeredBy INT NOT NULL,
    AdministeredAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (HDLogID) REFERENCES HDLog(HDLogID) ON DELETE CASCADE,
    FOREIGN KEY (AdministeredBy) REFERENCES Users(UserID)
);
GO

-- Create indexes for better performance
CREATE INDEX IX_HDLog_PatientID ON HDLog(PatientID);
CREATE INDEX IX_HDLog_SessionDate ON HDLog(SessionDate);
CREATE INDEX IX_IntraDialyticMonitoring_HDLogID ON IntraDialyticMonitoring(HDLogID);
CREATE INDEX IX_PostDialysisMedications_HDLogID ON PostDialysisMedications(HDLogID);
GO
