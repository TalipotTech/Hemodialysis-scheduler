# HD Session Auto-History Feature

## Overview
This feature automatically moves completed HD treatment sessions to patient history based on slot completion times, ensuring that only active/current sessions are displayed in the main schedule view.

## Slot Timings

| Slot # | Name       | Timing                  | Capacity |
|--------|------------|-------------------------|----------|
| 1      | Morning    | 6:00 AM - 10:00 AM     | 10 beds  |
| 2      | Afternoon  | 11:00 AM - 3:00 PM     | 10 beds  |
| 3      | Evening    | 4:00 PM - 8:00 PM      | 10 beds  |
| 4      | Night      | 9:00 PM - 1:00 AM      | 10 beds  |

## How It Works

### Automatic Session Transition
Sessions automatically move to history after their slot time completes:

1. **Slot 1 (Morning)**: Sessions move to history after 10:00 AM
2. **Slot 2 (Afternoon)**: Sessions move to history after 3:00 PM
3. **Slot 3 (Evening)**: Sessions move to history after 8:00 PM
4. **Slot 4 (Night)**: Sessions move to history after 1:00 AM (next day)

### Session Lifecycle
1. **Patient Admission**: When a patient is admitted to a slot, a new HD session is created
2. **Active Session**: The session remains active and visible during the slot time
3. **Treatment Duration**: Standard dialysis treatment is 4 hours
4. **Auto-Move**: After the slot ends, the session automatically moves to history
5. **History Access**: All session data is preserved and accessible in patient history

## Backend Implementation

### Database Schema
Added new column to `HDSchedule` table:
- `IsMovedToHistory` (INTEGER): Flag to track if session moved to history (0 = active, 1 = history)

### New Endpoints
- `GET /api/hdschedule/history` - Retrieve all sessions in history
- `GET /api/hdschedule/today` - Get today's active sessions (auto-filters completed slots)
- `GET /api/hdschedule/active` - Get all active sessions (auto-filters completed slots)

### Auto-Move Logic
The `MoveCompletedSessionsToHistoryAsync()` method runs automatically when:
- Fetching today's schedules
- Fetching active sessions
- Checking slot availability

```csharp
// Example: Sessions automatically moved based on time
// Slot 1: 6:00 AM - 10:00 AM
// If current time > 10:00 AM, session moves to history

// Slot 4: 9:00 PM - 1:00 AM (crosses midnight)
// Session moves to history after 1:00 AM the next day
```

## Frontend Implementation

### Updated Components
1. **HdSessionScheduleComponent**: Updated slot timings display
2. **ScheduleService**: Added methods for history retrieval
3. **Schedule Models**: Added `isMovedToHistory` field

### New Features
- Real-time filtering of active vs. history sessions
- Slot timing information displayed during scheduling
- Automatic refresh to show current session status

## API Examples

### Get History Sessions
```http
GET /api/hdschedule/history
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "data": [
    {
      "scheduleID": 123,
      "patientID": 1,
      "patientName": "John Doe",
      "sessionDate": "2025-11-14T00:00:00",
      "slotID": 1,
      "bedNumber": 5,
      "isMovedToHistory": true,
      "isDischarged": false,
      "createdAt": "2025-11-14T06:30:00",
      "updatedAt": "2025-11-14T10:00:05"
    }
  ]
}
```

### Get Today's Active Sessions
```http
GET /api/hdschedule/today
Authorization: Bearer {token}
```

Response: Only returns sessions that haven't passed their slot end time.

## Benefits

1. **Clean Interface**: Only current/active sessions shown in main view
2. **Automatic Management**: No manual intervention needed to move sessions
3. **Data Preservation**: All session data retained in history
4. **Accurate Scheduling**: Prevents double-booking and slot conflicts
5. **Easy Reporting**: Clear separation between active and historical data

## Migration

### Database Migration Script
Location: `Database/09_AddIsMovedToHistoryColumn.sql`

To apply migration manually:
```bash
cd Database/MigrationTool
dotnet run
```

The migration:
- Adds `IsMovedToHistory` column with default value 0
- Creates performance index on (IsMovedToHistory, SessionDate, SlotID)
- Moves old sessions (> 1 day old) to history automatically

## Testing

### Test Scenarios

1. **Active Session Display**
   - Schedule a patient for current slot
   - Verify session appears in today's schedule
   - Check session is not in history

2. **Auto-Move After Slot Completion**
   - Wait for slot end time to pass
   - Refresh today's schedule
   - Verify session moved to history

3. **History Retrieval**
   - Call history endpoint
   - Verify all completed sessions appear
   - Check all session data is intact

4. **Multiple Slots**
   - Test all 4 slots independently
   - Verify each slot's sessions move at correct time
   - Test overnight slot (Slot 4) crossing midnight

### Test Times
For testing, you can adjust the system time or wait for:
- Slot 1: Test after 10:00 AM
- Slot 2: Test after 3:00 PM
- Slot 3: Test after 8:00 PM
- Slot 4: Test after 1:00 AM next day

## Notes

- Sessions remain in history permanently for record-keeping
- `IsDischarged` flag is separate from `IsMovedToHistory`
- A session can be both discharged and moved to history
- History sessions can still be viewed/reported on
- The system checks slot completion every time schedules are fetched

## Future Enhancements

Potential improvements:
1. Configurable slot timings per facility
2. Grace period before auto-move (e.g., 15 minutes buffer)
3. Manual override to keep session active longer
4. Notification when session moves to history
5. Dashboard widget showing today's completed sessions
