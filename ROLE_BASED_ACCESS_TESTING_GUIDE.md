# HD Scheduler - Role-Based Access Control Testing Guide

## Overview
This guide provides step-by-step instructions for testing role-based access control across all user roles in the HD Scheduler system.

---

## Test Environment Setup

### Prerequisites
1. Backend API running on `http://localhost:5001`
2. Frontend running on `http://localhost:4200`
3. SQLite database initialized with test users
4. JWT authentication enabled

### Test Users

Create or verify these test accounts exist in the database:

```sql
-- Admin User
INSERT INTO Users (Username, PasswordHash, Role, IsActive) 
VALUES ('admin', '[bcrypt_hash]', 'Admin', 1);

-- HOD User
INSERT INTO Users (Username, PasswordHash, Role, IsActive) 
VALUES ('hod', '[bcrypt_hash]', 'HOD', 1);

-- Doctor User
INSERT INTO Users (Username, PasswordHash, Role, IsActive) 
VALUES ('doctor', '[bcrypt_hash]', 'Doctor', 1);

-- Nurse User
INSERT INTO Users (Username, PasswordHash, Role, IsActive) 
VALUES ('nurse', '[bcrypt_hash]', 'Nurse', 1);

-- Technician User
INSERT INTO Users (Username, PasswordHash, Role, IsActive) 
VALUES ('technician', '[bcrypt_hash]', 'Technician', 1);
```

**Test Passwords**: Use the same password for all test accounts (e.g., "Test123!")

---

## Test Scenarios by Role

### 1. Admin Role Testing

#### Test Case 1.1: Full Access to Patient Management
**Steps**:
1. Login as `admin`
2. Navigate to **Patients** → **Patient List**
3. Click **Add New Patient**
4. Fill in patient details and click **Save Patient**
5. Edit an existing patient
6. View patient history
7. Attempt to discharge a patient

**Expected Results**:
- ✅ Can view all patients
- ✅ Can create new patients
- ✅ Can edit existing patients
- ✅ Can view patient history
- ✅ Can discharge patients
- ✅ "Add New Patient" button visible
- ✅ Edit and Discharge buttons visible in patient list

#### Test Case 1.2: Full Access to HD Sessions
**Steps**:
1. Navigate to **Schedule** → **HD Session Schedule**
2. Click **Schedule New Session**
3. Select patient, slot, bed, and fill prescription details
4. Click **Save Session**
5. Edit an existing session
6. Discharge a session

**Expected Results**:
- ✅ Can create HD sessions with full prescription details
- ✅ Can update sessions and prescriptions
- ✅ Can discharge sessions
- ✅ Can view all sessions

#### Test Case 1.3: Staff Management Access
**Steps**:
1. Navigate to **Admin** → **Staff Management**
2. Create a new staff member
3. Update staff details
4. Assign staff to slot
5. Delete a staff member

**Expected Results**:
- ✅ Full access to staff management
- ✅ Can create/update/delete staff
- ✅ Can assign staff to slots

---

### 2. HOD Role Testing

#### Test Case 2.1: Patient Viewing (No Create/Edit)
**Steps**:
1. Login as `hod`
2. Navigate to **Patients** → **Patient List**
3. Look for "Add New Patient" button
4. Click on a patient to view details

**Expected Results**:
- ✅ Can view all patients
- ✅ Can view patient history
- ❌ "Add New Patient" button NOT visible
- ❌ Cannot create new patients (403 Forbidden if attempted via API)
- ❌ Cannot edit patients

#### Test Case 2.2: Schedule Viewing and Management
**Steps**:
1. Navigate to **Schedule** → **Daily Schedule**
2. View today's schedule
3. Check bed availability
4. Attempt to discharge a patient

**Expected Results**:
- ✅ Can view daily schedule
- ✅ Can check bed availability
- ✅ Can discharge patients
- ❌ Cannot create new HD sessions

#### Test Case 2.3: Staff Management
**Steps**:
1. Navigate to **Admin** → **Staff Management**
2. Create a new staff member
3. Update staff details
4. Attempt to delete a staff member

**Expected Results**:
- ✅ Can view all staff
- ✅ Can create staff
- ✅ Can update staff
- ✅ Can assign staff to slots
- ❌ Cannot delete staff (Admin only)

---

### 3. Doctor Role Testing

#### Test Case 3.1: Patient Management
**Steps**:
1. Login as `doctor`
2. Navigate to **Patients** → **Patient List**
3. Click **Add New Patient**
4. Create a new patient
5. Edit existing patient
6. Discharge a patient

**Expected Results**:
- ✅ Can view all patients
- ✅ Can create new patients
- ✅ Can edit patients
- ✅ Can discharge patients
- ✅ "Add New Patient" button visible

#### Test Case 3.2: HD Session Prescription
**Steps**:
1. Navigate to **Schedule** → **HD Session Schedule**
2. Click **Schedule New Session**
3. Fill in prescription details:
   - Dry Weight
   - Dialyser Type
   - Prescribed Duration
   - UF Goal
   - Blood Flow Rate
   - Anticoagulation Type
4. Save session

**Expected Results**:
- ✅ Can create HD sessions with full prescription
- ✅ Can update prescriptions
- ✅ Can assign Doctor to session
- ✅ All prescription fields editable

#### Test Case 3.3: Equipment Management
**Steps**:
1. Create a session with equipment counts
2. View equipment usage alerts
3. Acknowledge equipment alerts

**Expected Results**:
- ✅ Can view equipment status
- ✅ Can see auto-incremented equipment counts
- ✅ Can acknowledge alerts
- ✅ Equipment alerts displayed when limits reached

#### Test Case 3.4: No Staff Management Access
**Steps**:
1. Attempt to navigate to **Staff Management**

**Expected Results**:
- ❌ Staff Management not visible in menu
- ❌ Direct URL access returns 403 Forbidden

---

### 4. Nurse Role Testing

#### Test Case 4.1: Patient Management
**Steps**:
1. Login as `nurse`
2. Navigate to **Patients** → **Patient List**
3. Create a new patient
4. Edit existing patient

**Expected Results**:
- ✅ Can view all patients
- ✅ Can create new patients
- ✅ Can edit patients
- ✅ "Add New Patient" button visible

#### Test Case 4.2: HD Session Execution
**Steps**:
1. Navigate to **Schedule** → **HD Session Schedule**
2. Create a new session
3. During session monitoring:
   - Update vital signs (BP, Pulse, Temperature)
   - Record intra-dialytic data
   - Add medications
4. Use auto-save feature (PATCH endpoint)

**Expected Results**:
- ✅ Can create HD sessions
- ✅ Can update session vitals
- ✅ Can administer medications
- ✅ Auto-save works during treatment
- ✅ Can discharge patients after session complete

#### Test Case 4.3: Medication Administration
**Steps**:
1. Open a session details
2. Add post-dialysis medication:
   - Medication Name
   - Dosage
   - Route
   - Administered By
3. Save medication

**Expected Results**:
- ✅ Can add medications
- ✅ Medication saved with nurse's username
- ✅ Can view medication history

#### Test Case 4.4: No Staff Management Access
**Steps**:
1. Attempt to access Staff Management

**Expected Results**:
- ❌ Staff Management not in menu
- ❌ Direct URL returns 403 Forbidden

---

### 5. Technician Role Testing (READ-ONLY)

#### Test Case 5.1: Patient List - View Only
**Steps**:
1. Login as `technician`
2. Navigate to **Patients** → **Patient List**
3. Look for "Add New Patient" button
4. Click on a patient

**Expected Results**:
- ✅ Can view patient list
- ✅ All patient data visible
- ❌ "Add New Patient" button NOT visible
- ❌ **READ-ONLY ACCESS** chip displayed
- ✅ View icon (👁️) instead of Edit icon in actions
- ❌ No Discharge button visible

#### Test Case 5.2: Patient Form - Read Only
**Steps**:
1. Click on a patient to view details
2. Check if form fields are editable
3. Look for "Save" button

**Expected Results**:
- ✅ Patient details displayed
- ✅ **READ-ONLY MODE** banner at top
- ❌ All form fields disabled (grayed out)
- ❌ No "Save Patient" button
- ✅ Only "Back" button visible

#### Test Case 5.3: Schedule Viewing
**Steps**:
1. Navigate to **Schedule** → **Daily Schedule**
2. View today's sessions
3. Check bed availability
4. Look for "Schedule New Session" button

**Expected Results**:
- ✅ Can view daily schedule
- ✅ Can see occupied beds
- ✅ Can check bed availability
- ❌ Cannot create new sessions
- ❌ "Schedule New Session" button NOT visible

#### Test Case 5.4: HD Session Details - View Only
**Steps**:
1. Click on a session to view details
2. Attempt to edit any field
3. Look for "Save" or "Update" buttons

**Expected Results**:
- ✅ Can view session details
- ✅ Can see patient vitals
- ✅ Can view equipment status
- ❌ All fields disabled/read-only
- ❌ No save buttons visible
- ❌ Cannot auto-save changes

#### Test Case 5.5: Equipment Status Viewing
**Steps**:
1. View a session with equipment usage
2. Check equipment alerts

**Expected Results**:
- ✅ Can view equipment counts (Dialyser, Blood Tubing)
- ✅ Can see equipment alerts
- ❌ Cannot acknowledge alerts (no button)
- ❌ Cannot update equipment counts

#### Test Case 5.6: Patient History - View Only
**Steps**:
1. Navigate to patient history
2. View treatment trends
3. View session details

**Expected Results**:
- ✅ Can view full patient history
- ✅ Can see vital trends charts
- ✅ Can view session statistics
- ❌ Cannot add notes
- ❌ Cannot edit historical data

#### Test Case 5.7: Dashboard - Read-Only Stats
**Steps**:
1. Login as `technician`
2. View Technician Dashboard

**Expected Results**:
- ✅ Dashboard shows today's session count
- ✅ Shows active patient count
- ✅ Displays today's sessions
- ✅ **READ-ONLY ACCESS** warning visible
- ❌ No action buttons to create/edit
- ✅ Can click "View Details" to see patient info (read-only)

#### Test Case 5.8: API Access Restrictions
**Steps**:
1. Using browser DevTools or Postman, attempt to:
   - POST /api/patients (create patient)
   - PUT /api/patients/{id} (update patient)
   - POST /api/hdschedule (create session)
   - PUT /api/hdschedule/{id} (update session)
   - POST /api/schedule/force-discharge/{id} (discharge)

**Expected Results**:
- ❌ All write operations return **403 Forbidden**
- ❌ Error message: "User does not have the required role"
- ✅ GET requests work (can view data)

---

## API Endpoint Testing

### Using Postman or curl

#### 1. Obtain JWT Token
```bash
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "technician",
  "password": "Test123!"
}
```

Copy the `token` from response.

#### 2. Test READ Access (Should Work)
```bash
GET http://localhost:5001/api/patients
Authorization: Bearer [your_token]
```

**Expected**: 200 OK with patient list

#### 3. Test WRITE Access (Should Fail for Technician)
```bash
POST http://localhost:5001/api/patients
Authorization: Bearer [your_token]
Content-Type: application/json

{
  "name": "Test Patient",
  "age": 45,
  "gender": "Male",
  "contactNumber": "1234567890",
  "mrn": "MRN123"
}
```

**Expected**: 403 Forbidden

#### 4. Test Doctor/Nurse WRITE Access (Should Work)
Login as `doctor` or `nurse`, get token, then:

```bash
POST http://localhost:5001/api/patients
Authorization: Bearer [doctor_token]
Content-Type: application/json

{
  "name": "Test Patient",
  "age": 45,
  "gender": "Male",
  "contactNumber": "1234567890",
  "mrn": "MRN123"
}
```

**Expected**: 201 Created

---

## Automated Testing Script (PowerShell)

```powershell
# Role-Based Access Control Test Script

$apiUrl = "http://localhost:5001/api"
$roles = @("admin", "hod", "doctor", "nurse", "technician")
$password = "Test123!"

function Test-RoleAccess {
    param($username, $role)
    
    Write-Host "`n===== Testing $role Role ($username) =====" -ForegroundColor Cyan
    
    # Login
    $loginBody = @{ username = $username; password = $password } | ConvertTo-Json
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    
    # Test GET Patients (should work for all)
    try {
        $headers = @{ Authorization = "Bearer $token" }
        $patients = Invoke-RestMethod -Uri "$apiUrl/patients" -Headers $headers
        Write-Host "✅ GET /patients: SUCCESS" -ForegroundColor Green
    } catch {
        Write-Host "❌ GET /patients: FAILED" -ForegroundColor Red
    }
    
    # Test POST Patients (should fail for Technician)
    try {
        $newPatient = @{ 
            name = "Test Patient"
            age = 45
            gender = "Male"
            contactNumber = "1234567890"
            mrn = "TEST001"
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$apiUrl/patients" -Method Post -Headers $headers -Body $newPatient -ContentType "application/json"
        Write-Host "✅ POST /patients: SUCCESS" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode -eq 403) {
            if ($role -eq "Technician") {
                Write-Host "✅ POST /patients: CORRECTLY FORBIDDEN (403)" -ForegroundColor Green
            } else {
                Write-Host "❌ POST /patients: INCORRECTLY FORBIDDEN" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ POST /patients: ERROR $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
    # Test POST HD Schedule (should fail for Technician and HOD)
    try {
        $newSession = @{
            patientID = 1
            sessionDate = (Get-Date).ToString("yyyy-MM-dd")
            slotID = 1
            bedNumber = 1
            dryWeight = 70
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$apiUrl/hdschedule" -Method Post -Headers $headers -Body $newSession -ContentType "application/json"
        Write-Host "✅ POST /hdschedule: SUCCESS" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode -eq 403) {
            if ($role -in @("Technician", "HOD")) {
                Write-Host "✅ POST /hdschedule: CORRECTLY FORBIDDEN (403)" -ForegroundColor Green
            } else {
                Write-Host "❌ POST /hdschedule: INCORRECTLY FORBIDDEN" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ POST /hdschedule: ERROR $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
}

# Run tests for all roles
foreach ($role in $roles) {
    Test-RoleAccess -username $role -role $role
}

Write-Host "`n===== Testing Complete =====" -ForegroundColor Cyan
```

**Run**: `.\test-rbac-full.ps1`

---

## Expected Test Results Summary

| Action | Admin | HOD | Doctor | Nurse | Technician |
|--------|-------|-----|--------|-------|------------|
| **View Patients** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Create Patient** | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Edit Patient** | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Delete Patient** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **View Schedule** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Create HD Session** | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Update HD Session** | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Discharge Patient** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Auto-save Session** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **View History** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **View Equipment** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Ack Equipment Alert** | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Manage Staff** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Manage Users** | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## Common Issues & Troubleshooting

### Issue 1: All Roles Can Edit
**Symptom**: Technician can create/edit records  
**Cause**: Missing `[Authorize(Roles)]` attribute on controller  
**Fix**: Add proper `[Authorize(Roles = "Admin,Doctor,Nurse")]` to endpoints

### Issue 2: Frontend Buttons Still Visible
**Symptom**: Edit/Save buttons visible for Technician  
**Cause**: Missing `*ngIf="!isReadOnly"` directive  
**Fix**: Update component template to hide buttons based on `isReadOnly` flag

### Issue 3: 401 Unauthorized Instead of 403 Forbidden
**Symptom**: Getting 401 when expecting 403  
**Cause**: JWT token expired or missing  
**Fix**: Re-login to get fresh token

### Issue 4: Form Fields Not Disabled
**Symptom**: Technician can type in form fields  
**Cause**: Form not disabled in ngOnInit  
**Fix**: Add `this.patientForm.disable()` for read-only users

---

## Acceptance Criteria Checklist

### ✅ Backend Authorization
- [ ] All endpoints have appropriate `[Authorize(Roles = "...")]` attributes
- [ ] Technicians receive 403 Forbidden on write operations
- [ ] Doctors and Nurses can create/update patients and sessions
- [ ] HOD can manage staff but not create sessions
- [ ] Admin has full access to all endpoints

### ✅ Frontend UI Controls
- [ ] Technicians see "READ-ONLY ACCESS" indicators
- [ ] Create/Edit buttons hidden for Technicians
- [ ] Form fields disabled for Technicians
- [ ] Save buttons not visible for Technicians
- [ ] View icons replace Edit icons for Technicians

### ✅ Medical Workflow
- [ ] Doctors can prescribe dialysis (create sessions)
- [ ] Nurses can execute sessions (update vitals, medications)
- [ ] Technicians can view all data (read-only)
- [ ] All staff can view patient history and trends
- [ ] Equipment alerts work for all roles

### ✅ Security
- [ ] JWT tokens required for all API calls
- [ ] Role claims validated on every request
- [ ] No role escalation possible
- [ ] Audit logs capture all write operations

---

## Final Validation

After completing all tests:

1. **Login as each role** and verify dashboard shows appropriate actions
2. **Attempt unauthorized actions** via browser DevTools (should fail)
3. **Check audit logs** to ensure all operations are logged
4. **Test mobile/tablet views** to ensure role restrictions work on all devices
5. **Performance test** to ensure role checks don't slow down API

---

**Status**: ✅ Ready for Testing  
**Last Updated**: ${new Date().toLocaleDateString()}  
**Tested By**: _____________________  
**Date**: _____________________
