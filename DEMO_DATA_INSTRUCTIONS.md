# Load Demo Data Instructions

## Quick Steps to Load Demo Patients and Sessions

### Option 1: Using VS Code SQLite Extension (Recommended)

1. **Install SQLite Extension** (if not already installed):
   - Press `Ctrl+Shift+X` to open Extensions
   - Search for "SQLite" by alexcvzz
   - Click Install

2. **Open the Database**:
   - Open `Backend/HDScheduler.db`
   - Right-click → "Open Database"
   - The database will appear in SQLite Explorer panel

3. **Execute the SQL Script**:
   - Open `LOAD_DEMO_DATA.sql` file in VS Code
   - Right-click in the editor → "Run Query"
   - Or press `Ctrl+Shift+Q`

### Option 2: Using DB Browser for SQLite

1. Download and install [DB Browser for SQLite](https://sqlitebrowser.org/)
2. Open `Backend/HDScheduler.db`
3. Go to "Execute SQL" tab
4. Copy and paste contents from `LOAD_DEMO_DATA.sql`
5. Click "Execute" button (▶️)

### Option 3: Manual Copy-Paste in VS Code

1. Right-click on `Backend/HDScheduler.db` → "Open Database"
2. In SQLite Explorer, right-click on database → "New Query"
3. Copy entire content from `LOAD_DEMO_DATA.sql`
4. Paste and execute

---

## What Gets Created

### 8 Demo Patients:
- John Smith (58M) - TTS cycle
- Sarah Johnson (62F) - MWF cycle
- Michael Brown (45M) - TTS cycle
- Emily Davis (71F) - MWF cycle
- Robert Wilson (53M) - TTS cycle
- Linda Martinez (67F) - MWF cycle
- James Anderson (49M)
- Patricia Taylor (74F)

### HD Sessions:
- **5 Active Sessions** (Today, Nov 21) - Currently on dialysis
- **8 Pre-Scheduled Sessions** (Nov 22-24) - Auto-generated recurring sessions
- **4 Completed Sessions** (Nov 18-19) - Historical data

---

## After Loading Demo Data

1. **Refresh the frontend** (if already open)
2. **Check Bed Schedule** - You'll see:
   - Today: 5 active sessions with bed assignments
   - Tomorrow & beyond: Pre-scheduled sessions (no bed assigned yet)
3. **View Patient List** - 8 new patients with MRN001-008
4. **Check Patient History** - Completed sessions visible

---

## Troubleshooting

### "duplicate column name" Error
This means the migration (STEP 1) was already run. Just skip to STEP 2.

### "no such column: SessionStatus"
You need to run STEP 1 first to add the required columns.

### Patient IDs Don't Match
The script uses `(SELECT MAX(PatientID) - X FROM Patients)` to dynamically calculate IDs based on your existing patients.

---

## Reset Demo Data (if needed)

To remove demo data:

```sql
-- Delete demo patients (and their sessions will cascade)
DELETE FROM Patients WHERE MRN LIKE 'MRN00%';

-- Or delete only demo sessions
DELETE FROM HDSchedule WHERE CreatedByStaffName IN ('admin', 'System') 
AND PatientID IN (SELECT PatientID FROM Patients WHERE MRN LIKE 'MRN00%');
```

---

## Next Steps

After loading demo data, restart the backend to ensure all changes are picked up:
1. Stop the backend (if running)
2. `cd Backend`
3. `dotnet run`
4. Frontend will automatically show the new data
