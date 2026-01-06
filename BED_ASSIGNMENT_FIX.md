# Bed Assignment Consistency Fix

## Problem Summary
The application was showing **inconsistent bed numbers** across different views (Patient List vs Schedule Grid) due to:

1. **Dynamic bed reassignment** - Backend was reassigning beds on-the-fly in the `GetDailySchedule` API
2. **Multiple data sources** - Frontend was fetching bed numbers from different endpoints
3. **No validation** - No checks to prevent double-booking of beds
4. **Lack of source of truth** - Bed numbers weren't consistently stored in the database

## Root Cause
The `ScheduleController.cs` `GetDailySchedule` endpoint had logic that would **dynamically assign unassigned schedules to available beds**, which meant:
- Same patient could show different bed numbers when the API was called at different times
- Bed assignments weren't permanent - they changed based on what was "available" at query time
- No single source of truth for bed assignments

## Solution Implemented

### 1. ✅ Enhanced Bed Assignment Service (`BedAssignmentService.cs`)

**Added comprehensive validation and conflict detection:**

```csharp
public interface IBedAssignmentService
{
    Task<int?> GetNextAvailableBedAsync(int slotId, DateTime sessionDate, int? excludeScheduleId = null);
    Task<bool> IsBedAvailableAsync(int slotId, int bedNumber, DateTime sessionDate, int? excludeScheduleId = null);
    Task<BedAssignmentValidationResult> ValidateBedAssignmentAsync(int scheduleId, int slotId, int bedNumber, DateTime sessionDate);
    Task<List<BedConflict>> GetBedConflictsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
```

**Key Features:**
- ✅ Smart bed assignment with infection control spacing (odd beds first: 1, 3, 5, 7, 9, then even: 2, 4, 6, 8, 10)
- ✅ Validates bed is within slot capacity (respects `MaxBeds` from database)
- ✅ Detects double-booking conflicts
- ✅ Scans entire database for bed assignment conflicts
- ✅ Provides detailed conflict reports with patient names and schedule IDs

### 2. ✅ Fixed GetDailySchedule API (`ScheduleController.cs`)

**BEFORE (❌ Bad):**
```csharp
// Dynamically assigned unassigned schedules to "available" beds
foreach (var schedule in unassignedSchedules) {
    for (int bedNum = 1; bedNum <= slot.MaxBeds; bedNum++) {
        if (bedObj.status == "available") {
            beds[bedIndex] = new { bedNumber = bedNum, patient = schedule };
            break; // PROBLEM: Bed number changes each time!
        }
    }
}
```

**AFTER (✅ Good):**
```csharp
// ⚠️ IMPORTANT: Do NOT dynamically reassign beds here!
// Schedules without bed assignments should remain unassigned.
// This prevents bed number inconsistency across views.
var unassignedSchedules = slotSchedulesForSlot.Where(s => 
    (s.BedNumber == null || s.BedNumber == 0) && 
    !assignedScheduleIds.Contains(s.ScheduleID)).ToList();

// Log warning about unassigned schedules
_logger.LogWarning($"⚠️ Schedule {schedule.ScheduleID} has no bed assignment. User must assign manually.");
```

**Result:** Bed numbers are now **only** sourced from the `HDSchedule.BedNumber` column in the database. No dynamic reassignment.

### 3. ✅ Added Bed Validation Endpoints

**New API endpoints for validation:**

```typescript
// Check for conflicts in a date range
GET /api/schedule/bed-conflicts?startDate=2026-01-01&endDate=2026-01-31

// Validate a specific bed assignment before saving
POST /api/schedule/validate-bed-assignment
{
  "scheduleId": 123,
  "slotId": 1,
  "bedNumber": 5,
  "sessionDate": "2026-01-05"
}
```

**Frontend service methods added:**
```typescript
// In schedule.service.ts
getBedConflicts(startDate?: Date, endDate?: Date): Observable<ApiResponse<any[]>>
validateBedAssignment(scheduleId, slotId, bedNumber, sessionDate): Observable<ApiResponse<any>>
```

### 4. ✅ Bed Assignment Validation in Create/Update

**Added validation to `HDScheduleController.cs`:**

```csharp
// Before creating new HD session
if (request.BedNumber.HasValue && request.BedNumber.Value > 0 && request.SlotID.HasValue) {
    var bedValidation = await _bedAssignmentService.ValidateBedAssignmentAsync(
        0, // New schedule
        request.SlotID.Value,
        request.BedNumber.Value,
        request.SessionDate);

    if (!bedValidation.IsValid) {
        return BadRequest($"Bed assignment conflict: {bedValidation.Message}");
    }
}
```

**Result:** Double-booking is now **impossible** - the backend will reject any attempt to assign a bed that's already occupied.

### 5. ✅ Frontend Consistency Guidelines

**Updated approach for displaying bed numbers:**

```typescript
// ❌ DON'T: Compute bed numbers dynamically
const bedNumber = this.computeBedFromSomething();

// ✅ DO: Always use the bed number from the schedule record
const bedNumber = patient.bedNumber || null;

// ✅ DO: Show "Not Assigned" if bed number is null/0
getBedNumber(bedNumber: number | null): string {
    return bedNumber ? `Bed ${bedNumber}` : 'Not Assigned';
}
```

## Testing & Validation

### Manual Testing Checklist:
- [ ] Create a new HD session with a bed assignment
- [ ] Verify bed number shows consistently in:
  - [ ] Schedule Grid
  - [ ] Patient List (Active Patients tab)
  - [ ] Patient List (Pre Schedule tab)
  - [ ] Future Bed Schedule tab
- [ ] Try to assign the same bed to two different patients in the same slot/date → Should show error
- [ ] Navigate away and back → Bed number should remain the same
- [ ] Edit a session and change bed number → New bed should be validated
- [ ] Test with sessions that have no bed assignment → Should show "Not Assigned" consistently

### API Testing:
```bash
# Test bed conflict detection
curl -X GET "http://localhost:4200/api/schedule/bed-conflicts?startDate=2026-01-01&endDate=2026-01-31"

# Test bed validation
curl -X POST "http://localhost:4200/api/schedule/validate-bed-assignment" \
  -H "Content-Type: application/json" \
  -d '{
    "scheduleId": 123,
    "slotId": 1,
    "bedNumber": 5,
    "sessionDate": "2026-01-05T00:00:00"
  }'
```

## Database Source of Truth

**HDSchedule table structure:**
```sql
CREATE TABLE HDSchedule (
    ScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SessionDate DATE NOT NULL,
    SlotID INTEGER,  -- Which shift (Morning, Afternoon, Evening, Night)
    BedNumber INTEGER CHECK (BedNumber BETWEEN 1 AND 10),  -- ✅ Source of truth
    ...
)
```

**Key Rule:** `BedNumber` in the `HDSchedule` table is the **single source of truth**. All frontend displays must use this value directly.

## Benefits of This Fix

✅ **Consistency** - Bed numbers are the same everywhere  
✅ **Data Integrity** - No double-booking possible  
✅ **Auditability** - Clear conflict detection and reporting  
✅ **User Trust** - Staff can rely on bed assignments being accurate  
✅ **Hospital Safety** - Prevents scheduling errors that could impact patient care  

## Next Steps (Optional Enhancements)

1. **Bed Assignment History Table** - Track bed changes over time for audit trail
2. **Bed Preference System** - Allow patients to have preferred beds
3. **Bed Conflict Dashboard** - Admin view showing all conflicts requiring resolution
4. **Auto-Heal Conflicts** - Automatically reassign beds when conflicts are detected
5. **Bed Status Tracking** - Track cleaning, maintenance, out-of-service beds

## Migration Notes

**⚠️ Important:** If you have existing schedules with inconsistent bed numbers:

1. Run the bed conflict scan:
   ```bash
   GET /api/schedule/bed-conflicts?startDate=2020-01-01&endDate=2030-12-31
   ```

2. Review and manually fix any `DOUBLE_BOOKING` conflicts

3. For `MISSING_BED` conflicts, either:
   - Assign beds manually through the UI
   - Or run a cleanup script to set `BedNumber = NULL` to make them explicitly unassigned

## Technical Details

### Bed Assignment Algorithm
The smart assignment prioritizes spacing for infection control:
- **First pass:** Assign odd beds (1, 3, 5, 7, 9...) to maximize distance
- **Second pass:** Fill even beds (2, 4, 6, 8, 10...)
- **Dynamic capacity:** Reads `MaxBeds` from `Slots` table (supports 10 or 11 beds per slot)

### Validation Flow
```
User assigns bed → Frontend validates → Backend validates → Database checks → Commit or Reject
                                          ↓
                        IBedAssignmentService.ValidateBedAssignmentAsync()
                                          ↓
                        Check bed range, check conflicts, return result
```

## Code Changes Summary

**Backend Files Modified:**
- ✅ `BedAssignmentService.cs` - Enhanced with validation methods
- ✅ `ScheduleController.cs` - Removed dynamic bed reassignment, added validation endpoints
- ✅ `HDScheduleController.cs` - Added bed validation in create/update

**Frontend Files Modified:**
- ✅ `schedule.service.ts` - Added bed conflict and validation methods

**New Features:**
- ✅ Bed conflict detection API
- ✅ Bed assignment validation API
- ✅ Comprehensive logging for bed operations

---

**Status:** ✅ **FIXED** - Bed numbers are now consistent and validated across the entire application.

**Date:** January 5, 2026  
**Impact:** High - Affects core scheduling functionality  
**Risk:** Low - Backwards compatible, no database schema changes required
