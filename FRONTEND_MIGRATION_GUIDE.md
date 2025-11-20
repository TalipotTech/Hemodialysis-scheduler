# Frontend Migration Guide for Split Patient/HDSchedule Tables

## Overview
The backend has been migrated to split the Patient table into:
1. **Patients** - Demographics only
2. **HDSchedule** - HD session data

## API Endpoint Changes

### Patient Endpoints (Demographics)
```typescript
// OLD (removed)
GET  /api/patients/active  // No longer exists

// NEW
GET  /api/patients                     // Get all patients
GET  /api/patients/{id}                // Get patient demographics
GET  /api/patients/search?q={term}     // Search by name/phone/MRN
GET  /api/patients/{id}/with-sessions  // Get patient + latest session
POST /api/patients                     // Create patient (demographics only)
PUT  /api/patients/{id}                // Update patient demographics
DELETE /api/patients/{id}              // Soft delete patient
```

### HD Schedule Endpoints (Sessions)
```typescript
// NEW Controller: /api/hdschedule
GET  /api/hdschedule                        // Get all sessions
GET  /api/hdschedule/active                 // Get active (not discharged) sessions
GET  /api/hdschedule/today                  // Get today's sessions
GET  /api/hdschedule/{id}                   // Get specific session
GET  /api/hdschedule/patient/{patientId}    // Get all sessions for a patient
GET  /api/hdschedule/slot/{slotId}          // Get sessions by slot
POST /api/hdschedule                        // Create new HD session
PUT  /api/hdschedule/{id}                   // Update HD session
PUT  /api/hdschedule/{id}/discharge         // Discharge patient from session
DELETE /api/hdschedule/{id}                 // Delete session
```

## Model Changes

### Patient Model (TypeScript)
```typescript
// OLD - Combined model
export interface Patient {
  patientID: number;
  name: string;
  age: number;
  dryWeight?: number;
  hdStartDate: Date;
  hdCycle?: string;
  // ... many HD-related fields
  slotID?: number;
  bedNumber?: number;
  isDischarged: boolean;
}

// NEW - Demographics only
export interface Patient {
  patientID: number;
  name: string;
  age: number;
  gender?: string;           // NEW: Male/Female/Other
  address?: string;          // NEW
  phoneNumber1: string;      // NEW: Mandatory
  phoneNumber2?: string;     // NEW: Optional
  guardianName?: string;     // NEW
  mrn?: string;             // Medical Record Number
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}
```

### HDSchedule Model (TypeScript)
```typescript
// NEW - Session data
export interface HDSchedule {
  scheduleID: number;
  patientID: number;
  sessionDate: Date;
  
  // Basic HD Info
  dryWeight?: number;
  hdStartDate?: Date;
  hdCycle?: string;
  weightGain?: number;
  
  // Equipment
  dialyserType?: string;
  dialyserReuseCount: number;
  bloodTubingReuse: number;
  hdUnitNumber?: string;
  
  // Prescription
  prescribedDuration?: number;
  ufGoal?: number;
  dialysatePrescription?: string;
  prescribedBFR?: number;
  
  // Anticoagulation
  anticoagulationType?: string;
  heparinDose?: number;
  syringeType?: string;
  bolusDose?: number;
  heparinInfusionRate?: number;
  
  // Access & Vitals
  accessType?: string;
  bloodPressure?: string;
  symptoms?: string;
  bloodTestDone: boolean;
  
  // Assignment
  slotID?: number;
  bedNumber?: number;
  assignedDoctor?: number;
  assignedNurse?: number;
  createdByStaffName?: string;
  createdByStaffRole?: string;
  
  // Status
  isDischarged: boolean;
  createdAt: Date;
  updatedAt: Date;
  
  // Navigation properties
  patientName?: string;
  assignedDoctorName?: string;
  assignedNurseName?: string;
}
```

## Workflow Changes

### 1. Patient Registration (First Visit)
```typescript
// Step 1: Search for existing patient
searchPatient(term: string) {
  return this.http.get<ApiResponse<Patient[]>>(
    `${this.apiUrl}/patients/search?q=${term}`
  );
}

// Step 2: If not found, create patient
createPatient(patient: CreatePatientRequest) {
  // Only demographics: name, age, gender, address, phone1, phone2, guardian, MRN
  return this.http.post<ApiResponse<number>>(
    `${this.apiUrl}/patients`, 
    patient
  );
}

// Step 3: Navigate to HD Session creation
// Use the returned patientID
```

### 2. Patient Return Visit
```typescript
// Step 1: Search/lookup patient
searchPatient(term: string) {
  return this.http.get<ApiResponse<Patient[]>>(
    `${this.apiUrl}/patients/search?q=${term}`
  );
}

// Step 2: Get patient with latest session
getPatientWithSessions(patientId: number) {
  return this.http.get<ApiResponse<PatientWithLatestSession>>(
    `${this.apiUrl}/patients/${patientId}/with-sessions`
  );
}

// Step 3: Create new HD session for existing patient
createHDSession(session: CreateHDScheduleRequest) {
  return this.http.post<ApiResponse<number>>(
    `${this.apiUrl}/hdschedule`,
    session
  );
}
```

### 3. Today's Schedule View
```typescript
// OLD
getActivePatients() {
  return this.http.get(`${this.apiUrl}/patients/active`);
}

// NEW
getTodaySchedules() {
  return this.http.get<ApiResponse<HDSchedule[]>>(
    `${this.apiUrl}/hdschedule/today`
  );
}

// Or get by slot
getSlotSchedules(slotId: number) {
  return this.http.get<ApiResponse<HDSchedule[]>>(
    `${this.apiUrl}/hdschedule/slot/${slotId}`
  );
}
```

## Files to Update

### Services to Create/Update
1. **patient.service.ts** - Demographics management
   - `getAll()`, `search()`, `getById()`, `create()`, `update()`, `delete()`

2. **hdschedule.service.ts** (NEW) - Session management
   - `getAll()`, `getToday()`, `getByPatient()`, `getBySlot()`, `create()`, `update()`, `discharge()`

### Components to Update
1. **patient-registration/** (NEW) - Two-step registration
   - Search existing patients
   - Create demographics if new
   - Navigate to HD session creation

2. **hd-session/** (NEW) - HD session management
   - Create new session for patient
   - Update session details
   - View session history

3. **today-schedule/** - Update to use HDSchedule
   - Display sessions grouped by slot
   - Show patient name (from join)
   - Show discharge status

4. **patient-search/** (NEW) - Quick patient lookup
   - Search by name/phone/MRN
   - Display results with click-through to sessions

## Migration Steps

1. **Create new services**
   ```bash
   cd Frontend/hd-scheduler-app/src/app/services
   ng generate service patient
   ng generate service hdschedule
   ```

2. **Create new models**
   ```bash
   cd Frontend/hd-scheduler-app/src/app/models
   # Create patient.model.ts
   # Create hdschedule.model.ts
   ```

3. **Create new components**
   ```bash
   ng generate component components/patient-registration
   ng generate component components/hd-session
   ng generate component components/patient-search
   ```

4. **Update existing components**
   - Update imports to use new models
   - Replace API calls to use new endpoints
   - Update templates to match new data structure

## Testing Checklist

- [ ] Search for patient by name
- [ ] Search for patient by phone number
- [ ] Search for patient by MRN
- [ ] Create new patient (demographics only)
- [ ] View patient details
- [ ] Update patient demographics
- [ ] Create HD session for existing patient
- [ ] View patient's session history
- [ ] View today's schedule
- [ ] View schedule by slot
- [ ] Update HD session
- [ ] Discharge patient from session
- [ ] View active vs discharged sessions

## Notes

- The old `/api/schedule` endpoints have been temporarily disabled
- Patient create/update now only handles demographics
- HD session data is in the new `/api/hdschedule` controller
- Search is optimized with indexes on Name, Phone1, and MRN
- Session history is available per patient via `/api/hdschedule/patient/{id}`
