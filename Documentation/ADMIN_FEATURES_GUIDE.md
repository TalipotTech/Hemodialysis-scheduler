# Admin Features Implementation Guide

## Overview
This document describes all admin management features implemented in the HD Scheduler system. All endpoints require Admin role authorization (StaffManagement allows HOD role as well).

## 1. User Management API
**Base URL:** `/api/usermanagement`  
**Authorization:** Admin only

### Endpoints

#### Get All Users
```http
GET /api/usermanagement
```
Returns list of all users (passwords excluded from response).

#### Get User by ID
```http
GET /api/usermanagement/{id}
```

#### Create User
```http
POST /api/usermanagement
Body: {
  "username": "string",
  "password": "string",
  "role": "Admin|HOD|Doctor|Nurse|Technician"
}
```
- Validates username uniqueness
- Hashes password with BCrypt
- Creates audit log entry

#### Update User
```http
PUT /api/usermanagement/{id}
Body: {
  "username": "string",
  "role": "string"
}
```
- Updates username and role
- Creates audit log entry

#### Delete User
```http
DELETE /api/usermanagement/{id}
```
- Prevents self-deletion
- Creates audit log entry

#### Reset Password
```http
POST /api/usermanagement/{id}/reset-password
Body: {
  "newPassword": "string"
}
```
- Hashes new password with BCrypt
- Creates audit log entry

#### Toggle User Status (Enable/Disable)
```http
POST /api/usermanagement/{id}/toggle-status
```
- Prevents self-disable
- Creates audit log entry

---

## 2. Staff Management API
**Base URL:** `/api/staffmanagement`  
**Authorization:** Admin, HOD

### Endpoints

#### Get All Staff
```http
GET /api/staffmanagement
```

#### Get Active Staff Only
```http
GET /api/staffmanagement/active
```

#### Get Staff by ID
```http
GET /api/staffmanagement/{id}
```

#### Get Staff by Role
```http
GET /api/staffmanagement/role/{role}
```

#### Get Staff by Slot
```http
GET /api/staffmanagement/slot/{slotId}
```

#### Create Staff
```http
POST /api/staffmanagement
Body: {
  "name": "string",
  "role": "Doctor|Nurse|Technician|HOD",
  "assignedSlot": number (nullable)
}
```

#### Update Staff
```http
PUT /api/staffmanagement/{id}
Body: {
  "name": "string",
  "role": "string",
  "assignedSlot": number (nullable),
  "isActive": boolean
}
```

#### Delete Staff
```http
DELETE /api/staffmanagement/{id}
```
**Admin only**

#### Assign to Slot
```http
POST /api/staffmanagement/{id}/assign-slot
Body: {
  "slotID": number (nullable)
}
```

#### Toggle Staff Status
```http
POST /api/staffmanagement/{id}/toggle-status
```

---

## 3. System Settings API
**Base URL:** `/api/systemsettings`  
**Authorization:** Admin only

### Slot Management

#### Get All Slots
```http
GET /api/systemsettings/slots
```

#### Get Slot by ID
```http
GET /api/systemsettings/slots/{id}
```

#### Create Slot
```http
POST /api/systemsettings/slots
Body: {
  "slotName": "string",
  "startTime": "HH:mm:ss",
  "endTime": "HH:mm:ss",
  "maxBeds": number
}
```

#### Update Slot
```http
PUT /api/systemsettings/slots/{id}
Body: {
  "slotName": "string",
  "startTime": "HH:mm:ss",
  "endTime": "HH:mm:ss",
  "maxBeds": number,
  "isActive": boolean
}
```

#### Delete Slot
```http
DELETE /api/systemsettings/slots/{id}
```
Cannot delete slots with existing bed assignments.

### Bed Configuration

#### Get Bed Capacity
```http
GET /api/systemsettings/beds/capacity
```
Returns bed capacity and occupancy for all slots.

#### Update Bed Capacity
```http
PUT /api/systemsettings/beds/capacity/{slotId}
Body: {
  "maxBeds": number
}
```
Cannot reduce capacity below current usage.

### System Parameters

#### Get System Parameters
```http
GET /api/systemsettings/parameters
```
Returns overview of system configuration:
- Total active slots
- Total bed capacity
- Total active patients
- Total active staff
- Database version

---

## 4. Reports API
**Base URL:** `/api/reports`  
**Authorization:** Admin, HOD

### Patient Volume Report
```http
GET /api/reports/patient-volume?startDate={date}&endDate={date}
```
Returns:
- Total patients and sessions
- Daily patient volume
- Volume by slot

### Occupancy Rates Report
```http
GET /api/reports/occupancy-rates?date={date}
```
Returns:
- Overall occupancy rate
- Bed utilization by slot
- Available beds

### Treatment Completion Report
```http
GET /api/reports/treatment-completion?startDate={date}&endDate={date}
```
Returns:
- Overall completion rate
- Completion rate per patient
- Session statistics

### Staff Performance Report
```http
GET /api/reports/staff-performance?startDate={date}&endDate={date}
```
Returns:
- Sessions handled per staff
- Average session duration
- Completed sessions count

### Monthly Summary
```http
GET /api/reports/monthly-summary?year={year}&month={month}
```
Returns comprehensive monthly statistics:
- Total and new patients
- Session completion rate
- Average occupancy rate

### Yearly Summary
```http
GET /api/reports/yearly-summary?year={year}
```
Returns:
- Total patients and sessions
- Monthly breakdown

---

## 5. Audit Logs API
**Base URL:** `/api/auditlogs`  
**Authorization:** Admin only

### Get All Audit Logs
```http
GET /api/auditlogs?page=1&pageSize=50
```
Paginated results, ordered by most recent.

### Get Logs by User
```http
GET /api/auditlogs/user/{userId}
```

### Get Logs by Entity
```http
GET /api/auditlogs/entity/{entityType}/{entityId}
```
Example: `/api/auditlogs/entity/Patient/123`

### Get Logs by Date Range
```http
GET /api/auditlogs/daterange?startDate={date}&endDate={date}
```

### Get Activity Summary
```http
GET /api/auditlogs/activity-summary?startDate={date}&endDate={date}
```
Returns user activity summary for the period.

### Get Login History
```http
GET /api/auditlogs/login-history?userId={id}&days={days}
```
User ID is optional; defaults to 30 days.

### Get Logs by Action
```http
GET /api/auditlogs/actions/{action}
```
Example: `/api/auditlogs/actions/CREATE`

### Get Audit Statistics
```http
GET /api/auditlogs/statistics?days={days}
```
Returns comprehensive statistics:
- Total actions and unique users
- Action counts by type (LOGIN, CREATE, UPDATE, DELETE)
- Most active user
- Most common action
- Actions by day

---

## Database Schema

### AuditLogs Table
```sql
CREATE TABLE AuditLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NULL,
    Username NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50) NULL,
    EntityID INT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IPAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
);
```

**Indexes:**
- IX_AuditLogs_UserID
- IX_AuditLogs_EntityType
- IX_AuditLogs_EntityID
- IX_AuditLogs_CreatedAt (DESC)
- IX_AuditLogs_Action

---

## Frontend Implementation Guide

### Components to Create

1. **User Management Component**
   - User list with search/filter
   - Create/edit user dialog
   - Password reset dialog
   - Enable/disable toggle
   - Delete confirmation

2. **Staff Management Component**
   - Staff list with role filter
   - Create/edit staff dialog
   - Slot assignment interface
   - Active/inactive toggle

3. **System Settings Component**
   - Slot configuration grid
   - Bed capacity management
   - System parameters display

4. **Reports Dashboard**
   - Date range selectors
   - Report type tabs
   - Chart visualizations (Chart.js or ngx-charts)
   - Export functionality

5. **Audit Logs Viewer**
   - Searchable log table
   - Filters (user, action, date range, entity)
   - Statistics dashboard

### Navigation Updates

Add to admin dashboard menu:
```typescript
menuItems = [
  { icon: 'people', label: 'User Management', route: '/admin/users' },
  { icon: 'groups', label: 'Staff Management', route: '/admin/staff' },
  { icon: 'settings', label: 'System Settings', route: '/admin/settings' },
  { icon: 'assessment', label: 'Reports', route: '/admin/reports' },
  { icon: 'history', label: 'Audit Logs', route: '/admin/audit-logs' }
];
```

---

## Testing with Swagger

All APIs are documented in Swagger UI:
- Open: `https://localhost:7001/swagger`
- Authenticate with admin user (admin/Admin@123)
- Test all endpoints interactively

---

## Audit Trail

All admin actions automatically create audit log entries:
- User management: CREATE, UPDATE, DELETE, RESET_PASSWORD, ENABLED/DISABLED
- Staff management: CREATE, UPDATE, DELETE, ASSIGN_SLOT, ENABLED/DISABLED
- System settings: CREATE, UPDATE, DELETE, UPDATE_CAPACITY

Each audit entry includes:
- User performing the action
- Timestamp
- Old and new values
- IP address
- Entity affected

---

## Next Steps

1. Create Angular services for each API
2. Build Angular components with Material Design
3. Add routing and navigation guards
4. Implement data visualization for reports
5. Add export functionality (PDF, Excel)
6. Implement real-time notifications for critical actions
