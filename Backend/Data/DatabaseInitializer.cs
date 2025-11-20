using System.Data.SQLite;
using Dapper;

namespace HDScheduler.API.Data;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        
        // Check if database is already initialized
        var tableCount = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Users'");
        
        if (tableCount > 0)
        {
            Console.WriteLine("Database already initialized.");
            
            // Run migrations for existing database
            RunMigrations(connection);
            return;
        }
        
        Console.WriteLine("Initializing SQLite database...");
        
        // Create tables
        CreateTables(connection);
        
        // Seed data
        SeedData(connection);
        
        Console.WriteLine("✓ Database initialized successfully!");
    }
    
    private static void RunMigrations(SQLiteConnection connection)
    {
        // Check if ScheduleID column exists in HDLogs
        var columnExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*) 
            FROM pragma_table_info('HDLogs') 
            WHERE name='ScheduleID'
        ");
        
        if (columnExists == 0)
        {
            Console.WriteLine("Running migration: Adding ScheduleID to HDLogs, IntraDialyticRecords, and PostDialysisMedications...");
            
            try
            {
                // Add ScheduleID to HDLogs
                connection.Execute("ALTER TABLE HDLogs ADD COLUMN ScheduleID INTEGER");
                connection.Execute("CREATE INDEX IF NOT EXISTS idx_hdlogs_scheduleid ON HDLogs(ScheduleID)");
                
                // Update existing records
                connection.Execute(@"
                    UPDATE HDLogs
                    SET ScheduleID = (
                        SELECT h.ScheduleID 
                        FROM HDSchedule h 
                        WHERE h.PatientID = HDLogs.PatientID 
                        AND DATE(h.SessionDate) = DATE(HDLogs.SessionDate)
                        LIMIT 1
                    )
                    WHERE ScheduleID IS NULL
                ");
                
                // Add ScheduleID to IntraDialyticRecords
                connection.Execute("ALTER TABLE IntraDialyticRecords ADD COLUMN ScheduleID INTEGER");
                connection.Execute("CREATE INDEX IF NOT EXISTS idx_intradialytic_scheduleid ON IntraDialyticRecords(ScheduleID)");
                
                connection.Execute(@"
                    UPDATE IntraDialyticRecords
                    SET ScheduleID = (
                        SELECT h.ScheduleID 
                        FROM HDSchedule h 
                        WHERE h.PatientID = IntraDialyticRecords.PatientID 
                        AND DATE(h.SessionDate) = DATE(IntraDialyticRecords.SessionDate)
                        LIMIT 1
                    )
                    WHERE ScheduleID IS NULL
                ");
                
                // Add ScheduleID to PostDialysisMedications
                connection.Execute("ALTER TABLE PostDialysisMedications ADD COLUMN ScheduleID INTEGER");
                connection.Execute("CREATE INDEX IF NOT EXISTS idx_medications_scheduleid ON PostDialysisMedications(ScheduleID)");
                
                connection.Execute(@"
                    UPDATE PostDialysisMedications
                    SET ScheduleID = (
                        SELECT h.ScheduleID 
                        FROM HDSchedule h 
                        WHERE h.PatientID = PostDialysisMedications.PatientID 
                        AND DATE(h.SessionDate) = DATE(PostDialysisMedications.SessionDate)
                        LIMIT 1
                    )
                    WHERE ScheduleID IS NULL
                ");
                
                Console.WriteLine("✓ Migration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed: {ex.Message}");
            }
        }
        
        // Check if DialyserModel column exists in HDSchedule (indicates new monitoring fields migration)
        var dialyserModelExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*) 
            FROM pragma_table_info('HDSchedule') 
            WHERE name='DialyserModel'
        ");
        
        if (dialyserModelExists == 0)
        {
            Console.WriteLine("Running migration: Adding monitoring fields to HDSchedule...");
            
            try
            {
                // Add all new monitoring columns
                var alterStatements = new[]
                {
                    "ALTER TABLE HDSchedule ADD COLUMN DialyserModel TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN AccessLocation TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN StartTime TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN PreWeight REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN PreBPSitting TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN PreTemperature REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN AccessBleedingTime TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN AccessStatus TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN Complications TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN MonitoringTime TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN HeartRate INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN ActualBFR INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN VenousPressure INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN ArterialPressure INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN CurrentUFR REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN TotalUFAchieved REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN TmpPressure INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN Interventions TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN StaffInitials TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN MedicationType TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN MedicationName TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN Dose TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN Route TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN AdministeredAt TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN AlertType TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN AlertMessage TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN Severity TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN Resolution TEXT"
                };
                
                foreach (var sql in alterStatements)
                {
                    try
                    {
                        connection.Execute(sql);
                    }
                    catch (Exception ex)
                    {
                        // Ignore "duplicate column" errors
                        if (!ex.Message.Contains("duplicate column"))
                        {
                            throw;
                        }
                    }
                }
                
                Console.WriteLine("✓ Monitoring fields migration completed successfully!");
                Console.WriteLine("  Added 28 new columns to HDSchedule table for comprehensive HD session tracking");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Monitoring fields migration failed: {ex.Message}");
            }
        }
        
        // Check if EquipmentUsageAlerts table exists
        var equipmentAlertsTableExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*) 
            FROM sqlite_master 
            WHERE type='table' AND name='EquipmentUsageAlerts'
        ");
        
        if (equipmentAlertsTableExists == 0)
        {
            Console.WriteLine("Running migration: Creating EquipmentUsageAlerts table...");
            
            try
            {
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS EquipmentUsageAlerts (
                        AlertID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PatientID INTEGER NOT NULL,
                        ScheduleID INTEGER,
                        EquipmentType TEXT NOT NULL,
                        CurrentUsageCount INTEGER NOT NULL,
                        MaxUsageLimit INTEGER NOT NULL,
                        Severity TEXT NOT NULL,
                        AlertMessage TEXT NOT NULL,
                        IsAcknowledged INTEGER NOT NULL DEFAULT 0,
                        AcknowledgedBy TEXT,
                        AcknowledgedAt TEXT,
                        CreatedAt TEXT DEFAULT (datetime('now')),
                        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                        FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_equipment_alerts_patient ON EquipmentUsageAlerts(PatientID);
                    CREATE INDEX IF NOT EXISTS idx_equipment_alerts_schedule ON EquipmentUsageAlerts(ScheduleID);
                    CREATE INDEX IF NOT EXISTS idx_equipment_alerts_unacknowledged ON EquipmentUsageAlerts(IsAcknowledged, PatientID);
                    CREATE INDEX IF NOT EXISTS idx_equipment_alerts_severity ON EquipmentUsageAlerts(Severity, IsAcknowledged);
                ");
                
                Console.WriteLine("✓ EquipmentUsageAlerts table created successfully!");
                Console.WriteLine("  - Dialyser max usage: 7 times");
                Console.WriteLine("  - Blood Tubing max usage: 12 times");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EquipmentUsageAlerts migration failed: {ex.Message}");
            }
        }
        
        // Check if PostWeight column exists in HDSchedule (Post-Dialysis Vital Signs)
        var postWeightExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*) 
            FROM pragma_table_info('HDSchedule') 
            WHERE name='PostWeight'
        ");
        
        if (postWeightExists == 0)
        {
            Console.WriteLine("Running migration: Adding Post-Dialysis Vital Signs columns to HDSchedule...");
            
            try
            {
                var alterStatements = new[]
                {
                    "ALTER TABLE HDSchedule ADD COLUMN PostWeight REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN PostSBP INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN PostDBP INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN PostHR INTEGER",
                    "ALTER TABLE HDSchedule ADD COLUMN PostAccessStatus TEXT",
                    "ALTER TABLE HDSchedule ADD COLUMN TotalFluidRemoved REAL",
                    "ALTER TABLE HDSchedule ADD COLUMN Notes TEXT"
                };
                
                foreach (var sql in alterStatements)
                {
                    try
                    {
                        connection.Execute(sql);
                    }
                    catch (Exception ex)
                    {
                        // Ignore "duplicate column" errors
                        if (!ex.Message.Contains("duplicate column"))
                        {
                            throw;
                        }
                    }
                }
                
                Console.WriteLine("✓ Post-Dialysis Vital Signs columns added successfully!");
                Console.WriteLine("  Added 7 new columns to HDSchedule: PostWeight, PostSBP, PostDBP, PostHR, PostAccessStatus, TotalFluidRemoved, Notes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Post-Dialysis Vital Signs migration failed: {ex.Message}");
            }
        }
    }
    
    private static void CreateTables(SQLiteConnection connection)
    {
        string sql = @"
            -- Users Table
            CREATE TABLE IF NOT EXISTS Users (
                UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL CHECK (Role IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT (datetime('now')),
                LastLogin TEXT
            );

            -- Slots Table
            CREATE TABLE IF NOT EXISTS Slots (
                SlotID INTEGER PRIMARY KEY,
                SlotName TEXT NOT NULL,
                StartTime TEXT NOT NULL,
                EndTime TEXT NOT NULL,
                BedCapacity INTEGER DEFAULT 10,
                MaxBeds INTEGER DEFAULT 10,
                IsActive INTEGER DEFAULT 1
            );

            -- Staff Table
            CREATE TABLE IF NOT EXISTS Staff (
                StaffID INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Role TEXT NOT NULL CHECK (Role IN ('Doctor', 'Nurse', 'Technician', 'HOD')),
                ContactNumber TEXT,
                StaffSpecialization TEXT,
                AssignedSlot INTEGER,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (AssignedSlot) REFERENCES Slots(SlotID)
            );

            -- Patients Table (Basic Patient Information)
            CREATE TABLE IF NOT EXISTS Patients (
                PatientID INTEGER PRIMARY KEY AUTOINCREMENT,
                MRN TEXT UNIQUE,
                Name TEXT NOT NULL,
                Age INTEGER NOT NULL CHECK (Age > 0 AND Age < 150),
                Gender TEXT CHECK (Gender IN ('Male', 'Female', 'Other')),
                ContactNumber TEXT NOT NULL,
                EmergencyContact TEXT,
                Address TEXT,
                GuardianName TEXT,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT (datetime('now')),
                UpdatedAt TEXT DEFAULT (datetime('now'))
            );

            -- HDSchedule Table (Hemodialysis Session Data)
            CREATE TABLE IF NOT EXISTS HDSchedule (
                ScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                SessionDate TEXT NOT NULL,
                DryWeight REAL,
                HDStartDate TEXT,
                HDCycle TEXT,
                WeightGain REAL,
                DialyserType TEXT CHECK (DialyserType IN ('HI', 'LO')),
                DialyserReuseCount INTEGER DEFAULT 0,
                BloodTubingReuse INTEGER DEFAULT 0,
                HDUnitNumber TEXT,
                PrescribedDuration REAL,
                UFGoal REAL,
                DialysatePrescription TEXT,
                PrescribedBFR INTEGER,
                AnticoagulationType TEXT,
                HeparinDose REAL,
                SyringeType TEXT,
                BolusDose REAL,
                HeparinInfusionRate REAL,
                AccessType TEXT,
                BloodPressure TEXT,
                Symptoms TEXT,
                BloodTestDone INTEGER DEFAULT 0,
                SlotID INTEGER,
                BedNumber INTEGER CHECK (BedNumber BETWEEN 1 AND 10),
                AssignedDoctor INTEGER,
                AssignedNurse INTEGER,
                CreatedByStaffName TEXT,
                CreatedByStaffRole TEXT,
                IsDischarged INTEGER DEFAULT 0,
                CreatedAt TEXT DEFAULT (datetime('now')),
                UpdatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (SlotID) REFERENCES Slots(SlotID),
                FOREIGN KEY (AssignedDoctor) REFERENCES Staff(StaffID),
                FOREIGN KEY (AssignedNurse) REFERENCES Staff(StaffID)
            );

            -- BedAssignments Table
            CREATE TABLE IF NOT EXISTS BedAssignments (
                AssignmentID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                SlotID INTEGER NOT NULL,
                BedNumber INTEGER NOT NULL CHECK (BedNumber BETWEEN 1 AND 10),
                AssignmentDate TEXT NOT NULL,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT (datetime('now')),
                DischargedAt TEXT,
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (SlotID) REFERENCES Slots(SlotID)
            );

            -- AuditLogs Table
            CREATE TABLE IF NOT EXISTS AuditLogs (
                LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                UserID INTEGER,
                Username TEXT,
                Action TEXT NOT NULL,
                EntityType TEXT,
                EntityID INTEGER,
                OldValues TEXT,
                NewValues TEXT,
                IPAddress TEXT,
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (UserID) REFERENCES Users(UserID)
            );

            -- HDLogs Table
            CREATE TABLE IF NOT EXISTS HDLogs (
                LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                ScheduleID INTEGER,
                SessionDate TEXT NOT NULL,
                PreWeight REAL,
                PostWeight REAL,
                WeightLoss REAL,
                BloodPressurePre TEXT,
                BloodPressurePost TEXT,
                Temperature REAL,
                Notes TEXT,
                CreatedBy TEXT,
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
            );

            -- IntraDialyticRecords Table
            CREATE TABLE IF NOT EXISTS IntraDialyticRecords (
                RecordID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                ScheduleID INTEGER,
                SessionDate TEXT NOT NULL,
                TimeRecorded TEXT NOT NULL,
                BloodPressure TEXT,
                PulseRate INTEGER,
                Temperature REAL,
                UFVolume REAL,
                VenousPressure INTEGER,
                Notes TEXT,
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
            );

            -- PostDialysisMedications Table
            CREATE TABLE IF NOT EXISTS PostDialysisMedications (
                MedicationID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                ScheduleID INTEGER,
                SessionDate TEXT NOT NULL,
                MedicationName TEXT NOT NULL,
                Dosage TEXT,
                Route TEXT,
                AdministeredBy TEXT,
                AdministeredAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
            );

            -- EquipmentUsageAlerts Table (Track Dialyser and Blood Tubing usage)
            CREATE TABLE IF NOT EXISTS EquipmentUsageAlerts (
                AlertID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                ScheduleID INTEGER,
                EquipmentType TEXT NOT NULL,
                CurrentUsageCount INTEGER NOT NULL,
                MaxUsageLimit INTEGER NOT NULL,
                Severity TEXT NOT NULL,
                AlertMessage TEXT NOT NULL,
                IsAcknowledged INTEGER NOT NULL DEFAULT 0,
                AcknowledgedBy TEXT,
                AcknowledgedAt TEXT,
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
                FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
            );

            -- Indexes for performance
            CREATE INDEX IF NOT EXISTS idx_patients_mrn ON Patients(MRN);
            CREATE INDEX IF NOT EXISTS idx_patients_contact ON Patients(ContactNumber);
            CREATE INDEX IF NOT EXISTS idx_patients_name ON Patients(Name);
            CREATE INDEX IF NOT EXISTS idx_hdschedule_patientid ON HDSchedule(PatientID);
            CREATE INDEX IF NOT EXISTS idx_hdschedule_sessiondate ON HDSchedule(SessionDate);
            CREATE INDEX IF NOT EXISTS idx_hdschedule_slotid ON HDSchedule(SlotID);
            CREATE INDEX IF NOT EXISTS idx_hdschedule_discharged ON HDSchedule(IsDischarged);
            CREATE INDEX IF NOT EXISTS idx_bedassignments_slotid_date ON BedAssignments(SlotID, AssignmentDate);
            CREATE INDEX IF NOT EXISTS idx_bedassignments_patientid ON BedAssignments(PatientID);
            CREATE INDEX IF NOT EXISTS idx_bedassignments_active ON BedAssignments(IsActive);
            CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
            CREATE INDEX IF NOT EXISTS idx_staff_assignedslot ON Staff(AssignedSlot);
            CREATE INDEX IF NOT EXISTS idx_auditlogs_userid ON AuditLogs(UserID);
            CREATE INDEX IF NOT EXISTS idx_hdlogs_patientid ON HDLogs(PatientID);
            CREATE INDEX IF NOT EXISTS idx_hdlogs_scheduleid ON HDLogs(ScheduleID);
            CREATE INDEX IF NOT EXISTS idx_intradialytic_scheduleid ON IntraDialyticRecords(ScheduleID);
            CREATE INDEX IF NOT EXISTS idx_medications_scheduleid ON PostDialysisMedications(ScheduleID);
            CREATE INDEX IF NOT EXISTS idx_equipment_alerts_patient ON EquipmentUsageAlerts(PatientID);
            CREATE INDEX IF NOT EXISTS idx_equipment_alerts_schedule ON EquipmentUsageAlerts(ScheduleID);
            CREATE INDEX IF NOT EXISTS idx_equipment_alerts_unacknowledged ON EquipmentUsageAlerts(IsAcknowledged, PatientID);
            CREATE INDEX IF NOT EXISTS idx_equipment_alerts_severity ON EquipmentUsageAlerts(Severity, IsAcknowledged);
        ";
        
        connection.Execute(sql);
    }
    
    private static void SeedData(SQLiteConnection connection)
    {
        // Insert Slots
        connection.Execute(@"
            INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime) VALUES
            (1, 'Morning', '06:00:00', '10:00:00'),
            (2, 'Afternoon', '11:00:00', '15:00:00'),
            (3, 'Evening', '16:00:00', '20:00:00'),
            (4, 'Night', '21:00:00', '01:00:00')
        ");
        
        // Insert Users (password: Admin@123 for all) - Generate fresh BCrypt hashes
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        
        connection.Execute(@"
            INSERT INTO Users (Username, PasswordHash, Role) VALUES
            (@Username1, @Hash, 'Admin'),
            (@Username2, @Hash, 'HOD'),
            (@Username3, @Hash, 'Doctor'),
            (@Username4, @Hash, 'Nurse'),
            (@Username5, @Hash, 'Technician')
        ", new { 
            Username1 = "admin", 
            Username2 = "hod", 
            Username3 = "doctor1", 
            Username4 = "nurse1", 
            Username5 = "tech1", 
            Hash = passwordHash 
        });
        
        // Insert Staff
        connection.Execute(@"
            INSERT INTO Staff (Name, Role, StaffSpecialization, AssignedSlot) VALUES
            ('Dr. John Smith', 'Doctor', 'Nephrology', 1),
            ('Dr. Sarah Johnson', 'Doctor', 'Internal Medicine', 2),
            ('Nurse Mary Williams', 'Nurse', 'Dialysis Specialist', 1),
            ('Nurse Lisa Brown', 'Nurse', 'Critical Care', 2),
            ('Tech Mike Davis', 'Technician', 'Dialysis Technician', 1),
            ('HOD Dr. Robert Wilson', 'HOD', 'Head of Department', NULL)
        ");
        
        // Insert Sample Patients
        connection.Execute(@"
            INSERT INTO Patients (MRN, Name, Age, Gender, ContactNumber, Address, IsActive) VALUES
            ('MRN001', 'John Doe', 45, 'Male', '555-0101', '123 Main St', 1),
            ('MRN002', 'Jane Smith', 52, 'Female', '555-0102', '456 Oak Ave', 1),
            ('MRN003', 'Robert Johnson', 38, 'Male', '555-0103', '789 Pine Rd', 1),
            ('MRN004', 'Mary Williams', 61, 'Female', '555-0104', '321 Elm St', 1),
            ('MRN005', 'James Brown', 47, 'Male', '555-0105', '654 Maple Dr', 1),
            ('MRN006', 'Patricia Davis', 55, 'Female', '555-0106', '987 Cedar Ln', 1),
            ('MRN007', 'Michael Wilson', 42, 'Male', '555-0107', '147 Birch Ave', 1),
            ('MRN008', 'Linda Martinez', 59, 'Female', '555-0108', '258 Spruce St', 1),
            ('MRN009', 'dhuva', 55, 'Female', '555-0109', 'City Center', 1),
            ('MRN010', 'sana', 48, 'Female', '555-0110', 'Downtown', 1)
        ");
        
        // Insert Sample HD Schedules for today
        // First 8 are DISCHARGED (old patients), last 2 are ACTIVE (dhuva and sana)
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        connection.Execute($@"
            INSERT INTO HDSchedule (PatientID, SessionDate, SlotID, BedNumber, DryWeight, DialyserType, BloodPressure, IsDischarged) VALUES
            (1, '{today}', 1, 1, 65.5, 'HI', '120/80', 1),
            (2, '{today}', 1, 2, 58.2, 'LO', '130/85', 1),
            (3, '{today}', 1, 3, 72.0, 'HI', '125/82', 1),
            (4, '{today}', 2, 1, 54.8, 'LO', '118/75', 1),
            (5, '{today}', 2, 2, 68.5, 'HI', '135/90', 1),
            (6, '{today}', 2, 4, 61.3, 'LO', '128/84', 1),
            (7, '{today}', 3, 1, 70.2, 'HI', '122/78', 1),
            (8, '{today}', 3, 2, 56.7, 'LO', '132/88', 1),
            (9, '{today}', 2, 3, 58.0, 'LO', '128/82', 0),
            (10, '{today}', 2, 5, 62.0, 'HI', '125/80', 0)
        ");
        
        Console.WriteLine("✓ Sample data added: 10 patients (8 discharged, 2 active: dhuva and sana)");
    }
}
