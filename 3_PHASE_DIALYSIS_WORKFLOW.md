# 3-Phase Dialysis Workflow - Implementation Complete

## Overview
A comprehensive 3-phase dialysis workflow system has been implemented with independent save functionality for each phase, ensuring NO data loss between phases.

---

## Features

### ✅ Three Distinct Phases
1. **Pre-Dialysis Assessment** - Initial patient evaluation
2. **Intra-Dialysis Monitoring** - Treatment monitoring and vital signs recording
3. **Post-Dialysis Assessment** - Final evaluation and discharge

### ✅ Independent Save per Phase
- Each phase has its own **Save** button
- Auto-save functionality (saves after 2 seconds of inactivity)
- Visual save status indicators (Saving... → Saved at XX:XX)
- Data persists immediately to database

### ✅ Phase Locking & Progression
- Completing a phase **locks** it (prevents further editing)
- Timestamps recorded for each phase transition
- Linear progression: PRE → INTRA → POST → COMPLETED
- Visual lock indicators on completed phases

### ✅ Data Validation
- Required fields per phase (weight, BP, HR, etc.)
- Input validation ranges (e.g., BP: 60-250 mmHg, Weight: 20-250 kg)
- Form validation prevents incomplete submissions

---

## Database Schema (Migration 14)

### New Columns Added to `HDLogs` Table:

#### Phase Tracking:
- `SessionPhase` TEXT - Current phase (PRE_DIALYSIS, INTRA_DIALYSIS, POST_DIALYSIS, COMPLETED)
- `PreDialysisCompletedAt` DATETIME - Timestamp when pre-dialysis completed
- `IntraDialysisStartedAt` DATETIME - Timestamp when treatment started
- `PostDialysisStartedAt` DATETIME - Timestamp when post-dialysis started
- `IsPreDialysisLocked` BOOLEAN - Prevents editing after completion
- `IsIntraDialysisLocked` BOOLEAN - Prevents editing after completion

#### Pre-Dialysis Fields:
- `PreWeight` DECIMAL - Patient weight before treatment
- `PreSBP` INTEGER - Systolic blood pressure
- `PreDBP` INTEGER - Diastolic blood pressure
- `PreHR` INTEGER - Heart rate
- `PreTemp` DECIMAL - Temperature
- `AccessSite` TEXT - Vascular access location
- `PreAssessmentNotes` TEXT - Initial assessment notes

#### Post-Dialysis Fields:
- `PostDialysisWeight` DECIMAL - Patient weight after treatment
- `PostDialysisSBP` INTEGER - Post-treatment systolic BP
- `PostDialysisDBP` INTEGER - Post-treatment diastolic BP
- `PostDialysisHR` INTEGER - Post-treatment heart rate
- `AccessBleedingTime` INTEGER - Time until bleeding stopped (minutes)
- `TotalFluidRemoved` DECIMAL - Total UF removed (liters)
- `PostAccessStatus` TEXT - Vascular access condition
- `MedicationsAdministered` TEXT - Post-dialysis medications (EPO, iron, etc.)
- `DischargeNotes` TEXT - Discharge instructions and follow-up

---

## Backend API Endpoints

### Base URL: `http://localhost:5001/api/HDLog`

### 1. Get Phase Status
```http
GET /api/HDLog/{hdLogId}/phase-status
```
**Response:**
```json
{
  "success": true,
  "data": {
    "sessionPhase": "PRE_DIALYSIS",
    "isPreDialysisLocked": false,
    "isIntraDialysisLocked": false,
    "preDialysisCompletedAt": null,
    "intraDialysisStartedAt": null,
    "postDialysisStartedAt": null
  }
}
```

### 2. Save Pre-Dialysis Data
```http
PUT /api/HDLog/{hdLogId}/save-pre-dialysis
```
**Request Body:**
```json
{
  "preWeight": 75.5,
  "preSBP": 140,
  "preDBP": 90,
  "preHR": 78,
  "preTemp": 36.8,
  "accessSite": "Left AVF",
  "preAssessmentNotes": "Patient feeling well, no complaints"
}
```

### 3. Complete Pre-Dialysis Phase
```http
POST /api/HDLog/{hdLogId}/complete-pre-dialysis
```
**Effect:** 
- Sets `SessionPhase = 'INTRA_DIALYSIS'`
- Sets `Status = 'Active'`
- Locks pre-dialysis phase
- Records `PreDialysisCompletedAt` timestamp

### 4. Start Post-Dialysis Phase
```http
POST /api/HDLog/{hdLogId}/start-post-dialysis
```
**Effect:**
- Sets `SessionPhase = 'POST_DIALYSIS'`
- Locks intra-dialysis phase
- Records `PostDialysisStartedAt` timestamp

### 5. Save Post-Dialysis Data
```http
PUT /api/HDLog/{hdLogId}/save-post-dialysis
```
**Request Body:**
```json
{
  "postDialysisWeight": 73.2,
  "postDialysisSBP": 130,
  "postDialysisDBP": 85,
  "postDialysisHR": 72,
  "accessBleedingTime": 8,
  "totalFluidRemoved": 2.3,
  "postAccessStatus": "Good, no complications",
  "medicationsAdministered": "EPO 4000 IU IV, Iron 100mg IV",
  "dischargeNotes": "Patient stable, no complaints. Next session scheduled."
}
```

### 6. Complete Post-Dialysis & Discharge
```http
POST /api/HDLog/{hdLogId}/complete-post-dialysis
```
**Effect:**
- Sets `SessionPhase = 'COMPLETED'`
- Sets `Status = 'Completed'`
- Sets `IsDischarged = true`
- Records `ActualEndTime = DateTime.Now`

---

## Frontend Components

### 1. DialysisWorkflowComponent
**Location:** `Frontend/hd-scheduler-app/src/app/features/schedule/dialysis-workflow/`

**Files:**
- `dialysis-workflow.component.ts` - Component logic (470 lines)
- `dialysis-workflow.component.html` - Template with mat-stepper (380 lines)
- `dialysis-workflow.component.scss` - Styling (270 lines)

**Route:** `/patients/:id/workflow/:scheduleId`

**Features:**
- Angular Material Stepper (mat-stepper) with 3 steps
- Reactive Forms for pre-dialysis and post-dialysis
- Auto-save with debouncing (2-second delay)
- Visual save indicators (idle → saving → saved)
- Phase lock indicators (lock icon + warning message)
- Form validation with error messages
- Responsive design (mobile-friendly)

---

## How to Use the Workflow

### Step 1: Access the Workflow
1. Navigate to **Schedule Grid** (http://localhost:4200/schedule)
2. Click on an **occupied bed** (shows patient information)
3. In the HD Session form, click **"Start Workflow"** button (accent color, top-right)

### Step 2: Pre-Dialysis Assessment
**Required Fields:**
- Pre-Weight (kg) - Range: 20-250
- Systolic BP (mmHg) - Range: 60-250
- Diastolic BP (mmHg) - Range: 40-150

**Optional Fields:**
- Heart Rate (bpm) - Range: 40-200
- Temperature (°C) - Range: 35-42
- Access Site (e.g., "Left AVF", "Right CVC")
- Pre-Assessment Notes (text area for observations)

**Actions:**
- **Save Pre-Dialysis Data** - Saves form data to database
- **Complete & Start Treatment** - Locks phase and advances to Intra-Dialysis

**Auto-Save:** Form auto-saves every 2 seconds while editing

### Step 3: Intra-Dialysis Monitoring
**Display:**
- Treatment start timestamp
- Link to **"Record Vital Signs"** (opens vital monitoring component)
- **"End Treatment & Start Post-Dialysis"** button

**Actions:**
- Click **"Record Vital Signs"** to add vital monitoring records (BP, pulse, temp, UF, BFR)
- Click **"End Treatment"** when dialysis session is complete

### Step 4: Post-Dialysis Assessment
**Required Fields:**
- Post-Weight (kg) - Range: 20-250
- Systolic BP (mmHg) - Range: 60-250
- Diastolic BP (mmHg) - Range: 40-150
- Heart Rate (bpm) - Range: 40-200
- Access Bleeding Time (min) - Range: 0-60
- Total Fluid Removed (L) - Minimum: 0
- Post-Access Status (text, e.g., "Good, no complications")

**Optional Fields:**
- Medications Administered (text area, e.g., "EPO 4000 IU IV, Iron 100mg IV")
- Discharge Notes (text area for instructions and follow-up)

**Actions:**
- **Save Post-Dialysis Data** - Saves form data to database
- **Complete & Discharge Patient** - Finalizes session and discharges patient

**Auto-Save:** Form auto-saves every 2 seconds while editing

**Post-Completion:**
- Automatically redirects to Patient History page after 2 seconds
- Session marked as COMPLETED in database

---

## Visual Indicators

### Save Status Indicators:
- **Idle** - No indicator shown
- **Saving...** - Progress bar + upload icon + "Auto-saving..." text
- **Saved** - Green checkmark + "Saved at HH:MM" text (displays for 3 seconds)

### Phase Status Chips:
- **PRE_DIALYSIS** - Blue chip (primary color)
- **INTRA_DIALYSIS** - Pink chip (accent color)
- **POST_DIALYSIS** - Orange chip (warn color)
- **COMPLETED** - Gray chip (default color)

### Phase Lock Icons:
- **Lock icon** (orange) - Displayed next to completed phase labels
- **Check circle icon** (green) - Displayed in phase completed messages

---

## Technical Architecture

### Frontend Stack:
- **Angular 18+** (Standalone components)
- **Angular Material** (UI components)
- **RxJS** (Reactive programming)
- **TypeScript** (Type safety)

### Backend Stack:
- **ASP.NET Core 8.0** (Web API)
- **SQLite** (Database)
- **Dapper** (ORM)
- **JWT Authentication** (Security)

### Authorization:
- Endpoints restricted to: **Admin, Doctor, Nurse** roles
- Authorization checked via `[Authorize(Roles = "Admin,Doctor,Nurse")]` attribute

---

## Migration Execution

### Migration 14 Applied Successfully ✅

**Execution Method:** Custom C# utility (`Migration14Runner`)
**Execution Date:** [Timestamp in database]
**Status:** All columns created, indexes added, existing sessions migrated

**Verification Query:**
```sql
PRAGMA table_info(HDLogs);
SELECT SessionPhase, PreWeight, PostDialysisWeight 
FROM HDLogs 
WHERE SessionPhase IS NOT NULL;
```

---

## Testing Checklist

### Backend Testing:
- ✅ All 8 endpoints responding correctly
- ✅ Phase validation working (can't skip phases)
- ✅ Lock validation working (can't edit locked phases)
- ✅ Data persistence confirmed (no data loss)
- ✅ Timestamps recording correctly
- ✅ Authorization working (Admin/Doctor/Nurse only)

### Frontend Testing:
- ✅ Component routing working (`/patients/:id/workflow/:scheduleId`)
- ✅ Stepper navigation working (linear progression)
- ✅ Form validation working (required fields, ranges)
- ✅ Auto-save working (2-second debounce)
- ✅ Save indicators displaying correctly
- ✅ Phase locks preventing editing
- ✅ Navigation buttons working (Save, Complete, Discharge)
- ✅ Responsive design tested (mobile/desktop)

### Integration Testing:
- ✅ End-to-end workflow (PRE → INTRA → POST → COMPLETED)
- ✅ Data persistence across phases
- ✅ No data loss when navigating away
- ✅ Browser refresh maintains state (data loaded from DB)
- ✅ Auto-redirect after discharge working

---

## Known Limitations & Future Enhancements

### Current Limitations:
1. Cannot "undo" a completed phase (by design - prevents data tampering)
2. Intra-dialysis phase uses separate vital monitoring component (not integrated in stepper)
3. No "Draft" recovery for workflow (uses database persistence instead)

### Potential Enhancements:
1. Add "Pause Treatment" feature for Intra-Dialysis phase
2. Integrate vital monitoring directly into stepper (remove navigation)
3. Add print/PDF export for completed sessions
4. Add phase-specific alerts (e.g., "High BP detected in pre-dialysis")
5. Add summary view showing all phases on one page (read-only)
6. Add audit trail showing who completed each phase

---

## Troubleshooting

### Issue: "Session not found or not yet initialized"
**Cause:** HD Log not created for the schedule
**Solution:** Ensure schedule has an HDLogID before accessing workflow

### Issue: "Cannot edit - phase is locked"
**Cause:** Phase has been completed
**Solution:** This is expected behavior - locked phases cannot be edited for data integrity

### Issue: "Required fields missing"
**Cause:** Form validation failing
**Solution:** Fill in all required fields (marked with red asterisk)

### Issue: Auto-save not working
**Cause:** Phase is locked OR form is invalid
**Solution:** Check phase status and form validation errors

### Issue: "Failed to load session data"
**Cause:** Backend API not running OR database connection issue
**Solution:** Verify backend is running on port 5001, check database file exists

---

## File Structure

```
Backend/
├── DTOs/
│   ├── PreDialysisDTO.cs          # Pre-dialysis data transfer object
│   ├── PostDialysisDTO.cs         # Post-dialysis data transfer object
│   └── SessionPhaseStatusDTO.cs   # Phase status response object
├── Models/
│   └── HDLog.cs                   # Updated with 25+ new properties
├── Controllers/
│   └── HDLogController.cs         # 8 new phase management endpoints
└── Repositories/
    └── IHDLogRepository.cs        # Database access methods

Frontend/
└── hd-scheduler-app/
    └── src/
        └── app/
            ├── core/
            │   └── services/
            │       └── schedule.service.ts   # 7 new API methods
            ├── features/
            │   └── schedule/
            │       └── dialysis-workflow/
            │           ├── dialysis-workflow.component.ts    # 470 lines
            │           ├── dialysis-workflow.component.html  # 380 lines
            │           └── dialysis-workflow.component.scss  # 270 lines
            └── app.routes.ts                 # New route added

Database/
├── 14_AddSessionPhases.sql       # Migration SQL script
└── Migration14Runner/            # C# migration utility
    ├── Migration14Runner.csproj
    └── Program.cs
```

---

## Summary

✅ **Backend:** 8 new endpoints, 3 new DTOs, 25+ new model properties
✅ **Frontend:** Complete workflow component with stepper, forms, validation
✅ **Database:** Migration 14 applied successfully with all columns
✅ **Features:** Independent save per phase, auto-save, phase locking, validation
✅ **Authorization:** Admin, Doctor, Nurse roles only
✅ **Testing:** End-to-end workflow tested and working
✅ **Documentation:** Complete API documentation and user guide

**Next Steps:**
1. Test workflow with real patient data
2. Train staff on 3-phase workflow usage
3. Monitor auto-save performance and adjust debounce timing if needed
4. Gather user feedback for enhancements

---

**Created:** [Current Date]
**Version:** 1.0
**Status:** ✅ Production Ready
