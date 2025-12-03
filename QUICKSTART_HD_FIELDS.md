# Quick Start Guide - HD Treatment Fields

## ✅ Implementation Complete!

All backend and frontend code has been updated. The new HD treatment fields are ready to use!

## 🚀 Start the Application

### Option 1: Start Backend Only (Migration will run automatically)
```powershell
cd Backend
dotnet run
```

The migration will automatically apply when the backend starts. You'll see:
```
✅ Migration applied successfully!
New columns added to Patients table:
  - DryWeight
  - HDCycle
  - HDStartDate
  - DialyserType
  - DialyserCount
  - BloodTubingCount
  - TotalDialysisCompleted
```

### Option 2: Start Everything with One Command
```powershell
.\start.ps1
```

## 📋 New Features Available

### 1. **Create/Edit Patient Form**
Navigate to: `http://localhost:4200/patients/new`

**New Fields Added:**
- **Dry Weight (kg)** - Required, range 20-200 kg
- **HD Cycle/Frequency** - Required, dropdown (3x/week, 2x/week, etc.)
- **Date of HD Started** - Required, date picker
- **Dialyser: Hi/Lo Flux** - Required, dropdown
- **Dialyser Count** - Required, current usage count
- **Blood Tubing Count** - Required, current usage count
- **Total Dialysis Sessions Completed** - Required, total sessions

### 2. **Auto-Increment on Discharge**
When a patient is discharged (manually or automatically after 5 hours), the system **automatically increments**:
- `DialyserCount` +1
- `BloodTubingCount` +1
- `TotalDialysisCompleted` +1

**No manual action needed!**

### 3. **Equipment Usage Alerts**
Check equipment status via API:
```
GET /api/patients/{patientId}/equipment-status
```

**Alert Levels:**
- ✅ **OK** - 0-79% usage
- ⚠️ **Warning** - 80-89% usage
- 🔴 **Critical** - 90-99% usage
- ❌ **Expired** - 100%+ usage (replacement required immediately)

## 🧪 Test It Out

### Test 1: Create New Patient
1. Go to Patients → Add New Patient
2. Fill in basic info (name, age, etc.)
3. Scroll to "Hemodialysis Treatment Details"
4. Fill in all HD fields
5. Click "Save Patient"
6. ✅ All fields should be saved

### Test 2: Auto-Increment
1. Create patient with DialyserCount=0, BloodTubingCount=0, TotalDialysisCompleted=0
2. Schedule HD session for that patient
3. Discharge the patient
4. Check patient details
5. ✅ Counts should be: DialyserCount=1, BloodTubingCount=1, TotalDialysisCompleted=1

### Test 3: Equipment Alerts
1. Set patient's DialyserCount to 10 (out of 12 max)
2. Call: `GET /api/patients/{id}/equipment-status`
3. ✅ Should show "Warning" status with message "Dialyser usage at 83.3%. Consider replacement soon"

## 📊 Database Changes

The following columns have been added to the `Patients` table:

| Column | Type | Description | Auto-Increment |
|--------|------|-------------|----------------|
| DryWeight | REAL | Dry weight in kg | No |
| HDCycle | TEXT | HD frequency | No |
| HDStartDate | TEXT | Date HD started | No |
| DialyserType | TEXT | Hi/Lo Flux | No |
| DialyserCount | INTEGER | Current dialyser usage | ✅ Yes |
| BloodTubingCount | INTEGER | Current blood tubing usage | ✅ Yes |
| TotalDialysisCompleted | INTEGER | Total sessions completed | ✅ Yes |

## 🔧 Troubleshooting

### Migration Didn't Run?
If you see an error about columns already existing, that's fine! It means the migration already ran.

### Fields Not Showing in Form?
1. Make sure frontend is running: `cd Frontend/hd-scheduler-app && ng serve`
2. Clear browser cache (Ctrl+Shift+R)
3. Check console for errors (F12)

### Data Not Saving?
1. Check backend console for errors
2. Verify API is running: `http://localhost:5000/swagger`
3. Check patient model includes all fields

## 📱 Frontend Integration

The patient form now includes:
- ✅ Validation for all required fields
- ✅ Proper input types (number, date, dropdown)
- ✅ Helpful hints and error messages
- ✅ Save and update functionality

## 🎯 What Happens Automatically

1. **On Backend Start:**
   - Migration runs automatically
   - Columns are added to Patients table (if not already present)

2. **On Patient Discharge:**
   - DialyserCount increments by 1
   - BloodTubingCount increments by 1
   - TotalDialysisCompleted increments by 1

3. **On Equipment Status Check:**
   - System calculates usage percentages
   - Returns appropriate alert level
   - Includes replacement warnings

## ✨ You're All Set!

Run `dotnet run` in the Backend folder to start the application. The migration will apply automatically, and you can start using the new fields immediately!

---

**Need Help?** Check `HD_TREATMENT_FIELDS_IMPLEMENTATION.md` for detailed technical documentation.
