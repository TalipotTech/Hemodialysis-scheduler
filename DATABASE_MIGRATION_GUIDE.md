# Database Schema Migration Guide
## Splitting Patient Table into Patients + HDSchedule

### Overview
This migration separates patient demographics from hemodialysis session data to follow proper database normalization principles.

### New Schema Structure

#### 1. **Patients Table** (Demographics Only)
```sql
CREATE TABLE Patients (
    PatientID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Age INTEGER NOT NULL,
    Gender TEXT CHECK (Gender IN ('Male', 'Female', 'Other')),
    Address TEXT,
    PhoneNumber1 TEXT NOT NULL,        -- Mandatory
    PhoneNumber2 TEXT,                  -- Optional
    GuardianName TEXT,
    MRN TEXT UNIQUE,                    -- Medical Record Number
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now'))
);
```

**Purpose**: Store permanent patient demographic information that rarely changes.

#### 2. **HDSchedule Table** (Hemodialysis Sessions)
```sql
CREATE TABLE HDSchedule (
    ScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,         -- Links to Patients table
    SessionDate TEXT NOT NULL,
    
    -- HD Session Details
    DryWeight REAL,
    HDStartDate TEXT,
    HDCycle TEXT,
    WeightGain REAL,
    DialyserType TEXT,
    DialyserReuseCount INTEGER,
    BloodTubingReuse INTEGER,
    HDUnitNumber TEXT,
    
    -- Prescription
    PrescribedDuration REAL,
    UFGoal REAL,
    DialysatePrescription TEXT,
    PrescribedBFR INTEGER,
    
    -- Anticoagulation
    AnticoagulationType TEXT,
    HeparinDose REAL,
    SyringeType TEXT,
    BolusDose REAL,
    HeparinInfusionRate REAL,
    
    -- Access & Vitals
    AccessType TEXT,
    BloodPressure TEXT,
    Symptoms TEXT,
    BloodTestDone INTEGER,
    
    -- Bed & Staff Assignment
    SlotID INTEGER,
    BedNumber INTEGER,
    AssignedDoctor INTEGER,
    AssignedNurse INTEGER,
    CreatedByStaffName TEXT,
    CreatedByStaffRole TEXT,
    
    -- Status
    IsDischarged INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now')),
    
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);
```

**Purpose**: Store each hemodialysis session with full clinical details. Multiple sessions per patient.

### Workflow Changes

#### Patient Registration (First Visit)
```
1. Search for existing patient by:
   - PatientID
   - Name
   - PhoneNumber1
   - MRN

2. If NEW patient:
   - INSERT into Patients table
   - Collect: Name, Age, Gender, Address, Phone1, Phone2, Guardian, MRN
   
3. After patient is registered:
   - Navigate to HD Session creation
   - Create entry in HDSchedule table
```

#### Patient Return Visit
```
1. Search/Lookup patient by:
   - PatientID
   - Name
   - PhoneNumber1
   - MRN

2. Once patient is identified:
   - Display patient demographics (from Patients table)
   - Show previous sessions (from HDSchedule table)
   
3. Create new HD session:
   - INSERT into HDSchedule with PatientID
   - Fill session-specific data
```

### API Endpoint Changes

#### Patient Management
```
GET    /api/patients                     - List all patients
GET    /api/patients/{id}                - Get patient demographics
GET    /api/patients/search?q={term}     - Search by name/phone/MRN
POST   /api/patients                     - Create new patient (demographics only)
PUT    /api/patients/{id}                - Update patient demographics
DELETE /api/patients/{id}                - Soft delete patient
```

#### HD Schedule Management
```
GET    /api/hdschedule/patient/{patientId}        - Get all sessions for patient
GET    /api/hdschedule/{scheduleId}               - Get specific session
POST   /api/hdschedule                            - Create new HD session
PUT    /api/hdschedule/{scheduleId}               - Update HD session
DELETE /api/hdschedule/{scheduleId}               - Delete session

GET    /api/hdschedule/today                      - Today's sessions
GET    /api/hdschedule/slot/{slotId}              - Sessions by slot
GET    /api/hdschedule/active                     - Active (not discharged) sessions
```

#### Combined Views
```
GET    /api/patients/{id}/with-sessions          - Patient + all sessions
GET    /api/patients/{id}/latest-session         - Patient + most recent session
```

### Benefits of This Design

1. **Data Normalization**
   - Patient demographics stored once
   - Session data stored separately with history
   - Eliminates redundancy

2. **Flexibility**
   - Easy to track patient history across multiple sessions
   - Can update demographics without affecting session records
   - Can analyze session trends per patient

3. **Query Efficiency**
   - Indexed searches on Phone, MRN, Name
   - Fast lookup for patient registration
   - Efficient session filtering by date, slot, status

4. **Audit Trail**
   - Complete history of all HD sessions
   - Can track changes over time
   - Better reporting capabilities

### Migration Steps

1. **Backup existing database**
   ```powershell
   Copy-Item g:\ENSATE\HD_Project\Backend\HDScheduler.db g:\ENSATE\HD_Project\Backend\HDScheduler_backup.db
   ```

2. **Stop backend server**
   ```powershell
   Get-Process -Name dotnet | Stop-Process -Force
   ```

3. **Delete old database**
   ```powershell
   Remove-Item g:\ENSATE\HD_Project\Backend\HDScheduler.db
   ```

4. **Restart backend** (auto-creates new schema)
   ```powershell
   cd g:\ENSATE\HD_Project\Backend
   dotnet run --launch-profile http
   ```

5. **Verify new schema**
   - Check console logs for "Database initialized successfully"
   - Test patient creation
   - Test HD session creation

### Code Files Updated

- ✅ `Backend/Data/DatabaseInitializer.cs` - New schema with split tables
- ✅ `Backend/Models/Patient.cs` - Demographics only
- ✅ `Backend/Models/HDSchedule.cs` - Session data model (NEW)
- ⏳ `Backend/Repositories/IPatientRepository.cs` - Update interface
- ⏳ `Backend/Repositories/PatientRepository.cs` - Update queries
- ⏳ `Backend/Controllers/PatientsController.cs` - Update endpoints
- ⏳ Frontend files - Update to match new API

### Testing Checklist

- [ ] Create new patient with demographics
- [ ] Search for patient by name
- [ ] Search for patient by phone number
- [ ] Search for patient by MRN
- [ ] Create HD session for existing patient
- [ ] View patient with all sessions
- [ ] Update patient demographics
- [ ] Update HD session details
- [ ] Discharge patient from session
- [ ] View today's active sessions
- [ ] Filter sessions by slot
