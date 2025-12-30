# Hemodialysis Scheduler - Application Workflow Documentation

## Table of Contents
- [User Workflows](#user-workflows)
- [Dialysis Treatment Workflow](#dialysis-treatment-workflow)
- [Scheduling Process](#scheduling-process)
- [Patient Management](#patient-management)
- [Treatment Monitoring](#treatment-monitoring)
- [Reporting & Analytics](#reporting--analytics)

---

## User Workflows

### 1. Login & Authentication Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                     LOGIN PROCESS                            │
└─────────────────────────────────────────────────────────────┘

User enters credentials
        ↓
Frontend validates input
        ↓
POST /api/auth/login
        ↓
Backend verifies username & password (BCrypt)
        ↓
   ┌────────────┐
   │  Valid?    │
   └────┬───┬───┘
     NO │   │ YES
        │   │
        │   ↓
        │ Generate JWT token
        │   ↓
        │ Update LastLogin timestamp
        │   ↓
        │ Return: { token, userId, username, role }
        │   ↓
        │ Store token in localStorage
        │   ↓
        │ Redirect to role-based dashboard
        │
        ↓
   Return error: "Invalid credentials"
        ↓
   Show error message
```

**Code Implementation:**

```typescript
// Frontend: auth.service.ts
login(username: string, password: string): Observable<any> {
  return this.http.post(`${this.apiUrl}/auth/login`, { username, password })
    .pipe(
      tap(response => {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data));
      })
    );
}
```

```csharp
// Backend: AuthController.cs
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request.Username, request.Password);
    
    if (result == null)
        return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid credentials"));
    
    return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Login successful"));
}
```

### 2. Role-Based Dashboard Access

#### Admin Dashboard
```
Features:
├── User Management (CRUD)
├── System Settings
├── Complete Data Access
├── Reports & Analytics
└── Audit Logs
```

#### HOD (Head of Department) Dashboard
```
Features:
├── Staff Management
├── Patient Overview
├── Schedule Management
├── Department Reports
└── Resource Allocation
```

#### Doctor Dashboard
```
Features:
├── Patient List
├── Medical History
├── Treatment Prescriptions
├── Schedule View
└── Clinical Reports
```

#### Nurse Dashboard
```
Features:
├── Daily Schedule
├── Patient Care
├── Treatment Monitoring
├── Vital Signs Entry
└── Discharge Process
```

#### Technician Dashboard
```
Features:
├── Equipment Status
├── Dialyser Reuse Tracking
├── Machine Assignments
└── Technical Reports
```

---

## Dialysis Treatment Workflow

### Complete 3-Phase Treatment Cycle

```
┌─────────────────────────────────────────────────────────────┐
│                   DIALYSIS SESSION LIFECYCLE                 │
└─────────────────────────────────────────────────────────────┘

1. PRE-DIALYSIS ASSESSMENT (15-20 minutes)
   ├── Patient arrival
   ├── Vital signs measurement
   │   ├── Weight
   │   ├── Blood Pressure (SBP/DBP)
   │   ├── Heart Rate
   │   └── Temperature
   ├── Access site inspection
   ├── Patient complaints documentation
   └── Review prescription
        ↓
   [Save & Lock Pre-Dialysis Phase]
        ↓

2. INTRA-DIALYSIS TREATMENT (3-4 hours)
   ├── Machine setup
   ├── Priming
   ├── Patient connection
   ├── Initial parameters set
   │   ├── Blood Flow Rate (BFR)
   │   ├── Dialysate Flow Rate (DFR)
   │   ├── UF Goal
   │   └── Treatment duration
   ├── Continuous monitoring (every 30-60 min)
   │   ├── Blood pressure
   │   ├── Heart rate
   │   ├── Venous pressure
   │   ├── Arterial pressure
   │   ├── UF volume
   │   └── Patient symptoms
   └── Adjustments as needed
        ↓
   [Auto-save monitoring records]
        ↓

3. POST-DIALYSIS ASSESSMENT (10-15 minutes)
   ├── Patient disconnection
   ├── Final vital signs
   │   ├── Post-weight
   │   ├── Blood Pressure
   │   ├── Heart Rate
   │   └── Temperature
   ├── Total UF removed calculation
   ├── Treatment duration recorded
   ├── Complications documentation
   ├── Discharge instructions
   └── Next session scheduling
        ↓
   [Complete & Lock Post-Dialysis Phase]
        ↓
   [Mark as COMPLETED]
        ↓
   [Move to History]
```

### Phase Locking Mechanism

```typescript
// Frontend: Phase progression logic
export class HDLogForm {
  savePreDialysis() {
    this.hdLogService.savePreDialysis(this.preDialysisData).subscribe({
      next: (response) => {
        this.isPreDialysisLocked = true;
        this.currentPhase = 'INTRA_DIALYSIS';
      }
    });
  }
  
  completeIntraDialysis() {
    this.hdLogService.completeIntraDialysis(this.logId).subscribe({
      next: (response) => {
        this.isIntraDialysisLocked = true;
        this.currentPhase = 'POST_DIALYSIS';
      }
    });
  }
  
  completePostDialysis() {
    this.hdLogService.completePostDialysis(this.postDialysisData).subscribe({
      next: (response) => {
        this.sessionPhase = 'COMPLETED';
        this.router.navigate(['/schedule']);
      }
    });
  }
}
```

```csharp
// Backend: Phase transition service
public class HDCycleService : IHDCycleService
{
    public async Task<bool> CompletePreDialysisAsync(int logId)
    {
        const string sql = @"
            UPDATE HDLogs 
            SET SessionPhase = 'INTRA_DIALYSIS',
                PreDialysisCompletedAt = GETDATE(),
                IsPreDialysisLocked = 1
            WHERE LogID = @LogId";
        
        return await _connection.ExecuteAsync(sql, new { LogId = logId }) > 0;
    }
    
    public async Task<bool> CompleteIntraDialysisAsync(int logId)
    {
        const string sql = @"
            UPDATE HDLogs 
            SET SessionPhase = 'POST_DIALYSIS',
                PostDialysisStartedAt = GETDATE(),
                IsIntraDialysisLocked = 1
            WHERE LogID = @LogId";
        
        return await _connection.ExecuteAsync(sql, new { LogId = logId }) > 0;
    }
}
```

### Auto-Save Functionality

```typescript
// Frontend: Auto-save implementation
private setupAutoSave() {
  // Auto-save after 2 seconds of inactivity
  this.formValueChanges$
    .pipe(
      debounceTime(2000),
      distinctUntilChanged()
    )
    .subscribe(() => {
      this.autoSave();
    });
}

private autoSave() {
  this.saveStatus = 'Saving...';
  
  this.hdLogService.saveProgress(this.currentPhaseData).subscribe({
    next: () => {
      this.saveStatus = `Saved at ${new Date().toLocaleTimeString()}`;
    },
    error: () => {
      this.saveStatus = 'Save failed';
    }
  });
}
```

---

## Scheduling Process

### Bed Assignment Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                  BED ASSIGNMENT PROCESS                      │
└─────────────────────────────────────────────────────────────┘

1. VIEW AVAILABLE SLOTS
   ├── Slot 1 (06:00-10:00) - 10 beds
   ├── Slot 2 (10:00-14:00) - 10 beds
   ├── Slot 3 (14:00-18:00) - 10 beds
   └── Slot 4 (18:00-22:00) - 10 beds
        ↓
2. CHECK BED AVAILABILITY
   For selected date & slot:
   ├── Query active sessions
   ├── Calculate: Available = Total Beds - Occupied
   └── Display bed grid with status
        ↓
3. SELECT BED & PATIENT
   ├── Choose available bed (1-10)
   ├── Select patient from dropdown
   ├── Assign doctor & nurse
   └── Set treatment parameters
        ↓
4. CREATE SCHEDULE
   ├── Validate bed availability
   ├── Insert into HDSchedule table
   ├── Log audit trail
   └── Send confirmation
        ↓
5. UPDATE BED STATUS
   └── Mark bed as OCCUPIED
```

**Code Implementation:**

```csharp
// Backend: ScheduleController.cs
[HttpPost("assign-bed")]
public async Task<ActionResult> AssignBed([FromBody] BedAssignmentRequest request)
{
    // Check availability
    var isAvailable = await _scheduleRepository.IsBedAvailableAsync(
        request.SessionDate, request.SlotID, request.BedNumber);
    
    if (!isAvailable)
        return BadRequest("Bed is already occupied");
    
    // Create schedule
    var schedule = new HDSchedule
    {
        PatientID = request.PatientID,
        SessionDate = request.SessionDate,
        SlotID = request.SlotID,
        BedNumber = request.BedNumber,
        AssignedDoctor = request.DoctorID,
        AssignedNurse = request.NurseID,
        IsDischarged = false
    };
    
    var scheduleId = await _scheduleRepository.CreateAsync(schedule);
    
    // Log audit
    await _auditLogRepository.CreateAsync(new AuditLog
    {
        Username = User.Identity.Name,
        Action = "BED_ASSIGNED",
        EntityType = "HDSchedule",
        EntityID = scheduleId,
        Timestamp = DateTime.UtcNow
    });
    
    return Ok(new { scheduleId, message = "Bed assigned successfully" });
}
```

### Recurring Sessions Management

```
┌─────────────────────────────────────────────────────────────┐
│              RECURRING SESSION CREATION                      │
└─────────────────────────────────────────────────────────────┘

1. SELECT PATIENT
   └── From active patients list
        ↓
2. CHOOSE FREQUENCY
   ├── 2x per week (Monday, Thursday)
   ├── 3x per week (Monday, Wednesday, Friday)
   └── Custom schedule
        ↓
3. SET PARAMETERS
   ├── Start date
   ├── End date (or number of sessions)
   ├── Slot preference
   ├── Bed preference (optional)
   └── Treatment prescription
        ↓
4. AUTO-GENERATE SESSIONS
   └── System creates multiple HDSchedule records
        ↓
5. SMART BED ALLOCATION
   ├── Try to assign same bed if available
   └── Assign next available bed
        ↓
6. CONFIRMATION
   └── Display all created sessions
```

**Code Implementation:**

```csharp
// Backend: RecurringSessionService.cs
public async Task<List<int>> CreateRecurringSessionsAsync(RecurringSessionRequest request)
{
    var scheduleIds = new List<int>();
    var currentDate = request.StartDate;
    
    while (currentDate <= request.EndDate)
    {
        // Check if this day matches the frequency pattern
        if (ShouldCreateSession(currentDate, request.Frequency))
        {
            // Find available bed
            var bedNumber = await FindAvailableBedAsync(currentDate, request.SlotID, request.PreferredBed);
            
            if (bedNumber.HasValue)
            {
                var schedule = new HDSchedule
                {
                    PatientID = request.PatientID,
                    SessionDate = currentDate,
                    SlotID = request.SlotID,
                    BedNumber = bedNumber.Value,
                    DryWeight = request.DryWeight,
                    UFGoal = request.UFGoal,
                    PrescribedDuration = request.Duration,
                    DialyserType = request.DialyserType
                };
                
                var scheduleId = await _scheduleRepository.CreateAsync(schedule);
                scheduleIds.Add(scheduleId);
            }
        }
        
        currentDate = currentDate.AddDays(1);
    }
    
    return scheduleIds;
}

private bool ShouldCreateSession(DateTime date, string frequency)
{
    return frequency switch
    {
        "2x_week" => date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Thursday,
        "3x_week" => date.DayOfWeek == DayOfWeek.Monday || 
                     date.DayOfWeek == DayOfWeek.Wednesday || 
                     date.DayOfWeek == DayOfWeek.Friday,
        _ => true
    };
}
```

---

## Patient Management

### Patient Registration Workflow

```
┌─────────────────────────────────────────────────────────────┐
│               PATIENT REGISTRATION PROCESS                   │
└─────────────────────────────────────────────────────────────┘

1. DEMOGRAPHIC INFORMATION
   ├── Full Name (Required)
   ├── Age (Required, 1-150)
   ├── Gender (Male/Female/Other)
   ├── Address
   ├── Phone Number 1 (Required)
   ├── Phone Number 2 (Optional)
   ├── Guardian Name
   └── Generate MRN (Medical Record Number)
        ↓
2. MEDICAL INFORMATION
   ├── HD Start Date
   ├── HD Cycle (e.g., "Cycle 45")
   ├── Dry Weight (kg)
   ├── Access Type (AVF/AVG/CVC)
   ├── Access Location
   └── Preferred Treatment Slot
        ↓
3. TREATMENT PRESCRIPTION
   ├── Dialyser Type (HI/LO)
   ├── Prescribed Duration (hours)
   ├── UF Goal (liters)
   ├── Blood Flow Rate (mL/min)
   ├── Dialysate Prescription
   └── Anticoagulation Protocol
        ↓
4. VALIDATION
   ├── Check for duplicate MRN
   ├── Validate phone numbers
   ├── Validate age range
   └── Ensure required fields completed
        ↓
5. SAVE TO DATABASE
   ├── Insert into Patients table
   ├── Log audit trail
   └── Return PatientID
        ↓
6. CONFIRMATION
   └── Display patient card with MRN
```

**Code Implementation:**

```typescript
// Frontend: patient-form.component.ts
export class PatientForm {
  patientForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    age: new FormControl(null, [Validators.required, Validators.min(1), Validators.max(150)]),
    gender: new FormControl(''),
    phoneNumber1: new FormControl('', [Validators.required, Validators.pattern(/^\d{10}$/)]),
    phoneNumber2: new FormControl(''),
    guardianName: new FormControl(''),
    hdStartDate: new FormControl(''),
    dryWeight: new FormControl(null, [Validators.min(20), Validators.max(250)]),
    accessType: new FormControl('AVF'),
    dialyserType: new FormControl('HI')
  });
  
  onSubmit() {
    if (this.patientForm.invalid) {
      this.showValidationErrors();
      return;
    }
    
    this.patientService.createPatient(this.patientForm.value).subscribe({
      next: (response) => {
        this.notificationService.success(`Patient registered with MRN: ${response.data.mrn}`);
        this.router.navigate(['/patients']);
      },
      error: (error) => {
        this.notificationService.error(error.message);
      }
    });
  }
}
```

```csharp
// Backend: PatientsController.cs
[HttpPost]
[Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
public async Task<ActionResult<ApiResponse<Patient>>> CreatePatient([FromBody] PatientDto dto)
{
    // Generate unique MRN
    var mrn = await GenerateUniqueMRNAsync();
    
    var patient = new Patient
    {
        Name = dto.Name,
        Age = dto.Age,
        Gender = dto.Gender,
        PhoneNumber1 = dto.PhoneNumber1,
        PhoneNumber2 = dto.PhoneNumber2,
        GuardianName = dto.GuardianName,
        MRN = mrn,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
    
    var patientId = await _patientRepository.CreateAsync(patient);
    patient.PatientID = patientId;
    
    // Log audit
    await _auditLogRepository.CreateAsync(new AuditLog
    {
        Username = User.Identity.Name,
        Action = "PATIENT_CREATED",
        EntityType = "Patient",
        EntityID = patientId,
        NewValue = JsonSerializer.Serialize(patient),
        Timestamp = DateTime.UtcNow
    });
    
    return Ok(ApiResponse<Patient>.SuccessResponse(patient, "Patient registered successfully"));
}

private async Task<string> GenerateUniqueMRNAsync()
{
    var year = DateTime.Now.Year;
    var lastMRN = await _patientRepository.GetLastMRNAsync(year);
    var sequence = lastMRN != null ? int.Parse(lastMRN.Substring(4)) + 1 : 1;
    return $"{year}{sequence:D4}"; // Format: 20250001
}
```

### Patient Search & Filtering

```typescript
// Frontend: Advanced search with Syncfusion Grid
export class PatientList {
  public searchSettings: SearchSettingsModel = { 
    fields: ['name', 'mrn', 'phoneNumber1'], 
    operator: 'contains',
    ignoreCase: true 
  };
  
  public filterSettings: FilterSettingsModel = { 
    type: 'Excel' 
  };
  
  public toolbar: ToolbarItems[] = ['Search', 'ExcelExport', 'PdfExport'];
  
  // Quick filters
  filterActivePatients() {
    this.grid.filterByColumn('isActive', 'equal', true);
  }
  
  filterBySlot(slotId: number) {
    this.grid.filterByColumn('slotID', 'equal', slotId);
  }
  
  filterByHDCycle(cycle: string) {
    this.grid.filterByColumn('hdCycle', 'contains', cycle);
  }
}
```

---

## Treatment Monitoring

### Intra-Dialytic Monitoring Process

```
┌─────────────────────────────────────────────────────────────┐
│          HOURLY MONITORING DURING TREATMENT                  │
└─────────────────────────────────────────────────────────────┘

MONITORING INTERVAL: Every 30-60 minutes

1. VITAL SIGNS CHECK
   ├── Blood Pressure (SBP/DBP)
   ├── Heart Rate
   ├── Temperature
   └── SpO2 (if available)
        ↓
2. MACHINE PARAMETERS
   ├── Blood Flow Rate (BFR)
   ├── Dialysate Flow Rate (DFR)
   ├── Venous Pressure
   ├── Arterial Pressure
   ├── Transmembrane Pressure (TMP)
   └── UF Rate & Volume
        ↓
3. PATIENT ASSESSMENT
   ├── Patient complaints
   ├── Comfort level
   ├── Cramping
   ├── Dizziness/Nausea
   └── Access site check
        ↓
4. RECORD ENTRY
   ├── Timestamp (auto-generated)
   ├── Enter all parameters
   ├── Add notes if needed
   └── Recorded by (nurse name)
        ↓
5. AUTO-SAVE
   └── Data saved to IntraDialyticRecords
        ↓
6. ALERT GENERATION
   └── If parameters out of range
       ├── Notify assigned nurse
       └── Flag for doctor review
```

**Code Implementation:**

```typescript
// Frontend: monitoring-form.component.ts
export class MonitoringForm {
  monitoringForm = new FormGroup({
    sbp: new FormControl(null, [Validators.required, Validators.min(60), Validators.max(250)]),
    dbp: new FormControl(null, [Validators.required, Validators.min(40), Validators.max(150)]),
    heartRate: new FormControl(null, [Validators.min(40), Validators.max(200)]),
    bloodFlowRate: new FormControl(null, [Validators.min(200), Validators.max(450)]),
    dialysateFlowRate: new FormControl(500),
    venousPressure: new FormControl(null),
    ufVolume: new FormControl(null),
    notes: new FormControl('')
  });
  
  onSubmit() {
    const record = {
      logId: this.hdLogId,
      timestamp: new Date().toISOString(),
      ...this.monitoringForm.value,
      recordedBy: this.authService.getUsername()
    };
    
    this.monitoringService.saveMonitoringRecord(record).subscribe({
      next: () => {
        this.notificationService.success('Monitoring record saved');
        this.checkForAlerts(record);
        this.resetForm();
      }
    });
  }
  
  checkForAlerts(record: any) {
    // Check for hypotension
    if (record.sbp < 90) {
      this.alertService.create({
        type: 'WARNING',
        message: 'Low blood pressure detected',
        severity: 'HIGH'
      });
    }
    
    // Check for high venous pressure
    if (record.venousPressure > 200) {
      this.alertService.create({
        type: 'ALERT',
        message: 'High venous pressure - check access',
        severity: 'CRITICAL'
      });
    }
  }
}
```

```csharp
// Backend: IntraDialyticRecord model & repository
public class IntraDialyticRecord
{
    public int RecordID { get; set; }
    public int LogID { get; set; }
    public DateTime TimeStamp { get; set; }
    public int? SBP { get; set; }
    public int? DBP { get; set; }
    public int? HeartRate { get; set; }
    public int? BloodFlowRate { get; set; }
    public int? DialysateFlowRate { get; set; }
    public int? VenousPressure { get; set; }
    public int? ArterialPressure { get; set; }
    public decimal? UFVolume { get; set; }
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; }
}

public async Task<int> CreateMonitoringRecordAsync(IntraDialyticRecord record)
{
    const string sql = @"
        INSERT INTO IntraDialyticRecords 
        (LogID, TimeStamp, SBP, DBP, HeartRate, BloodFlowRate, 
         DialysateFlowRate, VenousPressure, UFVolume, Notes, RecordedBy)
        VALUES 
        (@LogID, @TimeStamp, @SBP, @DBP, @HeartRate, @BloodFlowRate, 
         @DialysateFlowRate, @VenousPressure, @UFVolume, @Notes, @RecordedBy);
        SELECT CAST(SCOPE_IDENTITY() as int)";
    
    return await _connection.QuerySingleAsync<int>(sql, record);
}
```

### Equipment Reuse Tracking

```
┌─────────────────────────────────────────────────────────────┐
│              DIALYSER REUSE MANAGEMENT                       │
└─────────────────────────────────────────────────────────────┘

1. SESSION START
   └── Check current reuse count
        ↓
2. REUSE VALIDATION
   ├── If count < max limit (typically 6-12)
   │   └── Allow reuse
   └── If count >= max limit
       └── Alert: "New dialyser required"
        ↓
3. POST-SESSION
   ├── Increment reuse count
   ├── Record dialyser condition
   └── Store for next use
        ↓
4. ALERTS
   ├── Reuse count approaching limit
   └── Dialyser expiry notification
```

**Code Implementation:**

```csharp
// Backend: EquipmentUsageService.cs
public class EquipmentUsageService
{
    public async Task<EquipmentValidation> ValidateDialyserReuseAsync(int scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        const int MAX_REUSE = 12;
        
        var validation = new EquipmentValidation
        {
            CanReuse = schedule.DialyserReuseCount < MAX_REUSE,
            CurrentCount = schedule.DialyserReuseCount,
            MaxCount = MAX_REUSE,
            RemainingUses = MAX_REUSE - schedule.DialyserReuseCount
        };
        
        if (validation.RemainingUses <= 2)
        {
            validation.Warning = "Dialyser approaching maximum reuse limit";
        }
        
        return validation;
    }
    
    public async Task IncrementDialyserReuseAsync(int scheduleId)
    {
        const string sql = @"
            UPDATE HDSchedule 
            SET DialyserReuseCount = DialyserReuseCount + 1,
                UpdatedAt = GETDATE()
            WHERE ScheduleID = @ScheduleID";
        
        await _connection.ExecuteAsync(sql, new { ScheduleID = scheduleId });
    }
}
```

---

## Reporting & Analytics

### Available Reports

#### 1. Daily Treatment Report
```
Content:
├── Total sessions today
├── Completed sessions
├── Ongoing sessions
├── Scheduled sessions
├── Bed occupancy rate
└── Staff assignments
```

#### 2. Patient Treatment History
```
Content:
├── Patient demographics
├── All past sessions
├── Treatment parameters trends
├── Weight gain analysis
├── Complication records
└── Medication history
```

#### 3. Monthly Statistics
```
Content:
├── Total sessions per slot
├── Average treatment duration
├── Average UF removed
├── Complication rate
├── Equipment usage stats
└── Staff performance metrics
```

#### 4. Equipment Usage Report
```
Content:
├── Dialyser reuse statistics
├── Blood tubing usage
├── HD unit utilization
└── Maintenance alerts
```

**Code Implementation:**

```csharp
// Backend: ReportsController.cs
[HttpGet("daily-summary")]
public async Task<ActionResult<DailyReportDto>> GetDailySummary([FromQuery] DateTime date)
{
    var report = new DailyReportDto
    {
        Date = date,
        TotalSessions = await _scheduleRepository.GetTotalSessionsAsync(date),
        CompletedSessions = await _scheduleRepository.GetCompletedSessionsAsync(date),
        OngoingSessions = await _scheduleRepository.GetOngoingSessionsAsync(date),
        ScheduledSessions = await _scheduleRepository.GetScheduledSessionsAsync(date)
    };
    
    // Calculate bed occupancy
    var slotStats = await _scheduleRepository.GetSlotStatisticsAsync(date);
    report.BedOccupancyRate = slotStats.Select(s => new SlotOccupancy
    {
        SlotID = s.SlotID,
        SlotName = s.SlotName,
        TotalBeds = 10,
        OccupiedBeds = s.OccupiedBeds,
        OccupancyPercentage = (s.OccupiedBeds / 10.0) * 100
    }).ToList();
    
    return Ok(report);
}

[HttpGet("patient-history/{patientId}")]
public async Task<ActionResult<PatientHistoryReport>> GetPatientHistory(int patientId)
{
    var patient = await _patientRepository.GetByIdAsync(patientId);
    var sessions = await _scheduleRepository.GetPatientSessionsAsync(patientId);
    var logs = await _hdLogRepository.GetPatientLogsAsync(patientId);
    
    var report = new PatientHistoryReport
    {
        Patient = patient,
        TotalSessions = sessions.Count,
        AverageTreatmentDuration = logs.Average(l => l.TreatmentDuration ?? 0),
        AverageUFRemoved = logs.Average(l => l.TotalUFRemoved ?? 0),
        WeightTrend = logs.Select(l => new WeightRecord
        {
            Date = l.CreatedAt,
            PreWeight = l.PreWeight,
            PostWeight = l.PostWeight,
            WeightLoss = l.PreWeight - l.PostWeight
        }).ToList(),
        Complications = logs.Where(l => !string.IsNullOrEmpty(l.Complications))
                            .Select(l => new ComplicationRecord
                            {
                                Date = l.CreatedAt,
                                Description = l.Complications
                            }).ToList()
    };
    
    return Ok(report);
}
```

### Analytics Dashboard

```typescript
// Frontend: analytics-dashboard.component.ts
export class AnalyticsDashboard implements OnInit {
  // Chart data
  sessionTrendData: ChartData;
  bedOccupancyData: ChartData;
  complicationRateData: ChartData;
  
  ngOnInit() {
    this.loadAnalytics();
  }
  
  loadAnalytics() {
    forkJoin({
      sessionTrend: this.analyticsService.getSessionTrend(30), // Last 30 days
      bedOccupancy: this.analyticsService.getBedOccupancyStats(),
      complications: this.analyticsService.getComplicationRate()
    }).subscribe(data => {
      this.renderCharts(data);
    });
  }
  
  renderCharts(data: any) {
    // Session trend line chart
    this.sessionTrendData = {
      labels: data.sessionTrend.dates,
      datasets: [{
        label: 'Daily Sessions',
        data: data.sessionTrend.counts,
        borderColor: '#4CAF50',
        fill: false
      }]
    };
    
    // Bed occupancy pie chart
    this.bedOccupancyData = {
      labels: ['Occupied', 'Available'],
      datasets: [{
        data: [data.bedOccupancy.occupied, data.bedOccupancy.available],
        backgroundColor: ['#FF6384', '#36A2EB']
      }]
    };
  }
}
```

---

## Background Services

### 1. Session History Background Service

```csharp
// Automatically moves completed sessions to history
public class SessionHistoryBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run every hour
                await MoveCompletedSessionsToHistoryAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionHistoryBackgroundService");
            }
        }
    }
    
    private async Task MoveCompletedSessionsToHistoryAsync()
    {
        const string sql = @"
            UPDATE HDSchedule
            SET IsMovedToHistory = 1
            WHERE IsDischarged = 1
            AND IsMovedToHistory = 0
            AND SessionDate < DATEADD(day, -1, GETDATE())";
        
        var rowsAffected = await _connection.ExecuteAsync(sql);
        _logger.LogInformation($"Moved {rowsAffected} sessions to history");
    }
}
```

### 2. Session Completion Service

```csharp
// Automatically marks sessions as Ready-For-Discharge
public class SessionCompletionService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForCompletedSessionsAsync();
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionCompletionService");
            }
        }
    }
    
    private async Task CheckForCompletedSessionsAsync()
    {
        const string sql = @"
            UPDATE HDLogs
            SET TreatmentStatus = 'READY_FOR_DISCHARGE'
            WHERE SessionPhase = 'POST_DIALYSIS'
            AND TreatmentStatus = 'IN_PROGRESS'
            AND PostDialysisStartedAt < DATEADD(minute, -30, GETDATE())";
        
        await _connection.ExecuteAsync(sql);
    }
}
```

---

## Error Handling & Validation

### Frontend Validation
```typescript
// Form validation with real-time feedback
export class FormValidation {
  // Age validation
  ageValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const age = control.value;
    if (age < 1 || age > 150) {
      return { invalidAge: 'Age must be between 1 and 150' };
    }
    return null;
  };
  
  // Blood pressure validation
  bpValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    const sbp = group.get('sbp')?.value;
    const dbp = group.get('dbp')?.value;
    
    if (sbp && dbp && sbp <= dbp) {
      return { invalidBP: 'Systolic BP must be greater than Diastolic BP' };
    }
    return null;
  };
}
```

### Backend Error Handling
```csharp
// Global exception handler
public class ErrorHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "An internal server error occurred",
            Data = null
        };
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Documentation Version
**Version**: 1.0  
**Last Updated**: December 24, 2025  
**Author**: TalipotTech Development Team  
**Repository**: https://github.com/TalipotTech/Hemodialysis-scheduler
