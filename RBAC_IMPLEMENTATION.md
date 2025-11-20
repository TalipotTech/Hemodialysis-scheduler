# Enhanced Role-Based Access Control (RBAC) Implementation

## Overview
This document describes the comprehensive RBAC system implemented for the HD Scheduler application, ensuring secure and appropriate access control for all user roles.

---

## Role Permissions Matrix

| **Role** | **Route Access** | **HD Schedule** | **HD Logs** | **Monitoring** | **Medications** | **Patients** |
|----------|------------------|-----------------|-------------|----------------|-----------------|--------------|
| **Admin** | /admin-dashboard | Full CRUD | Full CRUD | Full CRUD | Full CRUD | Full CRUD |
| **HOD** | /hod-dashboard | Read-only | Read-only | Read-only | Read-only | Read-only |
| **Doctor** | /staff-entry | Create, Update | Create, Update | Create, Update | Create, Update | Full CRUD |
| **Nurse** | /staff-entry | Create, Update | Create, Update | Create, Update | Create, Update | Full CRUD |
| **Technician** | /technician-view | Read-only | Read-only | **Create, Update** | Read-only | Read-only |

---

## Detailed Permissions by Role

### 1. **Admin**
**Full system access**
- ✅ All routes accessible
- ✅ User management
- ✅ System settings
- ✅ Audit logs
- ✅ Create/Edit/Delete all records
- ✅ Reports generation

**Backend Authorization:**
```csharp
[Authorize(Roles = "Admin")]
```

---

### 2. **Doctor**
**Clinical prescriptions and patient care**
- ✅ Access: `/staff-entry` dashboard
- ✅ Create and edit patients
- ✅ Create HD schedules with prescriptions
- ✅ Add/update HD logs
- ✅ Add/update monitoring records
- ✅ Prescribe medications
- ❌ Cannot delete records (Admin only)
- ❌ Cannot access system settings

**Backend Authorization:**
```csharp
[Authorize(Roles = "Admin,Doctor,Nurse")]
```

---

### 3. **Nurse**
**Patient care and monitoring**
- ✅ Access: `/staff-entry` dashboard
- ✅ Create and edit patients
- ✅ Create HD schedules
- ✅ Add/update HD logs
- ✅ Add/update monitoring records
- ✅ Administer medications
- ❌ Cannot delete records (Admin only)
- ❌ Cannot access system settings

**Backend Authorization:**
```csharp
[Authorize(Roles = "Admin,Doctor,Nurse")]
```

---

### 4. **Technician**
**Monitoring-only access (Critical Restriction)**
- ✅ Access: `/technician-view` dashboard
- ✅ **View** HD schedules (read-only)
- ✅ **View** HD logs (read-only)
- ✅ **CREATE** IntraDialyticRecords (monitoring data)
- ✅ **UPDATE** IntraDialyticRecords (monitoring data)
- ✅ **View** medications (read-only)
- ❌ **CANNOT** create/edit HD schedules
- ❌ **CANNOT** create/edit HD logs
- ❌ **CANNOT** add/edit medications
- ❌ **CANNOT** delete any records
- ❌ **CANNOT** edit patient information

**Backend Authorization:**
```csharp
// Monitoring endpoints only
[Authorize(Roles = "Admin,Doctor,Nurse,Technician")]
```

**Key Endpoints for Technician:**
- `GET /api/HDLog/{id}` - View HD log
- `GET /api/HDLog/{hdLogId}/monitoring` - View monitoring records
- `POST /api/HDLog/{hdLogId}/monitoring` - **Add monitoring record** ✅
- `PUT /api/HDLog/monitoring/{monitoringId}` - **Update monitoring record** ✅
- `GET /api/HDLog/{hdLogId}/medications` - View medications (read-only)

---

### 5. **HOD (Head of Department)**
**Oversight and read-only access**
- ✅ Access: `/hod-dashboard`
- ✅ **View** all HD schedules
- ✅ **View** all HD logs
- ✅ **View** all monitoring records
- ✅ **View** all medications
- ✅ Staff management
- ✅ Generate reports
- ❌ **CANNOT** create/edit HD schedules
- ❌ **CANNOT** create/edit HD logs
- ❌ **CANNOT** create/edit monitoring records
- ❌ **CANNOT** prescribe medications

**Backend Authorization:**
```csharp
// Read endpoints
[Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]

// Management endpoints
[Authorize(Roles = "Admin,HOD")]
```

---

## Frontend Route Guards

### Protected Routes with Role Requirements

```typescript
// Admin-only routes
{ path: 'admin', data: { roles: ['Admin'] } }
{ path: 'admin/user-management', data: { roles: ['Admin'] } }
{ path: 'admin/system-settings', data: { roles: ['Admin'] } }
{ path: 'admin/audit-logs', data: { roles: ['Admin'] } }

// HOD routes
{ path: 'hod', data: { roles: ['HOD'] } }
{ path: 'admin/reports', data: { roles: ['Admin', 'HOD'] } }
{ path: 'admin/staff-management', data: { roles: ['Admin', 'HOD'] } }

// Doctor & Nurse routes
{ path: 'staff', data: { roles: ['Doctor', 'Nurse'] } }
{ path: 'patients', data: { roles: ['Admin', 'Doctor', 'Nurse'] } }
{ path: 'patients/new', data: { roles: ['Admin', 'Doctor', 'Nurse'] } }
{ path: 'schedule/hd-session/:patientId', data: { roles: ['Doctor', 'Nurse'] } }

// Technician routes
{ path: 'technician', data: { roles: ['Technician'] } }

// All authenticated users
{ path: 'schedule', data: { roles: ['Admin', 'HOD', 'Doctor', 'Nurse', 'Technician'] } }
{ path: 'patients/:id/history', data: { roles: ['Admin', 'Doctor', 'Nurse', 'HOD', 'Technician'] } }
```

---

## Backend Controller Authorizations

### HDScheduleController
```csharp
[HttpGet] // All roles can view
[Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]

[HttpPost] // Only Doctor/Nurse can create
[Authorize(Roles = "Admin,Doctor,Nurse")]

[HttpPut("{id}")] // Only Doctor/Nurse can update
[Authorize(Roles = "Admin,Doctor,Nurse")]

[HttpDelete("{id}")] // Only Admin can delete
[Authorize(Roles = "Admin")]
```

### HDLogController
```csharp
[HttpGet("{id}")] // All roles can view
[Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]

[HttpPost] // Only Doctor/Nurse can create
[Authorize(Roles = "Admin,Doctor,Nurse")]

[HttpPut("{id}")] // Only Doctor/Nurse can update (HOD excluded)
[Authorize(Roles = "Admin,Doctor,Nurse")]

// Monitoring endpoints
[HttpPost("{hdLogId}/monitoring")] // Technician CAN add
[Authorize(Roles = "Admin,Doctor,Nurse,Technician")]

[HttpPut("monitoring/{monitoringId}")] // Technician CAN update
[Authorize(Roles = "Admin,Doctor,Nurse,Technician")]

[HttpDelete("monitoring/{monitoringId}")] // Technician CANNOT delete
[Authorize(Roles = "Admin,Doctor,Nurse")]

// Medication endpoints
[HttpPost("{hdLogId}/medications")] // Technician CANNOT add
[Authorize(Roles = "Admin,Doctor,Nurse")]
```

---

## Security Benefits

1. **Principle of Least Privilege**: Each role has only the minimum access needed
2. **Separation of Duties**: Technicians monitor, Doctors prescribe
3. **Audit Trail**: All actions are logged with user role
4. **Compliance**: Meets healthcare data access requirements
5. **Error Prevention**: Technicians can't accidentally modify prescriptions
6. **Accountability**: Clear role-based responsibility tracking

---

## Testing Checklist

### Admin Testing
- [ ] Can access all dashboards
- [ ] Can create/edit/delete all records
- [ ] Can manage users and system settings

### Doctor Testing
- [ ] Can access /staff-entry
- [ ] Can create HD schedules and prescriptions
- [ ] Can add/edit HD logs
- [ ] Can add medications
- [ ] CANNOT access /admin routes

### Nurse Testing
- [ ] Can access /staff-entry
- [ ] Can create HD schedules
- [ ] Can add/edit monitoring records
- [ ] Can administer medications
- [ ] CANNOT access /admin routes

### Technician Testing
- [ ] Can access /technician-view
- [ ] Can view HD schedules (read-only)
- [ ] **CAN add monitoring records**
- [ ] **CAN update monitoring records**
- [ ] CANNOT edit HD schedules
- [ ] CANNOT edit HD logs
- [ ] CANNOT add/edit medications
- [ ] CANNOT access /staff-entry or /admin

### HOD Testing
- [ ] Can access /hod-dashboard
- [ ] Can view all HD logs (read-only)
- [ ] Can manage staff
- [ ] Can generate reports
- [ ] CANNOT create/edit HD schedules
- [ ] CANNOT create/edit HD logs
- [ ] CANNOT add monitoring records

---

## Implementation Date
November 13, 2025

## Status
✅ **IMPLEMENTED** - Enhanced RBAC is now active across the system.
