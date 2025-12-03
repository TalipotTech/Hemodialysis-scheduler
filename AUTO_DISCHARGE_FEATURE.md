# Auto-Discharge Feature - Implementation Complete ✅

## Overview
The system now automatically discharges patients **5 hours after their dialysis session starts**. This ensures that sessions don't remain active indefinitely and beds are freed up automatically.

---

## How It Works

### Background Service
- **Service Name:** `SessionCompletionService`
- **Check Interval:** Every 5 minutes
- **Auto-Discharge Time:** 5 hours after `TreatmentStartTime`

### Process Flow

1. **Session Starts**
   - When a dialysis session begins, `TreatmentStartTime` is recorded in `HDSchedule` table
   - Example: Session starts at 08:00 AM

2. **Background Monitoring**
   - Every 5 minutes, the service checks for sessions running longer than 5 hours
   - Example: Service checks at 08:05, 08:10, 08:15... 13:00, 13:05, etc.

3. **Auto-Discharge Trigger**
   - At 13:00 (5 hours after 08:00), the next check will find this session
   - Auto-discharge is triggered at 13:05 (next 5-minute check)

4. **Discharge Actions**
   - `HDSchedule.IsMovedToHistory = 1`
   - `HDSchedule.IsDischarged = 1`
   - `HDSchedule.SessionStatus = 'Discharged'`
   - `HDSchedule.DischargeTime = [calculated 5-hour time]`
   - `BedAssignments.IsActive = 0` (bed released)
   - `BedAssignments.DischargedAt = [current time]`

5. **Logging**
   - Detailed logs written for each auto-discharge
   - Example log:
     ```
     Auto-discharged Patient 12 from Schedule 45 after 5 hours.
     Session started: 2025-11-26 08:00:00, Auto-discharged: 2025-11-26 13:00:00
     ```

---

## Configuration

### Current Settings
- **Auto-Discharge Time:** 5 hours (hardcoded constant)
- **Check Interval:** 5 minutes
- **Enabled:** Always active when backend is running

### To Change Auto-Discharge Time
Edit `Backend/Services/SessionCompletionService.cs`:
```csharp
private const int AUTO_DISCHARGE_HOURS = 5; // Change this value
```

---

## Database Impact

### HDSchedule Table Updates
```sql
UPDATE HDSchedule
SET IsMovedToHistory = 1,
    IsDischarged = 1,
    SessionStatus = 'Discharged',
    DischargeTime = [5 hours after start],
    UpdatedAt = datetime('now')
WHERE TreatmentStartTime IS NOT NULL
  AND datetime('now') >= datetime(TreatmentStartTime, '+5 hours')
  AND IsDischarged = 0
  AND IsMovedToHistory = 0
```

### BedAssignments Table Updates
```sql
UPDATE BedAssignments
SET IsActive = 0,
    DischargedAt = datetime('now')
WHERE PatientID = [patient_id]
  AND SlotID = [slot_id]
  AND BedNumber = [bed_number]
  AND date(AssignmentDate) = date([session_date])
  AND IsActive = 1
```

---

## Monitoring Auto-Discharge

### Check Auto-Discharge Status
**Endpoint:** `GET /api/schedule/auto-discharge-info`

**Authorization:** Admin, HOD, Doctor, Nurse

**Response:**
```json
{
  "success": true,
  "data": {
    "autoDischargeEnabled": true,
    "autoDischargeAfterHours": 5,
    "checkIntervalMinutes": 5,
    "activeSessions": 3,
    "sessions": [
      {
        "scheduleId": 123,
        "patientId": 45,
        "patientName": "John Doe",
        "treatmentStartTime": "2025-11-26 08:00:00",
        "autoDischargeTime": "2025-11-26 13:00:00",
        "hoursRunning": 4.5,
        "hoursUntilAutoDischarge": 0.5,
        "willAutoDischarge": true,
        "bedNumber": 3,
        "slotId": 1,
        "sessionStatus": "Active"
      },
      {
        "scheduleId": 124,
        "patientId": 46,
        "patientName": "Jane Smith",
        "treatmentStartTime": "2025-11-26 09:30:00",
        "autoDischargeTime": "2025-11-26 14:30:00",
        "hoursRunning": 3.0,
        "hoursUntilAutoDischarge": 2.0,
        "willAutoDischarge": true,
        "bedNumber": 5,
        "slotId": 2,
        "sessionStatus": "Active"
      }
    ]
  }
}
```

### Viewing Logs
Check application logs for auto-discharge events:
```
info: HDScheduler.API.Services.SessionCompletionService[0]
      Session Completion Service started (Auto-discharge after 5 hours)

info: HDScheduler.API.Services.SessionCompletionService[0]
      Auto-discharged Patient 12 from Schedule 45 after 5 hours.
      Session started: 2025-11-26 08:00:00, Auto-discharged: 2025-11-26 13:00:00

info: HDScheduler.API.Services.SessionCompletionService[0]
      Auto-discharged 3 patient(s) after 5 hours
```

---

## Workflow Integration

### Normal Dialysis Session (No Auto-Discharge)
1. Session starts at 08:00 → `TreatmentStartTime = 08:00`
2. Nurse/Doctor completes 3-phase workflow
3. Manual discharge at 12:30 (4.5 hours) via "Complete & Discharge"
4. **Auto-discharge never triggers** (already discharged manually)

### Incomplete Session (Auto-Discharge Triggers)
1. Session starts at 08:00 → `TreatmentStartTime = 08:00`
2. Staff forgets to discharge patient
3. At 13:05 (next check after 5 hours), auto-discharge triggers
4. Patient automatically discharged
5. Bed freed for next patient
6. Log entry created

### Emergency/Early Discharge
1. Session starts at 08:00
2. Patient needs early discharge at 10:00 (emergency)
3. Staff uses "Force Discharge" or completes post-dialysis early
4. **Auto-discharge never triggers** (already discharged manually)

---

## Benefits

### 1. **Prevents Stuck Sessions**
- Sessions don't remain active indefinitely
- No manual cleanup required

### 2. **Frees Beds Automatically**
- Beds released 5 hours after start
- Available for next patient booking

### 3. **Consistent Timing**
- Standard 4-hour dialysis + 1-hour buffer = 5 hours
- Predictable discharge timing

### 4. **Audit Trail**
- All auto-discharges logged with timestamps
- Easy to track and review

### 5. **No Manual Intervention**
- Works automatically in background
- Reduces administrative burden

---

## Technical Details

### Service Registration
In `Program.cs`:
```csharp
builder.Services.AddHostedService<SessionCompletionService>();
```

### Service Lifecycle
- **Starts:** When backend application starts
- **Runs:** Continuously until application stops
- **Check Interval:** Every 5 minutes
- **Thread-Safe:** Uses scoped services for database access

### Error Handling
- Each session processed individually
- Errors logged but don't stop other sessions
- Failed auto-discharges retried on next check (5 minutes)

---

## Testing

### Test Scenario 1: Normal Auto-Discharge
```sql
-- 1. Create a test session that started 6 hours ago
UPDATE HDSchedule
SET TreatmentStartTime = datetime('now', '-6 hours'),
    IsDischarged = 0,
    IsMovedToHistory = 0
WHERE ScheduleID = [test_schedule_id];

-- 2. Wait 5 minutes for next check
-- 3. Verify auto-discharge occurred
SELECT ScheduleID, IsDischarged, IsMovedToHistory, DischargeTime
FROM HDSchedule
WHERE ScheduleID = [test_schedule_id];
```

### Test Scenario 2: Check Active Sessions
```bash
# Call the monitoring endpoint
curl -X GET "http://localhost:5001/api/schedule/auto-discharge-info" \
  -H "Authorization: Bearer [your_jwt_token]"
```

### Test Scenario 3: Verify Logs
```bash
# Watch backend console for auto-discharge messages
# Look for: "Auto-discharged Patient X from Schedule Y after 5 hours"
```

---

## FAQ

### Q: What if I want to change the auto-discharge time?
**A:** Edit `SessionCompletionService.cs` and change `AUTO_DISCHARGE_HOURS = 5` to your desired value (e.g., 4 or 6).

### Q: Can I disable auto-discharge?
**A:** Yes, comment out or remove this line in `Program.cs`:
```csharp
// builder.Services.AddHostedService<SessionCompletionService>();
```

### Q: What happens if a session is manually discharged before 5 hours?
**A:** Auto-discharge won't trigger because the query checks for `IsDischarged = 0` and `IsMovedToHistory = 0`.

### Q: What if `TreatmentStartTime` is null?
**A:** Sessions without a start time are ignored by the auto-discharge process.

### Q: Can I see which sessions will be auto-discharged?
**A:** Yes, call `GET /api/schedule/auto-discharge-info` to see all active sessions and their auto-discharge times.

### Q: What happens to the bed when auto-discharge occurs?
**A:** The bed is released (`IsActive = 0` in BedAssignments) and becomes available for new patients.

### Q: Is there a grace period?
**A:** The check runs every 5 minutes, so auto-discharge can occur up to 5 minutes after the 5-hour mark (e.g., 5:00 to 5:05 hours).

---

## Verification Steps

### 1. Check Service is Running
```bash
# Look for this log on backend startup:
"Session Completion Service started (Auto-discharge after 5 hours)"
```

### 2. Create Test Session
```sql
-- Insert a test session that started 6 hours ago
INSERT INTO HDSchedule (
    PatientID, SessionDate, TreatmentStartTime, 
    IsDischarged, IsMovedToHistory, SessionStatus
) VALUES (
    1, date('now'), datetime('now', '-6 hours'),
    0, 0, 'Active'
);
```

### 3. Wait for Next Check
- Wait 5 minutes for the next background check
- Check logs for auto-discharge message

### 4. Verify Database
```sql
-- Check if session was auto-discharged
SELECT ScheduleID, IsDischarged, IsMovedToHistory, 
       DischargeTime, SessionStatus
FROM HDSchedule
WHERE TreatmentStartTime < datetime('now', '-5 hours')
  AND IsDischarged = 1;
```

---

## Implementation Files

| File | Purpose |
|------|---------|
| `Backend/Services/SessionCompletionService.cs` | Main auto-discharge logic |
| `Backend/Controllers/ScheduleController.cs` | Monitoring endpoint |
| `Backend/Program.cs` | Service registration |

---

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2025-11-26 | 1.0 | Initial implementation - Auto-discharge after 5 hours |

---

## Support

For issues or questions about auto-discharge:
1. Check application logs for errors
2. Verify service is running in backend console
3. Use the monitoring endpoint to check session status
4. Review this documentation

---

**Feature Status:** ✅ **ACTIVE AND RUNNING**

*The auto-discharge feature is now live and will automatically discharge patients 5 hours after their session starts.*
