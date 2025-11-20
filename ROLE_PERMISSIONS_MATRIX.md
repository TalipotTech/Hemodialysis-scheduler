# HD Scheduler - Role-Based Access Control Matrix

## Overview
This document provides a comprehensive view of all role-based permissions in the HD Scheduler system. Each medical role has specific access rights based on their responsibilities in the hemodialysis workflow.

---

## Role Definitions

### 1. **Admin**
- System administrator with full access
- Can manage users, staff, and system settings
- Can perform all operations

### 2. **HOD (Head of Department)**
- Department head with oversight capabilities
- Can manage staff assignments
- Full access to all patient data and schedules

### 3. **Doctor**
- Prescribes dialysis treatment plans
- Creates and updates HD sessions
- Can discharge patients
- Full access to patient history and statistics

### 4. **Nurse**
- Executes dialysis sessions
- Monitors patients during treatment
- Creates and updates HD sessions
- Can discharge patients
- Administers medications

### 5. **Technician**
- Technical support for dialysis equipment
- **READ-ONLY** access to schedules and patient data
- Can view but cannot create/modify records
- Helps with equipment maintenance alerts

---

## Permission Matrix by Controller

### PatientsController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/patients | ✅ | ✅ | ✅ | ✅ | ✅ | View all patients |
| GET /api/patients/active | ✅ | ✅ | ✅ | ✅ | ✅ | View active patients |
| GET /api/patients/search | ✅ | ✅ | ✅ | ✅ | ✅ | Search patients |
| GET /api/patients/{id} | ✅ | ✅ | ✅ | ✅ | ✅ | View patient details |
| GET /api/patients/{id}/with-sessions | ✅ | ✅ | ✅ | ✅ | ✅ | View patient with sessions |
| POST /api/patients | ✅ | ❌ | ✅ | ✅ | ❌ | Create new patient |
| PUT /api/patients/{id} | ✅ | ❌ | ✅ | ✅ | ❌ | Update patient info |
| DELETE /api/patients/{id} | ✅ | ❌ | ❌ | ❌ | ❌ | Delete patient (Admin only) |

**Workflow**: Doctors and Nurses register and update patient information. Technicians can view but not modify.

---

### ScheduleController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/schedule/daily | ✅ | ✅ | ✅ | ✅ | ✅ | View daily bed schedule |
| GET /api/schedule/availability | ✅ | ✅ | ✅ | ✅ | ✅ | Check bed availability |
| POST /api/schedule/move-to-history | ✅ | ✅ | ✅ | ✅ | ❌ | Move completed sessions |
| POST /api/schedule/force-discharge/{id} | ✅ | ✅ | ✅ | ✅ | ❌ | Manually discharge patient |

**Workflow**: All staff can view schedules. Doctors/Nurses can discharge patients. Technicians view-only.

---

### HDScheduleController (HD Sessions)

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/hdschedule | ✅ | ✅ | ✅ | ✅ | ✅ | View all HD schedules |
| GET /api/hdschedule/active | ✅ | ✅ | ✅ | ✅ | ✅ | View active sessions |
| GET /api/hdschedule/today | ✅ | ✅ | ✅ | ✅ | ✅ | View today's sessions |
| GET /api/hdschedule/history | ✅ | ✅ | ✅ | ✅ | ✅ | View session history |
| GET /api/hdschedule/{id} | ✅ | ✅ | ✅ | ✅ | ✅ | View session details |
| GET /api/hdschedule/patient/{patientId} | ✅ | ✅ | ✅ | ✅ | ✅ | View patient's sessions |
| GET /api/hdschedule/slot/{slotId} | ✅ | ✅ | ✅ | ✅ | ✅ | View slot schedule |
| POST /api/hdschedule | ✅ | ❌ | ✅ | ✅ | ❌ | Create HD session |
| PATCH /api/hdschedule/{id}/auto-save | ✅ | ✅ | ✅ | ✅ | ✅ | Auto-save during treatment |
| PUT /api/hdschedule/{id} | ✅ | ❌ | ✅ | ✅ | ❌ | Update prescription |
| PUT /api/hdschedule/{id}/discharge | ✅ | ❌ | ✅ | ✅ | ❌ | Discharge patient |
| DELETE /api/hdschedule/{id} | ✅ | ❌ | ❌ | ❌ | ❌ | Delete session (Admin only) |

**Workflow**: 
- Doctors prescribe dialysis treatment (create sessions with prescriptions)
- Nurses execute sessions (update vitals, medications)
- All staff can auto-save during treatment monitoring
- Technicians can view but not create/modify sessions

---

### HDScheduleController - Equipment Management

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/hdschedule/patient/{id}/suggested-equipment-counts | ✅ | ✅ | ✅ | ✅ | ✅ | Get auto-incremented counts |
| GET /api/hdschedule/patient/{id}/equipment-status | ✅ | ✅ | ✅ | ✅ | ✅ | Check equipment usage |
| GET /api/hdschedule/{id}/equipment-status | ✅ | ✅ | ✅ | ✅ | ✅ | Schedule equipment status |
| GET /api/hdschedule/patient/{id}/equipment-alerts | ✅ | ✅ | ✅ | ✅ | ✅ | Get equipment alerts |
| POST /api/hdschedule/{id}/check-equipment-alerts | ✅ | ❌ | ✅ | ✅ | ❌ | Create equipment alerts |
| PUT /api/hdschedule/equipment-alerts/{id}/acknowledge | ✅ | ❌ | ✅ | ✅ | ❌ | Acknowledge alert |

**Workflow**: All staff can view equipment status. Doctors/Nurses manage alerts. Technicians help with equipment checks.

---

### PatientHistoryController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/patienthistory/{patientId} | ✅ | ✅ | ✅ | ✅ | ✅ | View treatment history |
| GET /api/patienthistory/session/{scheduleId} | ✅ | ✅ | ✅ | ✅ | ✅ | View session details |
| GET /api/patienthistory/{patientId}/trends | ✅ | ✅ | ✅ | ✅ | ✅ | View vital trends |
| GET /api/patienthistory/{patientId}/statistics | ✅ | ✅ | ✅ | ✅ | ✅ | View treatment stats |

**Workflow**: All medical staff can view patient history, trends, and statistics for monitoring and decision-making.

---

### StaffManagementController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| GET /api/staffmanagement | ✅ | ✅ | ❌ | ❌ | ❌ | View all staff |
| GET /api/staffmanagement/active | ✅ | ✅ | ❌ | ❌ | ❌ | View active staff |
| GET /api/staffmanagement/{id} | ✅ | ✅ | ❌ | ❌ | ❌ | View staff details |
| GET /api/staffmanagement/role/{role} | ✅ | ✅ | ❌ | ❌ | ❌ | View staff by role |
| GET /api/staffmanagement/slot/{slotId} | ✅ | ✅ | ❌ | ❌ | ❌ | View staff by slot |
| POST /api/staffmanagement | ✅ | ✅ | ❌ | ❌ | ❌ | Create staff |
| PUT /api/staffmanagement/{id} | ✅ | ✅ | ❌ | ❌ | ❌ | Update staff |
| DELETE /api/staffmanagement/{id} | ✅ | ❌ | ❌ | ❌ | ❌ | Delete staff (Admin only) |
| POST /api/staffmanagement/{id}/assign-slot | ✅ | ✅ | ❌ | ❌ | ❌ | Assign to slot |
| POST /api/staffmanagement/{id}/toggle-status | ✅ | ✅ | ❌ | ❌ | ❌ | Enable/disable staff |

**Workflow**: Only Admin and HOD can manage staff. Medical staff cannot access staff management.

---

### UserManagementController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| All endpoints | ✅ | ❌ | ❌ | ❌ | ❌ | User account management |

**Workflow**: Admin-only for security. Creates user accounts for all staff.

---

### SystemSettingsController

| Endpoint | Admin | HOD | Doctor | Nurse | Technician | Purpose |
|----------|-------|-----|--------|-------|------------|---------|
| All endpoints | ✅ | ❌ | ❌ | ❌ | ❌ | System configuration |

**Workflow**: Admin-only for system-wide settings and configuration.

---

## Summary by Role

### **Admin** (Full Access)
- Complete system access
- User and staff management
- System configuration
- All patient and schedule operations
- Can delete records

### **HOD** (Management + Oversight)
- Staff management (create, update, assign)
- Full view access to all patient data
- Can manage schedules and discharges
- Cannot create/update HD sessions (clinical task)
- Cannot delete records

### **Doctor** (Clinical Decision Maker)
- Create and update patients
- Prescribe dialysis treatment (create HD sessions)
- Update treatment parameters
- Discharge patients
- Full read access to all data
- Cannot delete records
- Cannot manage staff

### **Nurse** (Clinical Executor)
- Create and update patients
- Execute dialysis sessions (create HD sessions)
- Monitor vitals and update session data
- Administer medications
- Discharge patients
- Auto-save during treatment
- Full read access to all data
- Cannot delete records
- Cannot manage staff

### **Technician** (Technical Support - Read Only)
- **VIEW ONLY** access to all patient data
- **VIEW ONLY** access to all schedules
- Can view equipment status and alerts
- Can auto-save during treatment monitoring
- **CANNOT** create or update patients
- **CANNOT** create or update sessions
- **CANNOT** discharge patients
- **CANNOT** manage staff
- **CANNOT** delete anything

---

## Medical Workflow Integration

### 1. **Patient Registration**
- **Doctor/Nurse**: Registers new patient → POST /api/patients
- **Technician**: Views patient list (read-only)

### 2. **Treatment Prescription**
- **Doctor**: Creates HD session with prescription → POST /api/hdschedule
- **Nurse**: Can also create sessions based on doctor's orders
- **Technician**: Views scheduled sessions

### 3. **Session Execution**
- **Nurse**: Updates vitals during treatment → PATCH /api/hdschedule/{id}/auto-save
- **Doctor**: Monitors and adjusts if needed
- **Technician**: Can view session progress, checks equipment

### 4. **Patient Discharge**
- **Doctor/Nurse**: Marks session complete → POST /api/schedule/force-discharge/{id}
- **Background Service**: Auto-moves completed sessions to history
- **Technician**: Views discharge status

### 5. **Equipment Management**
- **All Staff**: View equipment alerts
- **Doctor/Nurse**: Acknowledge and resolve alerts
- **Technician**: Assists with equipment checks

---

## Security Notes

1. **JWT Authentication**: All endpoints require valid JWT token with role claim
2. **Role Claims**: Token must include user's role (Admin, HOD, Doctor, Nurse, Technician)
3. **Authorization Middleware**: Validates role before allowing access to endpoints
4. **Audit Logging**: All create/update/delete operations are logged with user info
5. **Data Privacy**: All medical staff roles have equal view access (HIPAA compliance)
6. **Principle of Least Privilege**: Technicians have minimal (read-only) access

---

## Testing Role-Based Access

### Test Plan
1. **Login as Doctor** → Verify can create patients and sessions
2. **Login as Nurse** → Verify can create patients and sessions
3. **Login as Technician** → Verify cannot create/update, only view
4. **Login as HOD** → Verify can manage staff but not create sessions
5. **Login as Admin** → Verify full access to all endpoints

### Expected Behaviors
- ✅ Authorized actions return 200/201 with data
- ❌ Unauthorized actions return 403 Forbidden
- ❌ Missing/invalid token returns 401 Unauthorized

---

## API Response Examples

### ✅ Successful Authorized Request
```json
{
  "success": true,
  "message": "Patient created successfully",
  "data": { "patientId": 123 }
}
```

### ❌ Unauthorized Access (Wrong Role)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have the required role"
}
```

### ❌ Missing Authentication
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authorization token is missing or invalid"
}
```

---

## Future Enhancements

1. **Fine-grained Permissions**: Add specific permissions beyond roles (e.g., "canPrescribeHeparin")
2. **Department-based Access**: Restrict users to their assigned department/unit
3. **Slot-based Access**: Staff can only access data for their assigned time slots
4. **Patient-specific Consent**: Allow patients to restrict access to certain staff
5. **Activity Monitoring**: Track all read operations for audit compliance

---

**Last Updated**: ${new Date().toLocaleDateString()}  
**Status**: ✅ Role-based access fully implemented and tested
