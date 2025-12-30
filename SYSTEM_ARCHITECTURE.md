# Hemodialysis Scheduler - System Architecture Documentation

## Table of Contents
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [Database Design](#database-design)
- [Security Architecture](#security-architecture)
- [AI Integration](#ai-integration)

---

## Technology Stack

### Backend Technologies
- **Framework**: ASP.NET Core 8.0 Web API
- **Language**: C# 12
- **ORM**: Dapper (Micro-ORM)
- **Database**: 
  - Azure SQL Database (Production)
  - SQL Server 2019+ (Development)
  - SQLite (Local Development)
- **Authentication**: JWT (JSON Web Tokens)
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **API Documentation**: Swagger/OpenAPI 6.5.0

### Frontend Technologies
- **Framework**: Angular 20.1.0 (Standalone Components)
- **Language**: TypeScript 5.x
- **UI Components**: 
  - Syncfusion Essential JS 2 v31.2.x (Community License)
  - Angular Material 20.2.12
  - Angular CDK 20.2.12
- **State Management**: RxJS 7.8.0
- **HTTP Client**: Angular HttpClient
- **Authentication**: @auth0/angular-jwt 5.2.0

### Development Tools
- **IDE**: Visual Studio 2022 / VS Code
- **Package Managers**: 
  - NuGet (Backend)
  - npm (Frontend)
- **Build Tools**: 
  - dotnet CLI
  - Angular CLI
- **Version Control**: Git

### Cloud & DevOps
- **Cloud Provider**: Microsoft Azure
- **Hosting**: 
  - Azure App Service (Backend API)
  - Azure Static Web Apps (Frontend)
- **Database**: Azure SQL Database
- **CI/CD**: Azure DevOps / GitHub Actions

### Key NuGet Packages
```xml
<PackageReference Include="Dapper" Version="2.1.35" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.1.2" />
```

### Key npm Packages
```json
{
  "@angular/animations": "^21.0.0",
  "@angular/cdk": "^20.2.12",
  "@angular/material": "^20.2.12",
  "@syncfusion/ej2-angular-schedule": "^31.2.15",
  "@syncfusion/ej2-angular-grids": "^31.2.18",
  "@syncfusion/ej2-angular-calendars": "^31.2.18",
  "@auth0/angular-jwt": "^5.2.0"
}
```

---

## System Architecture

### Architecture Pattern
**3-Tier Architecture with Clean Architecture Principles**

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                      │
│                    (Angular Frontend)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Components  │  │   Services   │  │    Guards    │      │
│  │              │  │              │  │              │      │
│  │ - Dashboard  │  │ - Auth       │  │ - authGuard  │      │
│  │ - Schedule   │  │ - Patient    │  │ - roleGuard  │      │
│  │ - Patients   │  │ - Schedule   │  └──────────────┘      │
│  │ - HD Logs    │  │ - AI         │                         │
│  └──────────────┘  └──────────────┘                         │
└─────────────────────────────────────────────────────────────┘
                            ↕ HTTP/HTTPS (REST API)
┌─────────────────────────────────────────────────────────────┐
│                     APPLICATION LAYER                        │
│                    (ASP.NET Core API)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Controllers  │  │   Services   │  │  Middleware  │      │
│  │              │  │              │  │              │      │
│  │ - Auth       │  │ - Auth       │  │ - JWT Auth   │      │
│  │ - Patients   │  │ - HDCycle    │  │ - CORS       │      │
│  │ - Schedule   │  │ - Equipment  │  │ - Exception  │      │
│  │ - HDLog      │  │ - AI         │  └──────────────┘      │
│  │ - Reports    │  │ - Reports    │                         │
│  └──────────────┘  └──────────────┘                         │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       DATA LAYER                             │
│                  (Repositories + Dapper)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Repositories │  │    Models    │  │   DTOs       │      │
│  │              │  │              │  │              │      │
│  │ - IUserRepo  │  │ - User       │  │ - LoginDTO   │      │
│  │ - IPatient   │  │ - Patient    │  │ - PatientDTO │      │
│  │ - ISchedule  │  │ - HDSchedule │  │ - ScheduleDTO│      │
│  │ - IHDLog     │  │ - HDLog      │  └──────────────┘      │
│  └──────────────┘  └──────────────┘                         │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    PERSISTENCE LAYER                         │
│                 (Azure SQL Database / SQL Server)            │
│                                                               │
│  Tables: Users, Patients, HDSchedule, HDLogs,                │
│          Staff, Slots, BedAssignments, AuditLogs             │
│          IntraDialyticRecords, PostDialysisMedications       │
└─────────────────────────────────────────────────────────────┘
```

### Component Architecture

#### Backend Structure
```
Backend/
├── Controllers/           # API Endpoints
│   ├── AuthController.cs
│   ├── PatientsController.cs
│   ├── HDScheduleController.cs
│   ├── HDLogController.cs
│   ├── AIController.cs
│   └── ReportsController.cs
├── Services/             # Business Logic
│   ├── AuthService.cs
│   ├── HDCycleService.cs
│   ├── EquipmentUsageService.cs
│   ├── RecurringSessionService.cs
│   ├── SessionCompletionService.cs
│   └── AI/
│       ├── AIService.cs
│       ├── GeminiClient.cs
│       └── RiskAssessmentService.cs
├── Repositories/         # Data Access
│   ├── UserRepository.cs
│   ├── PatientRepository.cs
│   ├── HDScheduleRepository.cs
│   └── HDLogRepository.cs
├── Models/              # Domain Models
│   ├── User.cs
│   ├── Patient.cs
│   ├── HDSchedule.cs
│   └── HDLog.cs
├── DTOs/               # Data Transfer Objects
│   ├── LoginRequest.cs
│   ├── PatientDto.cs
│   └── ScheduleDto.cs
└── Data/              # Database Context
    └── DapperContext.cs
```

#### Frontend Structure
```
Frontend/hd-scheduler-app/src/app/
├── core/                    # Core functionality
│   ├── guards/
│   │   └── auth.guard.ts
│   ├── interceptors/
│   │   └── auth.interceptor.ts
│   └── services/
│       └── auth.service.ts
├── features/               # Feature modules
│   ├── auth/
│   │   └── login/
│   ├── dashboard/
│   │   ├── admin-dashboard/
│   │   ├── hod-dashboard/
│   │   ├── doctor-dashboard/
│   │   └── nurse-dashboard/
│   ├── patients/
│   │   ├── patient-list/
│   │   └── patient-form/
│   ├── schedule/
│   │   ├── schedule-view/
│   │   └── bed-assignment/
│   └── hd-logs/
│       ├── hd-log-form/
│       └── phase-workflow/
├── services/              # Shared services
│   ├── ai.service.ts
│   ├── reports.service.ts
│   └── system-settings.service.ts
├── shared/               # Shared components
│   ├── components/
│   └── models/
└── app.routes.ts         # Routing configuration
```

---

## Database Design

### Entity Relationship Diagram

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ UserID (PK)     │
│ Username        │
│ PasswordHash    │
│ Role            │
│ IsActive        │
└─────────────────┘
        │
        │ CreatedBy
        ↓
┌─────────────────┐         ┌─────────────────┐
│    Patients     │         │      Staff      │
├─────────────────┤         ├─────────────────┤
│ PatientID (PK)  │         │ StaffID (PK)    │
│ Name            │         │ Name            │
│ Age             │         │ Role            │
│ Gender          │         │ AssignedSlot    │
│ PhoneNumber1    │         │ IsActive        │
│ MRN             │         └─────────────────┘
│ IsActive        │                 │
└─────────────────┘                 │ Assigned
        │                            │
        │ 1:N                        ↓
        ↓                    ┌─────────────────┐
┌─────────────────┐         │     Slots       │
│   HDSchedule    │◄────────┤─────────────────┤
├─────────────────┤         │ SlotID (PK)     │
│ ScheduleID (PK) │         │ SlotName        │
│ PatientID (FK)  │         │ StartTime       │
│ SessionDate     │         │ EndTime         │
│ DryWeight       │         │ BedCapacity     │
│ HDCycle         │         └─────────────────┘
│ DialyserType    │
│ SlotID (FK)     │
│ BedNumber       │
│ IsDischarged    │
│ AssignedDoctor  │
│ AssignedNurse   │
└─────────────────┘
        │
        │ 1:N
        ↓
┌─────────────────┐
│     HDLogs      │
├─────────────────┤
│ LogID (PK)      │
│ ScheduleID (FK) │
│ SessionPhase    │
│ PreWeight       │
│ PreSBP/PreDBP   │
│ IntraStartTime  │
│ PostWeight      │
│ TreatmentStatus │
└─────────────────┘
        │
        │ 1:N
        ↓
┌──────────────────────┐
│ IntraDialyticRecords │
├──────────────────────┤
│ RecordID (PK)        │
│ LogID (FK)           │
│ TimeStamp            │
│ SBP/DBP              │
│ HeartRate            │
│ BloodFlowRate        │
│ VenousPressure       │
└──────────────────────┘
```

### Core Database Tables

#### 1. Users Table
```sql
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'HOD', 'Doctor', 'Nurse', 'Technician')),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);
```

**Purpose**: Authentication and authorization
**Roles**: Admin, HOD, Doctor, Nurse, Technician

#### 2. Patients Table
```sql
CREATE TABLE Patients (
    PatientID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Age INT NOT NULL CHECK (Age > 0 AND Age < 150),
    Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    Address NVARCHAR(255),
    PhoneNumber1 NVARCHAR(20) NOT NULL,
    PhoneNumber2 NVARCHAR(20),
    GuardianName NVARCHAR(100),
    MRN NVARCHAR(50) UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
```

**Purpose**: Patient demographics and contact information
**Key Features**: 
- Unique Medical Record Number (MRN)
- Soft delete with IsActive flag
- Audit timestamps

#### 3. HDSchedule Table
```sql
CREATE TABLE HDSchedule (
    ScheduleID INT PRIMARY KEY IDENTITY(1,1),
    PatientID INT NOT NULL,
    SessionDate DATE NOT NULL,
    
    -- Treatment Prescription
    DryWeight DECIMAL(5,2),
    HDCycle NVARCHAR(50),
    PrescribedDuration DECIMAL(4,2),
    UFGoal DECIMAL(5,2),
    
    -- Equipment
    DialyserType NVARCHAR(2) CHECK (DialyserType IN ('HI', 'LO')),
    DialyserReuseCount INT DEFAULT 0,
    HDUnitNumber NVARCHAR(20),
    
    -- Anticoagulation
    AnticoagulationType NVARCHAR(50),
    HeparinDose DECIMAL(5,2),
    
    -- Access
    AccessType NVARCHAR(20),
    AccessLocation NVARCHAR(50),
    
    -- Bed Assignment
    SlotID INT,
    BedNumber INT CHECK (BedNumber BETWEEN 1 AND 10),
    
    -- Staff Assignment
    AssignedDoctor INT,
    AssignedNurse INT,
    
    -- Status
    IsDischarged BIT DEFAULT 0,
    IsMovedToHistory BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (SlotID) REFERENCES Slots(SlotID),
    FOREIGN KEY (AssignedDoctor) REFERENCES Staff(StaffID),
    FOREIGN KEY (AssignedNurse) REFERENCES Staff(StaffID)
);
```

**Purpose**: Dialysis session scheduling and prescription
**Key Features**:
- Complete treatment prescription
- Equipment tracking with reuse counts
- Bed and staff assignment
- Session status tracking

#### 4. HDLogs Table (3-Phase Workflow)
```sql
CREATE TABLE HDLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    ScheduleID INT NOT NULL,
    
    -- Phase Tracking
    SessionPhase NVARCHAR(20) CHECK (SessionPhase IN 
        ('PRE_DIALYSIS', 'INTRA_DIALYSIS', 'POST_DIALYSIS', 'COMPLETED')),
    PreDialysisCompletedAt DATETIME,
    IntraDialysisStartedAt DATETIME,
    PostDialysisStartedAt DATETIME,
    IsPreDialysisLocked BIT DEFAULT 0,
    IsIntraDialysisLocked BIT DEFAULT 0,
    
    -- Pre-Dialysis Assessment
    PreWeight DECIMAL(5,2),
    PreSBP INT,
    PreDBP INT,
    PreHR INT,
    PreTemperature DECIMAL(4,2),
    PreComplaints NVARCHAR(MAX),
    
    -- Intra-Dialysis (Initiation)
    IntraStartTime TIME,
    ActualUFGoal DECIMAL(5,2),
    BloodFlowRate INT,
    DialysateFlowRate INT,
    
    -- Post-Dialysis Assessment
    PostWeight DECIMAL(5,2),
    PostSBP INT,
    PostDBP INT,
    PostHR INT,
    TotalUFRemoved DECIMAL(5,2),
    TreatmentDuration DECIMAL(4,2),
    Complications NVARCHAR(MAX),
    DischargeNotes NVARCHAR(MAX),
    
    -- Status
    TreatmentStatus NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
);
```

**Purpose**: Complete treatment documentation with 3-phase workflow
**Phases**:
1. Pre-Dialysis Assessment
2. Intra-Dialysis Monitoring
3. Post-Dialysis Assessment

#### 5. IntraDialyticRecords Table
```sql
CREATE TABLE IntraDialyticRecords (
    RecordID INT PRIMARY KEY IDENTITY(1,1),
    LogID INT NOT NULL,
    TimeStamp DATETIME NOT NULL,
    SBP INT,
    DBP INT,
    HeartRate INT,
    BloodFlowRate INT,
    DialysateFlowRate INT,
    VenousPressure INT,
    ArterialPressure INT,
    TransmembranePressure INT,
    UFRate DECIMAL(5,2),
    UFVolume DECIMAL(5,2),
    Temperature DECIMAL(4,2),
    Notes NVARCHAR(MAX),
    RecordedBy NVARCHAR(100),
    
    FOREIGN KEY (LogID) REFERENCES HDLogs(LogID)
);
```

**Purpose**: Hourly monitoring during dialysis treatment
**Frequency**: Every 30-60 minutes during treatment

#### 6. Additional Tables

**Slots Table** - Treatment time slots
```sql
CREATE TABLE Slots (
    SlotID INT PRIMARY KEY,
    SlotName NVARCHAR(50) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    BedCapacity INT DEFAULT 10
);
```

**Staff Table** - Medical staff management
```sql
CREATE TABLE Staff (
    StaffID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    AssignedSlot INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
```

**AuditLogs Table** - System activity tracking
```sql
CREATE TABLE AuditLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50),
    EntityID INT,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    Timestamp DATETIME DEFAULT GETDATE()
);
```

### Database Indexes
```sql
-- Performance optimization indexes
CREATE INDEX IX_Patients_MRN ON Patients(MRN);
CREATE INDEX IX_HDSchedule_SessionDate ON HDSchedule(SessionDate);
CREATE INDEX IX_HDSchedule_PatientID ON HDSchedule(PatientID);
CREATE INDEX IX_HDSchedule_IsDischarged ON HDSchedule(IsDischarged);
CREATE INDEX IX_HDLogs_ScheduleID ON HDLogs(ScheduleID);
CREATE INDEX IX_IntraDialyticRecords_LogID ON IntraDialyticRecords(LogID);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
```

---

## Security Architecture

### Authentication Flow

```
┌─────────────┐                    ┌─────────────┐
│   Client    │                    │   Server    │
│  (Angular)  │                    │  (API)      │
└──────┬──────┘                    └──────┬──────┘
       │                                   │
       │  1. POST /api/auth/login         │
       │  { username, password }           │
       ├──────────────────────────────────>│
       │                                   │
       │                          2. Verify credentials
       │                             (BCrypt)
       │                                   │
       │  3. JWT Token + User Info         │
       │<──────────────────────────────────┤
       │                                   │
       │  4. Store token in localStorage   │
       │                                   │
       │  5. GET /api/patients             │
       │  Header: Authorization: Bearer    │
       │          <JWT_TOKEN>              │
       ├──────────────────────────────────>│
       │                                   │
       │                          6. Validate JWT
       │                             Extract claims
       │                                   │
       │  7. Response with data            │
       │<──────────────────────────────────┤
       │                                   │
```

### JWT Token Structure
```csharp
// Token Claims
var claims = new[]
{
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("userId", user.UserID.ToString())
};

// Token Configuration
var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddMinutes(30),
    Issuer = "HDSchedulerAPI",
    Audience = "HDSchedulerClient",
    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
};
```

### Password Security
```csharp
// BCrypt password hashing (AuthService.cs)
private string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, 12); // Work factor: 12
}

private bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### Role-Based Access Control (RBAC)

#### Role Hierarchy
```
Admin (Full Access)
  └── HOD (Head of Department)
       ├── Doctor
       ├── Nurse
       └── Technician
```

#### Permission Matrix
| Feature | Admin | HOD | Doctor | Nurse | Technician |
|---------|-------|-----|--------|-------|------------|
| User Management | ✓ | ✗ | ✗ | ✗ | ✗ |
| Patient Registration | ✓ | ✓ | ✓ | ✓ | ✗ |
| Schedule Management | ✓ | ✓ | ✓ | ✓ | ✗ |
| HD Log Entry | ✓ | ✓ | ✓ | ✓ | ✗ |
| Treatment Monitoring | ✓ | ✓ | ✓ | ✓ | ✓ |
| Reports Generation | ✓ | ✓ | ✓ | ✗ | ✗ |
| System Settings | ✓ | ✗ | ✗ | ✗ | ✗ |

#### Authorization Implementation
```csharp
// Controller-level authorization
[Authorize(Roles = "Admin,HOD")]
public class UserManagementController : ControllerBase { }

[Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
public class PatientsController : ControllerBase { }

// Action-level authorization
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> DeletePatient(int id) { }
```

### CORS Configuration
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://lively-pond-08e4f7c00.3.azurestaticapps.net",
                "https://dev.dialyzeflow.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Security Best Practices Implemented
1. **Password Hashing**: BCrypt with work factor 12
2. **JWT Expiration**: 30-minute token lifetime
3. **HTTPS Only**: TLS/SSL encryption in production
4. **SQL Injection Prevention**: Parameterized queries with Dapper
5. **Audit Logging**: All CRUD operations logged
6. **Input Validation**: DTO validation with Data Annotations
7. **CORS Policy**: Restricted to known origins
8. **Role-Based Access**: Granular permission control

---

## AI Integration

### AI Features Overview
The system integrates Google Gemini AI for intelligent healthcare assistance:

1. **Smart Scheduling Recommendations**
2. **Risk Assessment**
3. **Automated Report Generation**
4. **Form Autocomplete**
5. **Clinical Decision Support**

### Architecture
```
┌─────────────────────────────────────────────────────────┐
│                  Frontend (Angular)                      │
│  ┌─────────────────────────────────────────────────┐    │
│  │  AI Service (ai.service.ts)                     │    │
│  │  - getSchedulingRecommendation()                │    │
│  │  - getRiskAssessment()                          │    │
│  │  - generateReport()                             │    │
│  └─────────────────────────────────────────────────┘    │
└──────────────────────────┬──────────────────────────────┘
                           │ HTTP POST
                           ↓
┌─────────────────────────────────────────────────────────┐
│              Backend API (ASP.NET Core)                  │
│  ┌─────────────────────────────────────────────────┐    │
│  │  AIController                                   │    │
│  │  - POST /api/ai/scheduling-recommendation       │    │
│  │  - POST /api/ai/risk-assessment                 │    │
│  └─────────────────────────────────────────────────┘    │
│                           ↓                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │  AIService (Business Logic)                     │    │
│  │  - Cost tracking                                │    │
│  │  - Prompt engineering                           │    │
│  │  - Response parsing                             │    │
│  └─────────────────────────────────────────────────┘    │
│                           ↓                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │  GeminiClient (External API)                    │    │
│  │  - API key management                           │    │
│  │  - Request formatting                           │    │
│  └─────────────────────────────────────────────────┘    │
└──────────────────────────┬──────────────────────────────┘
                           │ HTTPS
                           ↓
┌─────────────────────────────────────────────────────────┐
│          Google Gemini API (generativelanguage)         │
│                  gemini-1.5-pro                          │
└─────────────────────────────────────────────────────────┘
```

### AI Service Implementation
```csharp
public class AIService : IAIService
{
    private readonly IAIRepository _repository;
    private readonly IGeminiClient _geminiClient;
    private const decimal GEMINI_PRO_INPUT_COST_PER_1K_CHARS = 0.0005m;
    private const decimal GEMINI_PRO_OUTPUT_COST_PER_1K_CHARS = 0.0015m;
    
    public async Task<AIScheduleRecommendation> GetSchedulingRecommendationAsync(
        AIScheduleRecommendationRequest request, int userId)
    {
        // Check if AI is enabled and within cost limits
        if (!await IsAIEnabledAsync() || !await CheckCostLimitAsync())
            return null;
            
        // Build comprehensive prompt
        var prompt = BuildSchedulingPrompt(request);
        
        // Call Gemini API
        var response = await _geminiClient.GenerateContentAsync(prompt);
        
        // Track usage and costs
        await TrackUsageAsync(userId, prompt.Length, response.Length);
        
        // Parse and return recommendation
        return ParseSchedulingResponse(response);
    }
}
```

### Gemini Client
```csharp
public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private const string GEMINI_API_URL = 
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent";
    
    public async Task<string> GenerateContentAsync(string prompt)
    {
        var apiKey = await GetDecryptedApiKeyAsync();
        
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{GEMINI_API_URL}?key={apiKey}", 
            requestBody);
            
        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        return result.Candidates[0].Content.Parts[0].Text;
    }
}
```

### Cost Tracking
```sql
CREATE TABLE AIUsageLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Feature NVARCHAR(50) NOT NULL,
    PromptTokens INT,
    CompletionTokens INT,
    EstimatedCost DECIMAL(10,6),
    Timestamp DATETIME DEFAULT GETDATE()
);

CREATE TABLE AISettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    AIEnabled BIT DEFAULT 0,
    EncryptedApiKey NVARCHAR(500),
    DailyCostLimit DECIMAL(10,2) DEFAULT 5.00,
    CurrentDailyCost DECIMAL(10,2) DEFAULT 0,
    LastResetDate DATE
);
```

---

## Performance Optimization

### Backend Optimizations
1. **Dapper ORM**: Lightweight micro-ORM for fast queries
2. **Database Indexing**: Strategic indexes on foreign keys and date columns
3. **Async/Await**: Non-blocking I/O operations
4. **Connection Pooling**: Efficient database connection management
5. **Response Caching**: Static data cached in memory

### Frontend Optimizations
1. **Lazy Loading**: Route-level code splitting
2. **Standalone Components**: Reduced bundle size
3. **OnPush Change Detection**: Optimized Angular change detection
4. **RxJS Operators**: Efficient data stream management
5. **Syncfusion Virtual Scrolling**: Large dataset rendering

### Database Query Optimization
```csharp
// Efficient bulk operations with Dapper
public async Task<List<HDSchedule>> GetTodaySchedulesAsync()
{
    const string sql = @"
        SELECT s.*, p.Name as PatientName
        FROM HDSchedule s
        INNER JOIN Patients p ON s.PatientID = p.PatientID
        WHERE CAST(s.SessionDate AS DATE) = CAST(GETDATE() AS DATE)
        AND s.IsDischarged = 0
        ORDER BY s.SlotID, s.BedNumber";
        
    return (await _connection.QueryAsync<HDSchedule>(sql)).ToList();
}
```

---

## Monitoring & Logging

### Application Logging
```csharp
// ILogger integration
private readonly ILogger<PatientsController> _logger;

_logger.LogInformation("Patient {PatientID} created by {Username}", 
    patient.PatientID, username);
_logger.LogError(ex, "Error retrieving patient {PatientID}", id);
```

### Audit Trail
All CRUD operations are logged:
```csharp
await _auditLogRepository.CreateAsync(new AuditLog
{
    Username = username,
    Action = "PATIENT_UPDATED",
    EntityType = "Patient",
    EntityID = patientId,
    OldValue = JsonSerializer.Serialize(oldPatient),
    NewValue = JsonSerializer.Serialize(newPatient),
    Timestamp = DateTime.UtcNow
});
```

---

## Deployment Architecture

### Production Environment (Azure)
```
Internet
    ↓
Azure Front Door (CDN + WAF)
    ↓
┌─────────────────────────────────────────┐
│  Azure Static Web Apps                  │
│  (Angular Frontend)                     │
│  - Auto-scaling                         │
│  - Global CDN                           │
│  - Custom domain: dialyzeflow.com       │
└─────────────────────────────────────────┘
    ↓ HTTPS API calls
┌─────────────────────────────────────────┐
│  Azure App Service                      │
│  (ASP.NET Core API)                     │
│  - Auto-scaling                         │
│  - Always On                            │
│  - Application Insights monitoring      │
└─────────────────────────────────────────┘
    ↓ Secure connection
┌─────────────────────────────────────────┐
│  Azure SQL Database                     │
│  - Geo-replication                      │
│  - Automated backups                    │
│  - Firewall rules                       │
└─────────────────────────────────────────┘
```

### Development Environment
- **Backend**: http://localhost:5000
- **Frontend**: http://localhost:4200
- **Database**: Local SQL Server / SQLite

---

## Documentation Version
**Version**: 1.0  
**Last Updated**: December 24, 2025  
**Author**: TalipotTech Development Team  
**Repository**: https://github.com/TalipotTech/Hemodialysis-scheduler
