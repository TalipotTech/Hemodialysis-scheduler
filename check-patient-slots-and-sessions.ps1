# Check patient slot assignments and today's sessions
Write-Host "=== Checking Patient Slot Assignments and Today's Sessions ===" -ForegroundColor Cyan

# Get today's date in SQL Server format
$today = Get-Date -Format "yyyy-MM-dd"
Write-Host "Today's date: $today" -ForegroundColor Yellow

# SQL query to check patients with their slots and sessions
$query = @"
-- Check patients with their preferred slots and today's sessions
SELECT 
    p.PatientID,
    p.Name,
    p.PreferredSlotID,
    s.SlotName as PreferredSlot,
    p.BedNumber as PreferredBed,
    p.IsActive,
    -- Today's session info
    hs.ScheduleID,
    hs.SessionDate,
    hs.SlotID as TodaySlotID,
    hs.BedNumber as TodayBedNumber,
    hs.SessionStatus,
    hs.IsDischarged
FROM Patients p
LEFT JOIN Slots s ON p.PreferredSlotID = s.SlotID
LEFT JOIN HDSchedule hs ON p.PatientID = hs.PatientID 
    AND CAST(hs.SessionDate AS DATE) = '$today'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
WHERE p.IsActive = 1
ORDER BY 
    CASE 
        WHEN hs.ScheduleID IS NOT NULL THEN 0  -- Patients with today's session first
        ELSE 1
    END,
    p.PreferredSlotID,
    p.Name;

-- Summary statistics
SELECT 
    'Total Active Patients' as Statistic,
    COUNT(*) as Count
FROM Patients WHERE IsActive = 1
UNION ALL
SELECT 
    'Patients with Sessions Today',
    COUNT(DISTINCT hs.PatientID)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = '$today'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
UNION ALL
SELECT 
    'Pre-Scheduled for Today',
    COUNT(*)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = '$today'
    AND hs.SessionStatus = 'Pre-Scheduled'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0
UNION ALL
SELECT 
    'Active Sessions Today',
    COUNT(*)
FROM HDSchedule hs
WHERE CAST(hs.SessionDate AS DATE) = '$today'
    AND hs.SessionStatus = 'Active'
    AND hs.IsDischarged = 0
    AND hs.IsMovedToHistory = 0;
"@

# Execute query
try {
    Write-Host "`nExecuting query..." -ForegroundColor Gray
    Invoke-Sqlcmd -ServerInstance "hds-dev-server.database.windows.net" `
                  -Database "hds-dev-db" `
                  -Query $query `
                  -Username "hemodialysis" `
                  -Password "P@ssw0rd1234" `
                  -Encrypt `
                  -TrustServerCertificate | Format-Table -AutoSize
} catch {
    Write-Host "Error executing query: $_" -ForegroundColor Red
}
