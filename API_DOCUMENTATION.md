# Hemodialysis Scheduler - API Documentation

## Table of Contents
- [API Overview](#api-overview)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
- [Request/Response Examples](#requestresponse-examples)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)

---

## API Overview

### Base URL
- **Development**: `http://localhost:5000/api`
- **Production**: `https://hds-dev-api.azurewebsites.net/api`

### API Version
**Version**: 1.0

### Content Type
All requests and responses use `application/json`

### Authentication
JWT Bearer Token required for all endpoints except `/auth/login`

### Standard Response Format
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ }
}
```

---

## Authentication

### Login

**Endpoint**: `POST /api/auth/login`

**Description**: Authenticate user and receive JWT token

**Request Body**:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 1,
    "username": "admin",
    "role": "Admin",
    "expiresAt": "2025-12-24T15:30:00Z"
  }
}
```

**Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null
}
```

**Code Example**:
```typescript
// Angular/TypeScript
login(username: string, password: string): Observable<LoginResponse> {
  return this.http.post<ApiResponse<LoginResponse>>(
    `${this.apiUrl}/auth/login`,
    { username, password }
  ).pipe(
    map(response => response.data)
  );
}
```

```csharp
// C# Backend
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request.Username, request.Password);
    
    if (result == null)
        return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid credentials"));
    
    return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Login successful"));
}
```

### Get User Info

**Endpoint**: `GET /api/auth/user-info`

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "User information retrieved",
  "data": {
    "userId": 1,
    "username": "admin",
    "role": "Admin",
    "isActive": true,
    "lastLogin": "2025-12-24T10:00:00Z"
  }
}
```

---

## API Endpoints

### Patients API

#### Get All Patients

**Endpoint**: `GET /api/patients`

**Authorization**: Admin, HOD, Doctor, Nurse

**Query Parameters**:
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 50)
- `searchTerm` (optional): Search by name or MRN
- `isActive` (optional): Filter by active status

**Request**:
```
GET /api/patients?page=1&pageSize=20&isActive=true
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Patients retrieved successfully",
  "data": [
    {
      "patientID": 1,
      "name": "John Doe",
      "age": 45,
      "gender": "Male",
      "phoneNumber1": "1234567890",
      "phoneNumber2": "0987654321",
      "guardianName": "Jane Doe",
      "mrn": "20250001",
      "isActive": true,
      "createdAt": "2025-01-15T08:30:00Z"
    }
  ]
}
```

#### Get Patient by ID

**Endpoint**: `GET /api/patients/{id}`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Patient retrieved successfully",
  "data": {
    "patientID": 1,
    "name": "John Doe",
    "age": 45,
    "gender": "Male",
    "address": "123 Main St, City",
    "phoneNumber1": "1234567890",
    "phoneNumber2": "0987654321",
    "guardianName": "Jane Doe",
    "mrn": "20250001",
    "isActive": true,
    "createdAt": "2025-01-15T08:30:00Z",
    "updatedAt": "2025-12-20T14:30:00Z"
  }
}
```

#### Create Patient

**Endpoint**: `POST /api/patients`

**Authorization**: Admin, HOD, Doctor, Nurse

**Request Body**:
```json
{
  "name": "Jane Smith",
  "age": 52,
  "gender": "Female",
  "address": "456 Oak Ave, Town",
  "phoneNumber1": "5551234567",
  "phoneNumber2": "5559876543",
  "guardianName": "Bob Smith"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Patient created successfully",
  "data": {
    "patientID": 2,
    "name": "Jane Smith",
    "mrn": "20250002",
    "isActive": true,
    "createdAt": "2025-12-24T14:00:00Z"
  }
}
```

**Code Example**:
```typescript
// Frontend Service
createPatient(patient: PatientDto): Observable<Patient> {
  return this.http.post<ApiResponse<Patient>>(
    `${this.apiUrl}/patients`,
    patient,
    { headers: this.getAuthHeaders() }
  ).pipe(
    map(response => response.data)
  );
}

private getAuthHeaders(): HttpHeaders {
  const token = localStorage.getItem('token');
  return new HttpHeaders({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  });
}
```

```csharp
// Backend Controller
[HttpPost]
[Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
public async Task<ActionResult<ApiResponse<Patient>>> CreatePatient([FromBody] PatientDto dto)
{
    // Validate input
    if (!ModelState.IsValid)
        return BadRequest(ApiResponse<Patient>.ErrorResponse("Invalid patient data"));
    
    // Generate MRN
    var mrn = await GenerateUniqueMRNAsync();
    
    var patient = new Patient
    {
        Name = dto.Name,
        Age = dto.Age,
        Gender = dto.Gender,
        PhoneNumber1 = dto.PhoneNumber1,
        MRN = mrn,
        IsActive = true
    };
    
    var patientId = await _patientRepository.CreateAsync(patient);
    patient.PatientID = patientId;
    
    return CreatedAtAction(nameof(GetPatientById), 
        new { id = patientId }, 
        ApiResponse<Patient>.SuccessResponse(patient, "Patient created successfully"));
}
```

#### Update Patient

**Endpoint**: `PUT /api/patients/{id}`

**Authorization**: Admin, HOD, Doctor, Nurse

**Request Body**:
```json
{
  "name": "John Doe Updated",
  "age": 46,
  "phoneNumber1": "1234567890",
  "address": "789 New Street"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Patient updated successfully",
  "data": {
    "patientID": 1,
    "name": "John Doe Updated",
    "age": 46,
    "updatedAt": "2025-12-24T14:30:00Z"
  }
}
```

#### Delete Patient (Soft Delete)

**Endpoint**: `DELETE /api/patients/{id}`

**Authorization**: Admin only

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Patient deactivated successfully",
  "data": null
}
```

---

### Schedule API

#### Get Today's Schedules

**Endpoint**: `GET /api/hdschedule/today`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Today's schedules retrieved",
  "data": [
    {
      "scheduleID": 101,
      "patientID": 1,
      "patientName": "John Doe",
      "sessionDate": "2025-12-24",
      "slotID": 1,
      "slotName": "Slot 1 (06:00-10:00)",
      "bedNumber": 3,
      "dryWeight": 70.5,
      "ufGoal": 2.5,
      "dialyserType": "HI",
      "dialyserReuseCount": 3,
      "assignedDoctor": 5,
      "assignedNurse": 8,
      "isDischarged": false,
      "createdAt": "2025-12-20T10:00:00Z"
    }
  ]
}
```

#### Get Schedule by ID

**Endpoint**: `GET /api/hdschedule/{id}`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Schedule retrieved successfully",
  "data": {
    "scheduleID": 101,
    "patientID": 1,
    "sessionDate": "2025-12-24",
    "dryWeight": 70.5,
    "hdCycle": "Cycle 45",
    "ufGoal": 2.5,
    "prescribedDuration": 4.0,
    "dialyserType": "HI",
    "dialyserReuseCount": 3,
    "anticoagulationType": "Heparin",
    "heparinDose": 3000,
    "accessType": "AVF",
    "accessLocation": "Left Arm",
    "slotID": 1,
    "bedNumber": 3,
    "assignedDoctor": 5,
    "assignedNurse": 8,
    "isDischarged": false
  }
}
```

#### Create Schedule (Bed Assignment)

**Endpoint**: `POST /api/hdschedule`

**Authorization**: Admin, HOD, Doctor, Nurse

**Request Body**:
```json
{
  "patientID": 1,
  "sessionDate": "2025-12-25",
  "slotID": 2,
  "bedNumber": 5,
  "dryWeight": 70.5,
  "ufGoal": 2.5,
  "prescribedDuration": 4.0,
  "dialyserType": "HI",
  "anticoagulationType": "Heparin",
  "heparinDose": 3000,
  "accessType": "AVF",
  "accessLocation": "Left Arm",
  "assignedDoctor": 5,
  "assignedNurse": 8
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Schedule created successfully",
  "data": {
    "scheduleID": 102,
    "patientID": 1,
    "sessionDate": "2025-12-25",
    "slotID": 2,
    "bedNumber": 5
  }
}
```

#### Check Bed Availability

**Endpoint**: `GET /api/hdschedule/bed-availability`

**Query Parameters**:
- `sessionDate` (required): Date in YYYY-MM-DD format
- `slotID` (required): Slot ID (1-4)

**Request**:
```
GET /api/hdschedule/bed-availability?sessionDate=2025-12-25&slotID=2
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Bed availability retrieved",
  "data": {
    "sessionDate": "2025-12-25",
    "slotID": 2,
    "slotName": "Slot 2 (10:00-14:00)",
    "totalBeds": 10,
    "occupiedBeds": 7,
    "availableBeds": 3,
    "bedStatus": [
      { "bedNumber": 1, "isOccupied": true, "patientName": "John Doe" },
      { "bedNumber": 2, "isOccupied": true, "patientName": "Jane Smith" },
      { "bedNumber": 3, "isOccupied": false, "patientName": null },
      { "bedNumber": 4, "isOccupied": false, "patientName": null }
    ]
  }
}
```

#### Create Recurring Sessions

**Endpoint**: `POST /api/recurring-sessions`

**Request Body**:
```json
{
  "patientID": 1,
  "startDate": "2025-12-25",
  "endDate": "2026-01-25",
  "frequency": "3x_week",
  "slotID": 2,
  "preferredBed": 5,
  "dryWeight": 70.5,
  "ufGoal": 2.5,
  "prescribedDuration": 4.0,
  "dialyserType": "HI"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "12 sessions created successfully",
  "data": {
    "sessionsCreated": 12,
    "scheduleIDs": [102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113]
  }
}
```

---

### HD Logs API (Treatment Documentation)

#### Get HD Log by Schedule ID

**Endpoint**: `GET /api/hdlog/schedule/{scheduleId}`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "HD Log retrieved successfully",
  "data": {
    "logID": 501,
    "scheduleID": 101,
    "sessionPhase": "INTRA_DIALYSIS",
    "preDialysisCompletedAt": "2025-12-24T06:30:00Z",
    "intraDialysisStartedAt": "2025-12-24T06:45:00Z",
    "isPreDialysisLocked": true,
    "isIntraDialysisLocked": false,
    "preWeight": 73.0,
    "preSBP": 140,
    "preDBP": 85,
    "preHR": 78,
    "preTemperature": 36.8,
    "preComplaints": "Mild headache",
    "intraStartTime": "06:45:00",
    "actualUFGoal": 2.5,
    "bloodFlowRate": 300,
    "treatmentStatus": "IN_PROGRESS"
  }
}
```

#### Create/Update Pre-Dialysis Assessment

**Endpoint**: `POST /api/hdlog/pre-dialysis`

**Request Body**:
```json
{
  "scheduleID": 101,
  "preWeight": 73.0,
  "preSBP": 140,
  "preDBP": 85,
  "preHR": 78,
  "preTemperature": 36.8,
  "preComplaints": "Mild headache"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Pre-dialysis assessment saved",
  "data": {
    "logID": 501,
    "sessionPhase": "PRE_DIALYSIS",
    "savedAt": "2025-12-24T06:30:00Z"
  }
}
```

#### Complete Pre-Dialysis Phase

**Endpoint**: `POST /api/hdlog/{logId}/complete-pre-dialysis`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Pre-dialysis phase completed",
  "data": {
    "logID": 501,
    "sessionPhase": "INTRA_DIALYSIS",
    "preDialysisCompletedAt": "2025-12-24T06:30:00Z",
    "isPreDialysisLocked": true
  }
}
```

#### Save Intra-Dialysis Monitoring Record

**Endpoint**: `POST /api/hdlog/monitoring-record`

**Request Body**:
```json
{
  "logID": 501,
  "sbp": 135,
  "dbp": 80,
  "heartRate": 75,
  "bloodFlowRate": 300,
  "dialysateFlowRate": 500,
  "venousPressure": 150,
  "arterialPressure": -180,
  "ufVolume": 1.2,
  "temperature": 36.5,
  "notes": "Patient comfortable, no complaints",
  "recordedBy": "Nurse Mary"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Monitoring record saved",
  "data": {
    "recordID": 1001,
    "timestamp": "2025-12-24T07:30:00Z"
  }
}
```

#### Get All Monitoring Records

**Endpoint**: `GET /api/hdlog/{logId}/monitoring-records`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Monitoring records retrieved",
  "data": [
    {
      "recordID": 1001,
      "timestamp": "2025-12-24T07:00:00Z",
      "sbp": 140,
      "dbp": 85,
      "heartRate": 78,
      "bloodFlowRate": 300,
      "ufVolume": 0.8,
      "recordedBy": "Nurse Mary"
    },
    {
      "recordID": 1002,
      "timestamp": "2025-12-24T08:00:00Z",
      "sbp": 135,
      "dbp": 80,
      "heartRate": 75,
      "bloodFlowRate": 300,
      "ufVolume": 1.6,
      "recordedBy": "Nurse Mary"
    }
  ]
}
```

#### Complete Post-Dialysis Assessment

**Endpoint**: `POST /api/hdlog/post-dialysis`

**Request Body**:
```json
{
  "logID": 501,
  "postWeight": 70.5,
  "postSBP": 130,
  "postDBP": 78,
  "postHR": 72,
  "totalUFRemoved": 2.5,
  "treatmentDuration": 4.0,
  "complications": "None",
  "dischargeNotes": "Patient tolerated treatment well. Advised to monitor fluid intake."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Post-dialysis assessment completed",
  "data": {
    "logID": 501,
    "sessionPhase": "COMPLETED",
    "treatmentStatus": "COMPLETED",
    "completedAt": "2025-12-24T10:45:00Z"
  }
}
```

---

### Reports API

#### Get Daily Report

**Endpoint**: `GET /api/reports/daily`

**Query Parameters**:
- `date` (required): Date in YYYY-MM-DD format

**Request**:
```
GET /api/reports/daily?date=2025-12-24
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Daily report generated",
  "data": {
    "date": "2025-12-24",
    "totalSessions": 35,
    "completedSessions": 28,
    "ongoingSessions": 7,
    "scheduledSessions": 0,
    "bedOccupancyRate": [
      {
        "slotID": 1,
        "slotName": "Slot 1 (06:00-10:00)",
        "totalBeds": 10,
        "occupiedBeds": 9,
        "occupancyPercentage": 90.0
      },
      {
        "slotID": 2,
        "slotName": "Slot 2 (10:00-14:00)",
        "totalBeds": 10,
        "occupiedBeds": 10,
        "occupancyPercentage": 100.0
      }
    ]
  }
}
```

#### Get Patient History Report

**Endpoint**: `GET /api/reports/patient-history/{patientId}`

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Patient history report generated",
  "data": {
    "patient": {
      "patientID": 1,
      "name": "John Doe",
      "mrn": "20250001"
    },
    "totalSessions": 120,
    "averageTreatmentDuration": 3.8,
    "averageUFRemoved": 2.3,
    "weightTrend": [
      {
        "date": "2025-12-24",
        "preWeight": 73.0,
        "postWeight": 70.5,
        "weightLoss": 2.5
      }
    ],
    "complications": [
      {
        "date": "2025-12-10",
        "description": "Hypotension episode managed with saline"
      }
    ]
  }
}
```

#### Export Report

**Endpoint**: `GET /api/reports/export`

**Query Parameters**:
- `reportType` (required): daily | monthly | patient-history
- `format` (required): pdf | excel
- `startDate` (optional): Start date
- `endDate` (optional): End date

**Request**:
```
GET /api/reports/export?reportType=monthly&format=pdf&startDate=2025-12-01&endDate=2025-12-31
```

**Response**: File download (PDF or Excel)

---

### AI Features API

#### Get Scheduling Recommendation

**Endpoint**: `POST /api/ai/scheduling-recommendation`

**Request Body**:
```json
{
  "patientID": 1,
  "preferredSlots": [1, 2],
  "sessionDate": "2025-12-25",
  "patientHistory": {
    "totalSessions": 45,
    "complications": ["hypotension"],
    "averageUFGoal": 2.5
  }
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "AI recommendation generated",
  "data": {
    "recommendedSlot": 2,
    "recommendedBed": 5,
    "reasoning": "Based on patient history, Slot 2 timing is optimal. Bed 5 recommended for easy nurse access.",
    "confidence": 0.87
  }
}
```

#### Get Risk Assessment

**Endpoint**: `POST /api/ai/risk-assessment`

**Request Body**:
```json
{
  "patientID": 1,
  "currentVitals": {
    "sbp": 140,
    "dbp": 85,
    "heartRate": 78,
    "weight": 73.0
  }
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Risk assessment completed",
  "data": {
    "riskLevel": "MODERATE",
    "riskFactors": [
      {
        "factor": "Elevated blood pressure",
        "severity": "MODERATE",
        "recommendation": "Monitor BP closely during treatment"
      },
      {
        "factor": "High interdialytic weight gain",
        "severity": "HIGH",
        "recommendation": "Counsel patient on fluid restriction"
      }
    ],
    "overallScore": 6.5
  }
}
```

---

### User Management API

#### Get All Users

**Endpoint**: `GET /api/user-management`

**Authorization**: Admin only

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "userID": 1,
      "username": "admin",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2025-01-01T00:00:00Z",
      "lastLogin": "2025-12-24T10:00:00Z"
    }
  ]
}
```

#### Create User

**Endpoint**: `POST /api/user-management`

**Authorization**: Admin only

**Request Body**:
```json
{
  "username": "nurse1",
  "password": "Nurse@123",
  "role": "Nurse"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userID": 10,
    "username": "nurse1",
    "role": "Nurse",
    "isActive": true
  }
}
```

---

## Error Handling

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": [
    {
      "field": "age",
      "message": "Age must be between 1 and 150"
    }
  ]
}
```

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 OK | Request successful |
| 201 Created | Resource created successfully |
| 400 Bad Request | Invalid request data |
| 401 Unauthorized | Authentication required or failed |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 409 Conflict | Resource conflict (e.g., duplicate) |
| 500 Internal Server Error | Server error |

### Common Error Responses

#### Validation Error (400)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "field": "phoneNumber1",
      "message": "Phone number is required"
    },
    {
      "field": "age",
      "message": "Age must be between 1 and 150"
    }
  ]
}
```

#### Authentication Error (401)
```json
{
  "success": false,
  "message": "Invalid or expired token",
  "data": null
}
```

#### Authorization Error (403)
```json
{
  "success": false,
  "message": "You don't have permission to access this resource",
  "data": null
}
```

#### Not Found Error (404)
```json
{
  "success": false,
  "message": "Patient with ID 999 not found",
  "data": null
}
```

---

## Rate Limiting

### Current Limits
- **Standard endpoints**: 100 requests per minute per user
- **AI endpoints**: 10 requests per minute per user
- **Report generation**: 5 requests per minute per user

### Rate Limit Headers
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1735053600
```

### Rate Limit Exceeded Response (429)
```json
{
  "success": false,
  "message": "Rate limit exceeded. Please try again in 60 seconds.",
  "data": null
}
```

---

## Code Examples

### Complete API Integration Example

```typescript
// Angular Service - patient.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private apiUrl = 'https://api.example.com/api';
  
  constructor(private http: HttpClient) {}
  
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }
  
  getAllPatients(): Observable<Patient[]> {
    return this.http.get<ApiResponse<Patient[]>>(
      `${this.apiUrl}/patients`,
      { headers: this.getHeaders() }
    ).pipe(
      map(response => response.data)
    );
  }
  
  getPatientById(id: number): Observable<Patient> {
    return this.http.get<ApiResponse<Patient>>(
      `${this.apiUrl}/patients/${id}`,
      { headers: this.getHeaders() }
    ).pipe(
      map(response => response.data)
    );
  }
  
  createPatient(patient: PatientDto): Observable<Patient> {
    return this.http.post<ApiResponse<Patient>>(
      `${this.apiUrl}/patients`,
      patient,
      { headers: this.getHeaders() }
    ).pipe(
      map(response => response.data)
    );
  }
  
  updatePatient(id: number, patient: PatientDto): Observable<Patient> {
    return this.http.put<ApiResponse<Patient>>(
      `${this.apiUrl}/patients/${id}`,
      patient,
      { headers: this.getHeaders() }
    ).pipe(
      map(response => response.data)
    );
  }
  
  deletePatient(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(
      `${this.apiUrl}/patients/${id}`,
      { headers: this.getHeaders() }
    ).pipe(
      map(() => void 0)
    );
  }
}
```

```csharp
// C# Backend Repository - PatientRepository.cs
public class PatientRepository : IPatientRepository
{
    private readonly DapperContext _context;
    
    public PatientRepository(DapperContext context)
    {
        _context = context;
    }
    
    public async Task<List<Patient>> GetAllAsync()
    {
        const string sql = @"
            SELECT * FROM Patients 
            WHERE IsActive = 1 
            ORDER BY Name";
        
        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<Patient>(sql);
        return patients.ToList();
    }
    
    public async Task<Patient?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM Patients WHERE PatientID = @Id";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Patient>(sql, new { Id = id });
    }
    
    public async Task<int> CreateAsync(Patient patient)
    {
        const string sql = @"
            INSERT INTO Patients (Name, Age, Gender, PhoneNumber1, PhoneNumber2, 
                                  GuardianName, MRN, IsActive, CreatedAt)
            VALUES (@Name, @Age, @Gender, @PhoneNumber1, @PhoneNumber2, 
                    @GuardianName, @MRN, @IsActive, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, patient);
    }
    
    public async Task<bool> UpdateAsync(Patient patient)
    {
        const string sql = @"
            UPDATE Patients 
            SET Name = @Name, 
                Age = @Age, 
                Gender = @Gender,
                PhoneNumber1 = @PhoneNumber1,
                PhoneNumber2 = @PhoneNumber2,
                GuardianName = @GuardianName,
                UpdatedAt = GETDATE()
            WHERE PatientID = @PatientID";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, patient);
        return rowsAffected > 0;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE Patients 
            SET IsActive = 0, UpdatedAt = GETDATE() 
            WHERE PatientID = @Id";
        
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
```

---

## Postman Collection

### Import the following collection to test all endpoints:

```json
{
  "info": {
    "name": "HD Scheduler API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"username\": \"admin\",\n  \"password\": \"Admin@123\"\n}",
              "options": {
                "raw": {
                  "language": "json"
                }
              }
            },
            "url": {
              "raw": "{{baseUrl}}/api/auth/login",
              "host": ["{{baseUrl}}"],
              "path": ["api", "auth", "login"]
            }
          }
        }
      ]
    },
    {
      "name": "Patients",
      "item": [
        {
          "name": "Get All Patients",
          "request": {
            "method": "GET",
            "header": [
              {
                "key": "Authorization",
                "value": "Bearer {{token}}",
                "type": "text"
              }
            ],
            "url": {
              "raw": "{{baseUrl}}/api/patients",
              "host": ["{{baseUrl}}"],
              "path": ["api", "patients"]
            }
          }
        }
      ]
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000"
    },
    {
      "key": "token",
      "value": ""
    }
  ]
}
```

---

## API Versioning

Future versions will be accessible via:
- `/api/v2/patients`
- `/api/v3/patients`

Current version (v1) is the default and doesn't require version in URL.

---

## Documentation Version
**Version**: 1.0  
**Last Updated**: December 24, 2025  
**Author**: TalipotTech Development Team  
**Repository**: https://github.com/TalipotTech/Hemodialysis-scheduler
