# RBAC Quick Reference Guide

## Role Permission Matrix

### Who Can Do What?

| Action | Admin | Doctor | Nurse | Technician | HOD |
|--------|:-----:|:------:|:-----:|:----------:|:---:|
| **View HD Schedules** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Create HD Schedule** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Edit HD Schedule** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Delete HD Schedule** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **View HD Logs** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Create HD Log** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Edit HD Log** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Delete HD Log** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **View Monitoring** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Add Monitoring** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Edit Monitoring** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Delete Monitoring** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **View Medications** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Add Medications** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Delete Medications** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **View Patients** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Add/Edit Patients** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Delete Patients** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Manage Staff** | ✅ | ❌ | ❌ | ❌ | ✅ |
| **View Reports** | ✅ | ❌ | ❌ | ❌ | ✅ |
| **User Management** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **System Settings** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Audit Logs** | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## Code Examples

### Backend: Adding Authorization to Controller

```csharp
// All roles can view
[HttpGet]
[Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
public async Task<IActionResult> GetSchedules()
{
    // Your code here
}

// Only Doctor and Nurse can create
[HttpPost]
[Authorize(Roles = "Admin,Doctor,Nurse")]
public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
{
    // Your code here
}

// Only Admin can delete
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteSchedule(int id)
{
    // Your code here
}

// Technician can add monitoring
[HttpPost("{hdLogId}/monitoring")]
[Authorize(Roles = "Admin,Doctor,Nurse,Technician")]
public async Task<IActionResult> AddMonitoring(int hdLogId, [FromBody] MonitoringRequest request)
{
    // Your code here
}
```

### Frontend: Adding Role Guards to Routes

```typescript
{
  path: 'patients/:id/hd-session',
  loadComponent: () => import('./features/patients/hd-session-form/hd-session-form.component')
    .then(m => m.HdSessionFormComponent),
  canActivate: [authGuard],
  data: { roles: ['Doctor', 'Nurse'] } // Only Doctor and Nurse
}

{
  path: 'schedule',
  loadComponent: () => import('./features/schedule/schedule-grid/schedule-grid')
    .then(m => m.ScheduleGrid),
  canActivate: [authGuard],
  data: { roles: ['Admin', 'HOD', 'Doctor', 'Nurse', 'Technician'] } // All roles
}

{
  path: 'admin/user-management',
  loadComponent: () => import('./components/user-management/user-management.component')
    .then(m => m.UserManagementComponent),
  canActivate: [authGuard],
  data: { roles: ['Admin'] } // Admin only
}
```

### Frontend: Checking Permissions in Component

```typescript
import { AuthService } from '@core/services/auth.service';

export class MyComponent {
  constructor(private authService: AuthService) {}
  
  get canCreateSchedule(): boolean {
    const role = this.authService.getUserRole();
    return ['Admin', 'Doctor', 'Nurse'].includes(role);
  }
  
  get canOnlyViewMonitoring(): boolean {
    const role = this.authService.getUserRole();
    return ['HOD', 'Technician'].includes(role);
  }
  
  get isAdmin(): boolean {
    return this.authService.getUserRole() === 'Admin';
  }
}
```

```html
<!-- In template -->
<button *ngIf="canCreateSchedule" (click)="createSchedule()">
  Create HD Schedule
</button>

<input [readonly]="canOnlyViewMonitoring" [(ngModel)]="vitalSigns">

<button *ngIf="isAdmin" (click)="deleteRecord()">
  Delete
</button>
```

---

## API Testing with cURL

### Login
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"doctor1","password":"Admin@123"}'
```

### Get HD Schedules (with token)
```bash
curl http://localhost:5001/api/hdschedule \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Create HD Schedule (Doctor/Nurse only)
```bash
curl -X POST http://localhost:5001/api/hdschedule \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "patientID": 1,
    "scheduleDate": "2025-11-13",
    "slotID": 1,
    "bedNumber": 5,
    "dialyserType": "HI",
    "duration": 4.0,
    "bloodFlowRate": 300,
    "dialysateFlowRate": 500
  }'
```

### Expected Responses
- **200 OK**: Success
- **401 Unauthorized**: No token or invalid token
- **403 Forbidden**: Valid token but insufficient role permissions

---

## Testing Checklist

### For Each New Endpoint

1. ✅ Add `[Authorize(Roles = "...")]` attribute
2. ✅ Test with each role (Admin, Doctor, Nurse, Technician, HOD)
3. ✅ Verify 403 Forbidden for unauthorized roles
4. ✅ Update frontend route guard if needed
5. ✅ Update permission matrix documentation
6. ✅ Test in browser with actual logins

### For Each New Component

1. ✅ Add route with appropriate `data: { roles: [...] }`
2. ✅ Check if buttons/fields should be hidden/disabled by role
3. ✅ Implement permission checks in component
4. ✅ Test navigation with different user roles
5. ✅ Handle 403 errors gracefully

---

## Test Users

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Admin |
| doctor1 | Admin@123 | Doctor |
| nurse1 | Admin@123 | Nurse |
| tech1 | Admin@123 | Technician |
| hod | Admin@123 | HOD |

---

## Common Patterns

### Read-Only for HOD and Technician
```csharp
// View: All can see
[HttpGet]
[Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]

// Create/Edit: HOD and Technician excluded
[HttpPost]
[Authorize(Roles = "Admin,Doctor,Nurse")]
```

### Technician Can Add Monitoring
```csharp
// Technician's primary function
[HttpPost("{hdLogId}/monitoring")]
[Authorize(Roles = "Admin,Doctor,Nurse,Technician")]
```

### Admin Only Delete
```csharp
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
```

### HOD Management Functions
```csharp
[HttpGet("staff")]
[Authorize(Roles = "Admin,HOD")]

[HttpGet("reports")]
[Authorize(Roles = "Admin,HOD")]
```

---

## Troubleshooting

### 403 Forbidden Error
- Check JWT token contains correct role claim
- Verify `[Authorize(Roles)]` attribute includes the user's role
- Test with Postman/cURL to isolate frontend vs backend

### User Can Access Restricted Route
- Check frontend route guard has correct `data: { roles: [...] }`
- Verify `authGuard` is in `canActivate` array
- Clear browser cache and localStorage
- Re-login to get fresh token

### Token Not Working
- Check token expiration
- Verify Bearer token format: `Authorization: Bearer {token}`
- Ensure token contains role claim
- Check backend JWT configuration in `Program.cs`

---

## Related Files

- `Backend/Controllers/HDScheduleController.cs` - HD schedule endpoints
- `Backend/Controllers/HDLogController.cs` - HD log and monitoring endpoints
- `Backend/Controllers/PatientsController.cs` - Patient endpoints
- `Frontend/hd-scheduler-app/src/app/app.routes.ts` - Route guards
- `Frontend/hd-scheduler-app/src/app/core/guards/auth.guard.ts` - Authentication guard
- `Frontend/hd-scheduler-app/src/app/core/services/auth.service.ts` - Auth service

---

## Need Help?

See detailed documentation:
- `RBAC_IMPLEMENTATION.md` - Complete specification
- `RBAC_TEST_RESULTS.md` - Test results and examples
- `RBAC_COMPLETION_SUMMARY.md` - Implementation summary
