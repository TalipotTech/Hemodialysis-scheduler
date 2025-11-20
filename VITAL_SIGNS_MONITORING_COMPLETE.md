# Intra-Dialytic Vital Signs Monitoring - Complete Implementation

## Overview
The HD Scheduler now has a **complete vital signs monitoring system** that permanently saves all patient treatment data to their medical history.

## How It Works

### 1. **Recording Vital Signs During Treatment**
When a patient is undergoing dialysis treatment, medical staff can:

1. Navigate to the patient's active session
2. Click **"Record Vitals"** button
3. Fill in the monitoring form with:
   - **Blood Pressure** (e.g., 120/80)
   - **Pulse Rate** (40-200 bpm)
   - **Temperature** (35-42°C)
   - **UF Volume** (Ultrafiltration volume in liters)
   - **Blood Flow Rate** (optional, 50-600 mL/min)
   - **Venous Pressure** (optional)
   - **Arterial Pressure** (optional)
   - **TMP Pressure** (optional)
   - **Symptoms** (optional notes)
   - **Interventions** (optional notes)

4. Click **"Record Vitals"** to save

### 2. **Data Storage**
- All vital signs are saved to the **IntraDialyticRecord** table in the database
- Each record is linked to the specific HD session (**HDLog**)
- Records include **timestamp** for when vitals were recorded
- Data is **permanently stored** and accessible in patient history

### 3. **Viewing Patient History**
Medical staff can view complete treatment history:

1. Go to **Patient History** page
2. Click on any previous session
3. View **all vital signs** recorded during that treatment
4. Review trends over multiple sessions
5. Check symptoms and interventions from past treatments

## Access Routes

### New Monitoring Page
```
URL: /patients/:patientId/monitoring/:scheduleId
Access: Admin, Doctor, Nurse, Technician
Purpose: Record vital signs during active treatment
```

### Session Details Page
```
URL: /patients/:patientId/session/:scheduleId
Access: All roles (Admin, HOD, Doctor, Nurse, Technician)
Purpose: View complete session details including all vital signs
```

## Features

### ✅ Multiple Recordings Per Session
- Staff can record vitals **multiple times** during one treatment session
- Each recording has its own timestamp
- Typical workflow: Record vitals every 30-60 minutes during 4-hour treatment

### ✅ Permanent Storage
- All data saved to database immediately
- No data loss even if browser closes
- Full audit trail with timestamps

### ✅ Historical Access
- View any patient's previous sessions
- Compare vital signs across multiple treatments
- Track patient progress over time
- Identify patterns and trends

### ✅ Role-Based Access
- **Technicians** can record vitals during treatment
- **Nurses & Doctors** can record and delete vitals
- **HOD** can view all data (read-only)
- **All staff** can view patient history

## Backend API Endpoints

### Get Monitoring Records
```
GET /api/HDLog/{hdLogId}/monitoring
Returns: Array of all vital sign records for a session
```

### Add Monitoring Record
```
POST /api/HDLog/{hdLogId}/monitoring
Body: { bloodPressure, pulseRate, temperature, ufVolume, ... }
Returns: Record ID
```

### Delete Monitoring Record
```
DELETE /api/HDLog/monitoring/{monitoringId}
Access: Admin, Doctor, Nurse only
```

## Database Schema

### IntraDialyticRecord Table
- **MonitoringID** (Primary Key)
- **HDLogID** (Foreign Key to HDLog)
- **TimeRecorded** (DateTime - when vitals were taken)
- **BloodPressure** (varchar)
- **PulseRate** (decimal)
- **Temperature** (decimal)
- **UFVolume** (decimal)
- **ActualBFR** (decimal - Blood Flow Rate)
- **VenousPressure** (decimal)
- **ArterialPressure** (decimal)
- **TMPPressure** (decimal)
- **Symptoms** (varchar)
- **Interventions** (varchar)

## Benefits

### For Medical Staff
1. **Easy data entry** - Simple form with validation
2. **Real-time recording** - Save vitals immediately during treatment
3. **Quick access** - View current and past vitals quickly
4. **Better monitoring** - Track patient status during dialysis

### For Patient Care
1. **Complete medical records** - All vitals saved permanently
2. **Treatment continuity** - Next session can reference previous data
3. **Trend analysis** - Identify patterns over multiple sessions
4. **Quality improvement** - Evidence-based treatment adjustments

### For Compliance
1. **Full audit trail** - Who recorded what and when
2. **Regulatory compliance** - Complete documentation
3. **Quality assurance** - Verify treatment protocols followed
4. **Medical-legal protection** - Comprehensive records

## Example Workflow

### Typical Treatment Day

**8:00 AM** - Patient arrives for dialysis
- Nurse schedules session, assigns bed
- Pre-treatment vitals recorded

**8:30 AM** - Treatment begins
- Initial vitals: BP 130/85, Pulse 78, Temp 36.8°C, UF 0L

**9:30 AM** - First hour check
- Vitals: BP 125/80, Pulse 76, UF 0.8L
- Note: "Patient comfortable, no complaints"

**10:30 AM** - Mid-treatment
- Vitals: BP 120/78, Pulse 74, UF 1.6L

**11:30 AM** - Near completion
- Vitals: BP 118/75, Pulse 72, UF 2.3L

**12:00 PM** - Treatment complete
- Final vitals: BP 115/75, Pulse 70, UF 2.5L (goal achieved)
- Post-treatment medications recorded

**Next Visit** - Staff can review all previous vitals to compare and adjust treatment plan

## Navigation Paths

From **Schedule Grid** → Click patient → **Session Details** → Click **"Record Vitals"** → **Monitoring Page**

From **Patient History** → Click session → **Session Details** → View all recorded vitals

From **Nurse Dashboard** → Today's Sessions → Click patient → **Monitoring Page**

## Success Indicators

✅ Vitals saved to database with timestamp
✅ Data appears in patient history
✅ Can record multiple times per session
✅ Previous sessions show historical data
✅ All roles can view history appropriately
✅ Only authorized roles can add/edit data

---

**Implementation Date:** November 19, 2025
**Status:** ✅ COMPLETE AND READY FOR USE
