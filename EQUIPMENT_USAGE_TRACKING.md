# Equipment Usage Tracking & Alert System

## Overview
This feature implements automated tracking and alerting for dialysis equipment usage to ensure patient safety. The system monitors **Dialyser** and **Blood Tubing** reuse counts and generates alerts when equipment approaches or exceeds safe usage limits.

## Medical Safety Limits

### Dialyser
- **Maximum Reuse**: 7 times
- **Warning Threshold**: 6 uses (85% of limit)
- **Purpose**: Dialysers lose efficiency after multiple uses and can become unsafe

### Blood Tubing
- **Maximum Reuse**: 12 times
- **Warning Threshold**: 10 uses (83% of limit)
- **Purpose**: Blood tubing can develop micro-cracks and contamination risks with overuse

## Features

### 1. Real-Time Usage Tracking
- Tracks current usage count for each patient's equipment
- Displays usage percentage and remaining uses
- Visual progress bars with color-coded warnings

### 2. Automatic Alert Generation
The system generates alerts at three levels:

#### **Warning** (Yellow)
- Dialyser: 5-6 uses
- Blood Tubing: 9-10 uses
- Message: "NOTICE: Equipment usage at X/Max. Y use(s) remaining."

#### **Critical** (Orange)
- Dialyser: 6 uses
- Blood Tubing: 10 uses
- Message: "WARNING: Equipment is nearing maximum usage. Please prepare replacement."

#### **Expired** (Red)
- Dialyser: 7+ uses
- Blood Tubing: 12+ uses
- Message: "CRITICAL: Equipment has reached/exceeded maximum usage limit. MUST be replaced!"

### 3. UI Components

#### Equipment Usage Alert Component
Located in: `Frontend/hd-scheduler-app/src/app/shared/components/equipment-usage-alert/`

Features:
- Visual status indicators with icons
- Color-coded progress bars
- Usage counters (current/max)
- Alert messages with severity levels
- Blinking animations for critical alerts
- Replacement warnings

#### Integration Points
- **HD Session Form**: Shows alerts during session creation/editing
- **Session Schedule View**: Displays equipment status for active sessions
- **Patient History**: Historical equipment usage tracking

## Backend Implementation

### Models
**File**: `Backend/Models/EquipmentUsageAlert.cs`

```csharp
public class EquipmentUsageLimits
{
    public const int DIALYSER_MAX_REUSE = 7;
    public const int BLOOD_TUBING_MAX_REUSE = 12;
    public const int DIALYSER_WARNING_THRESHOLD = 6;
    public const int BLOOD_TUBING_WARNING_THRESHOLD = 10;
}

public class EquipmentUsageAlert
{
    public int AlertID { get; set; }
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public string EquipmentType { get; set; } // "Dialyser" or "Blood Tubing"
    public int CurrentUsageCount { get; set; }
    public int MaxUsageLimit { get; set; }
    public string Severity { get; set; } // "Warning", "Critical", "Expired"
    public string AlertMessage { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Service
**File**: `Backend/Services/EquipmentUsageService.cs`

Key Methods:
- `CheckEquipmentUsageAsync()` - Check current usage and generate status
- `LogEquipmentAlertAsync()` - Save alert to database
- `GetUnacknowledgedAlertsAsync()` - Retrieve active alerts for a patient
- `AcknowledgeAlertAsync()` - Mark alert as seen by staff
- `CheckAndCreateAlertsForScheduleAsync()` - Automatically generate alerts for a session

### API Endpoints
**Controller**: `Backend/Controllers/HDScheduleController.cs`

```
GET    /api/HDSchedule/patient/{patientId}/equipment-status
GET    /api/HDSchedule/{scheduleId}/equipment-status
GET    /api/HDSchedule/patient/{patientId}/equipment-alerts
POST   /api/HDSchedule/{scheduleId}/check-equipment-alerts
PUT    /api/HDSchedule/equipment-alerts/{alertId}/acknowledge
```

### Database
**Table**: `EquipmentUsageAlerts`

Schema:
```sql
CREATE TABLE EquipmentUsageAlerts (
    AlertID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    ScheduleID INTEGER,
    EquipmentType TEXT NOT NULL,
    CurrentUsageCount INTEGER NOT NULL,
    MaxUsageLimit INTEGER NOT NULL,
    Severity TEXT NOT NULL,
    AlertMessage TEXT NOT NULL,
    IsAcknowledged INTEGER NOT NULL DEFAULT 0,
    AcknowledgedBy TEXT,
    AcknowledgedAt TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedule(ScheduleID)
);
```

## Usage Workflow

### For Nurses/Doctors

1. **During Session Creation**:
   - Enter Dialyser Reuse Number and Blood Tubing Reuse Number
   - System automatically shows usage status with color-coded alerts
   - If equipment is at limit, clear warning message displays

2. **Alert Response**:
   - **Warning/Critical**: Prepare replacement equipment for next session
   - **Expired**: Inform patient immediately to bring new equipment
   - Do NOT proceed with session if equipment has exceeded limits

3. **Alert Acknowledgment**:
   - Review alert details
   - Acknowledge alert after taking appropriate action
   - Document any patient communication

### For Administrators

1. **Monitoring**:
   - View all unacknowledged alerts
   - Track equipment replacement patterns
   - Generate reports on equipment usage trends

2. **Audit Trail**:
   - All alerts are logged with timestamps
   - Acknowledgments track who addressed each alert
   - Historical data helps identify patients who frequently exceed limits

## Safety Considerations

### Critical Actions Required

1. **At Maximum Limit**:
   - ⛔ DO NOT use equipment that has reached maximum reuse count
   - 📞 Contact patient immediately to arrange replacement
   - 📋 Document in patient notes

2. **Approaching Limit**:
   - ⚠️ Notify patient during current session
   - 📅 Schedule equipment replacement before next session
   - ✅ Verify new equipment before next treatment

3. **Patient Education**:
   - Explain importance of timely equipment replacement
   - Provide written guidelines for equipment care
   - Emphasize safety risks of overused equipment

## Configuration

### Modifying Usage Limits

To change equipment limits, update constants in:
- **Backend**: `Backend/Models/EquipmentUsageAlert.cs`
- **Frontend**: `Frontend/hd-scheduler-app/src/app/shared/components/equipment-usage-alert/equipment-usage-alert.component.ts`

```typescript
readonly DIALYSER_MAX = 7;
readonly BLOOD_TUBING_MAX = 12;
readonly DIALYSER_WARNING_THRESHOLD = 6;
readonly BLOOD_TUBING_WARNING_THRESHOLD = 10;
```

## Testing

### Test Scenarios

1. **Normal Usage** (0-4 uses):
   - ✓ Green status
   - ✓ "Usage is normal" message

2. **Warning Level** (5 uses for Dialyser, 9 for Blood Tubing):
   - ⚠️ Yellow status
   - ⚠️ Notice message

3. **Critical Level** (6 uses for Dialyser, 10 for Blood Tubing):
   - 🔶 Orange status
   - 🔶 Warning message with replacement notice

4. **Expired** (7+ uses for Dialyser, 12+ for Blood Tubing):
   - 🔴 Red status
   - 🔴 Critical alert with blinking animation
   - ⛔ Replacement required banner

### Manual Testing Steps

1. Create a new HD session
2. Set Dialyser Reuse Number to different values (0, 5, 6, 7, 8)
3. Verify alert colors and messages change appropriately
4. Set Blood Tubing Reuse Number to different values (0, 9, 10, 12, 13)
5. Verify both alerts display correctly simultaneously
6. Check that critical alerts show blinking animation

## Troubleshooting

### Common Issues

**Issue**: Alerts not displaying
- Check that form fields `dialyserReuseNumber` and `bloodTubingReuse` have values
- Verify component is imported correctly
- Check console for JavaScript errors

**Issue**: Wrong alert levels
- Verify usage counts are correct
- Check threshold constants match backend limits
- Ensure calculation logic matches business rules

**Issue**: Database errors
- Run migration script: `Database/11_AddEquipmentUsageAlerts.sql`
- Verify `EquipmentUsageAlerts` table exists
- Check foreign key constraints are valid

## Future Enhancements

### Planned Features
1. **SMS/Email Notifications**: Automatic alerts to patients when equipment nearing limit
2. **Equipment Inventory Integration**: Track available replacement equipment
3. **Predictive Analytics**: Estimate when patients will need replacements
4. **Batch Alerts**: Daily report of all patients with equipment warnings
5. **Mobile App Integration**: Push notifications to staff and patients
6. **Equipment Lifecycle Tracking**: Track when equipment was purchased/issued

## API Examples

### Check Equipment Status
```bash
GET /api/HDSchedule/patient/123/equipment-status
Authorization: Bearer <token>

Response:
{
  "success": true,
  "data": [
    {
      "equipmentType": "Dialyser",
      "currentUsageCount": 6,
      "maxUsageLimit": 7,
      "remainingUses": 1,
      "usagePercentage": 85.7,
      "status": "Critical",
      "message": "⚠️ WARNING: Dialyser is nearing maximum usage...",
      "requiresReplacement": false
    },
    {
      "equipmentType": "Blood Tubing",
      "currentUsageCount": 10,
      "maxUsageLimit": 12,
      "remainingUses": 2,
      "usagePercentage": 83.3,
      "status": "Critical",
      "message": "⚠️ WARNING: Blood Tubing is nearing maximum usage...",
      "requiresReplacement": false
    }
  ]
}
```

### Create Alerts for Session
```bash
POST /api/HDSchedule/456/check-equipment-alerts
Authorization: Bearer <token>

Response:
{
  "success": true,
  "data": [
    {
      "alertID": 789,
      "patientID": 123,
      "scheduleID": 456,
      "equipmentType": "Dialyser",
      "currentUsageCount": 7,
      "maxUsageLimit": 7,
      "severity": "Expired",
      "alertMessage": "⚠️ CRITICAL: Dialyser has reached maximum usage limit...",
      "isAcknowledged": false,
      "createdAt": "2025-11-17T10:30:00Z"
    }
  ],
  "message": "Equipment usage alerts generated"
}
```

## Support

For issues or questions about the Equipment Usage Tracking system:
1. Check this documentation
2. Review code comments in implementation files
3. Contact development team
4. Submit issue in project repository

---

**Last Updated**: November 17, 2025
**Version**: 1.0
**Author**: HD Scheduler Development Team
