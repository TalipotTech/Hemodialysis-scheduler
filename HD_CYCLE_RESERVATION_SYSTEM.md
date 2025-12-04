# HD Cycle-Based Bed Reservation System - Complete Implementation

## Overview
This implementation adds automatic bed scheduling based on patient HD cycles. The system can now:
- Track reserved patients (future scheduled sessions based on HD cycle)
- Track active patients (currently undergoing treatment)
- Automatically schedule next dialysis sessions when a patient is discharged
- Display reservation statistics on the schedule grid

---

## Features Implemented

### 1. **HD Cycle Calculation Service** (`Backend/Services/HDCycleService.cs`)
Calculates next dialysis dates based on various HD cycle patterns:
- **Every X days** (e.g., "Every 2 days", "Every 3 days")
- **Sessions per week** (e.g., "3x/week", "2x/week")
- **Specific day patterns** (e.g., "MWF" - Monday/Wednesday/Friday, "TTS" - Tuesday/Thursday/Saturday)
- **Daily cycles** ("Daily", "Everyday")
- **Alternate days** ("Alternate", "Every other day")

**Key Methods:**
- `CalculateNextDialysisDate()` - Returns next dialysis date
- `GetUpcomingDialysisDates()` - Returns multiple future dates
- `ShouldHaveDialysisOnDate()` - Checks if patient should have dialysis on specific date
- `GetDaysBetweenSessions()` - Returns interval between sessions

---

### 2. **Reservation Management API** (`Backend/Controllers/ReservationController.cs`)

#### Endpoints:

**GET `/api/reservation/statistics?date=YYYY-MM-DD`**
- Returns reservation statistics for a specific date
- Shows counts of active vs reserved patients
- Groups patients by HD cycle type
- Lists all active and reserved patients with details

**Response Example:**
```json
{
  "success": true,
  "data": {
    "date": "2025-12-04",
    "summary": {
      "totalActive": 5,
      "totalReserved": 12,
      "totalPatients": 17,
      "activeSessions": 5,
      "futureSessions": 15
    },
    "byCycle": [
      {
        "hdCycle": "Every 2 days",
        "activeCount": 3,
        "reservedCount": 8
      }
    ],
    "activePatients": [...],
    "reservedPatients": [...]
  }
}
```

**POST `/api/reservation/auto-schedule-next/{scheduleId}`**
- Auto-schedules next session for a patient based on their HD cycle
- Creates a "Pre-Scheduled" session with same slot/bed
- Returns details of newly created session

**POST `/api/reservation/generate-schedule/{patientId}?daysAhead=30`**
- Generates multiple future sessions for a patient
- Useful for bulk scheduling based on HD cycle
- Optional parameters: `slotId`, `bedNumber`

**GET `/api/reservation/patients-status?date=YYYY-MM-DD`**
- Returns all patients with their reservation status (Active/Reserved/Inactive)
- Shows next scheduled date and next expected date based on HD cycle

---

### 3. **Enhanced Schedule Controller**

#### Updated `/api/schedule/daily` Endpoint
Now includes reservation statistics in response:
```json
{
  "date": "2025-12-04",
  "slots": [...],
  "statistics": {
    "totalActivePatients": 5,
    "totalReservedPatients": 12,
    "activeSessionsToday": 4,
    "preScheduledSessionsToday": 1,
    "totalSessionsToday": 5,
    "futureSessionsCount": 15
  }
}
```

#### Updated `/api/hdschedule/{id}/discharge` Endpoint
- Now includes `autoScheduleNext` parameter (default: true)
- Automatically creates next session when patient is discharged
- Returns info about next scheduled session

**Usage:**
```
PUT /api/hdschedule/93/discharge?autoScheduleNext=true
```

**Response:**
```json
{
  "success": true,
  "data": {
    "discharged": true,
    "message": "Patient discharged successfully",
    "nextSession": {
      "scheduled": true,
      "scheduleId": 150,
      "sessionDate": "2025-12-06",
      "hdCycle": "Every 2 days",
      "message": "Next session automatically scheduled for Dec 06, 2025"
    }
  }
}
```

---

### 4. **Frontend Reservation Statistics Display**

#### Updated Schedule Grid (`schedule-grid.html`)
Added a new statistics row showing HD cycle-based metrics:

**New Statistics Cards:**
1. **Active Patients (Today)** - Patients currently undergoing dialysis
2. **Reserved Patients (Future)** - Patients with future sessions scheduled
3. **Active / Pre-Scheduled** - Today's session breakdown
4. **Future Sessions** - Total upcoming scheduled sessions

**Visual Design:**
- Color-coded cards with icons
- Hover effects for better UX
- Responsive grid layout
- Gradient backgrounds matching theme

---

### 5. **Frontend Reservation Service** (`reservation.service.ts`)

New Angular service for reservation management:

```typescript
// Get reservation statistics
reservationService.getReservationStatistics(date?)

// Auto-schedule next session
reservationService.autoScheduleNextSession(scheduleId)

// Generate multiple future sessions
reservationService.generateScheduleForPatient(patientId, daysAhead, slotId?, bedNumber?)

// Get all patients with status
reservationService.getPatientsWithReservationStatus(date?)
```

---

## How It Works

### Patient HD Cycle Flow

1. **Patient Registration:**
   - Patient is registered with HD Cycle (e.g., "Every 2 days")
   - HD Start Date is recorded

2. **First Session:**
   - Session is scheduled manually
   - Patient undergoes dialysis

3. **Discharge:**
   - Nurse/Doctor marks session as discharged
   - System automatically calculates next date based on HD cycle
   - System creates "Pre-Scheduled" session for next date
   - Next session maintains same slot/bed if available

4. **Future Sessions:**
   - Pre-scheduled sessions appear in purple on schedule grid
   - When session date arrives, it becomes "Active" (red)
   - After completion, next session is auto-scheduled

### Reservation Status Logic

- **Active:** Patient has a session TODAY (SessionDate = Today)
- **Reserved:** Patient has future sessions (SessionDate > Today)
- **Inactive:** Patient has no upcoming sessions

---

## Visual Indicators on Schedule Grid

### Bed Colors:
- **Green** - Available beds (empty, ready for assignment)
- **Red** - Occupied (active session today)
- **Purple** - Pre-Scheduled (future sessions or reserved based on HD cycle)

### Statistics Display:
- **First Row** - Basic bed occupancy metrics
- **Second Row** - HD cycle-based reservation statistics

---

## Database Schema

### Existing Tables (No Changes Required)

**Patients Table:**
- `HDCycle` (TEXT) - Stores HD cycle pattern
- `HDStartDate` (DATE) - When HD treatment started
- `HDFrequency` (INT) - Sessions per week

**HDSchedule Table:**
- `SessionStatus` (TEXT) - "Pre-Scheduled", "Active", "Completed", etc.
- `IsAutoGenerated` (BOOLEAN) - Marks auto-generated sessions
- `ParentScheduleID` (INT) - Links to original session

---

## API Examples

### 1. Get Today's Reservation Statistics
```http
GET /api/reservation/statistics?date=2025-12-04
Authorization: Bearer {token}
```

### 2. Discharge Patient and Auto-Schedule Next
```http
PUT /api/hdschedule/93/discharge?autoScheduleNext=true
Authorization: Bearer {token}
```

### 3. Generate 30 Days of Sessions for Patient
```http
POST /api/reservation/generate-schedule/5?daysAhead=30&slotId=1&bedNumber=4
Authorization: Bearer {token}
```

### 4. Get All Patients with Reservation Status
```http
GET /api/reservation/patients-status?date=2025-12-04
Authorization: Bearer {token}
```

---

## Usage Instructions

### For Staff (Frontend)

1. **View Reservation Statistics:**
   - Open Schedule Grid
   - See two rows of statistics at top
   - First row: Basic bed occupancy
   - Second row: HD cycle-based reservations

2. **Understand Patient Status:**
   - **Active Patients:** Currently in center (red beds)
   - **Reserved Patients:** Scheduled for future (purple beds)
   - View counts in statistics cards

3. **Discharge Patient:**
   - Click on occupied bed (red)
   - Navigate to session details
   - Click "Discharge" button
   - System automatically schedules next session based on HD cycle
   - Confirmation shows next session date

4. **View Future Sessions:**
   - Click "Bed Schedule (Future Sessions)" tab
   - See all upcoming sessions
   - Filter by HD cycle, patient, date

### For Administrators

1. **Bulk Schedule Generation:**
   - Use API to generate sessions for all patients
   - Specify time range (e.g., 30 days ahead)
   - System respects HD cycles and prevents duplicates

2. **Monitor Reservations:**
   - Check reservation statistics daily
   - Ensure patients have future sessions scheduled
   - Identify patients without upcoming appointments

---

## Benefits

### For Patients:
✅ Automatic appointment scheduling  
✅ Consistent treatment schedule based on medical requirements  
✅ No manual booking needed for recurring sessions  
✅ Reduced chance of missed appointments  

### For Staff:
✅ Clear visibility of reserved vs active patients  
✅ Reduced administrative workload  
✅ Better capacity planning  
✅ Automatic scheduling based on medical protocols  

### For Center Management:
✅ Improved resource utilization  
✅ Better forecasting of bed occupancy  
✅ Data-driven insights on patient flow  
✅ Automated workflow reduces errors  

---

## Configuration

### HD Cycle Formats Supported:
- `"Every 2 days"`, `"Every 3 days"`, etc.
- `"3x/week"`, `"2x/week"`, `"3 times per week"`
- `"MWF"` (Monday, Wednesday, Friday)
- `"TTS"` (Tuesday, Thursday, Saturday)
- `"Daily"`, `"Everyday"`
- `"Alternate"`, `"Every other day"`

### System Settings (appsettings.json):
```json
{
  "AutoScheduling": {
    "Enabled": true,
    "DaysAheadDefault": 30,
    "PreserveSlotAndBed": true
  }
}
```

---

## Testing

### Test Scenarios:

1. **Create Patient with HD Cycle "Every 2 days"**
   - Schedule first session for Dec 4, 2025
   - Discharge patient
   - Verify next session created for Dec 6, 2025

2. **Verify Statistics Update**
   - Check reservation statistics API
   - Verify active count increases on session date
   - Verify reserved count includes future sessions

3. **Test Bulk Generation**
   - Generate 30 days of sessions for patient
   - Verify no duplicate sessions created
   - Verify all sessions follow HD cycle pattern

4. **UI Verification**
   - Purple beds show for future sessions
   - Red beds show for today's sessions
   - Statistics cards display correct counts

---

## Files Modified/Created

### Backend:
- ✅ `Backend/Services/HDCycleService.cs` (NEW)
- ✅ `Backend/Controllers/ReservationController.cs` (NEW)
- ✅ `Backend/Controllers/ScheduleController.cs` (UPDATED)
- ✅ `Backend/Controllers/HDScheduleController.cs` (UPDATED)
- ✅ `Backend/Repositories/IHDScheduleRepository.cs` (UPDATED)
- ✅ `Backend/Repositories/HDScheduleRepository.cs` (UPDATED)
- ✅ `Backend/Program.cs` (UPDATED)

### Frontend:
- ✅ `Frontend/.../reservation.service.ts` (NEW)
- ✅ `Frontend/.../schedule-grid.html` (UPDATED)
- ✅ `Frontend/.../schedule-grid.ts` (NO CHANGES NEEDED)
- ✅ `Frontend/.../schedule-grid.scss` (UPDATED)

---

## Next Steps (Optional Enhancements)

1. **Email/SMS Notifications**
   - Send reminders for upcoming sessions
   - Notify when next session is auto-scheduled

2. **Bed Assignment Optimization**
   - Suggest best bed based on patient preference/history
   - Load balance across slots

3. **HD Cycle Analytics**
   - Track adherence to HD cycle
   - Alert for missed sessions

4. **Patient Portal**
   - Allow patients to view their schedule
   - Request slot/bed changes

---

## Support

For questions or issues:
1. Check API responses for detailed error messages
2. Review browser console for frontend errors
3. Check backend logs for auto-scheduling issues
4. Verify patient has HD Cycle and HD Start Date configured

---

**Implementation Date:** December 4, 2025  
**Version:** 1.0  
**Status:** ✅ Complete and Ready to Use
