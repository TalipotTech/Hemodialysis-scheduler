Add-Type -Path "g:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\System.Data.SQLite.dll"

$db = New-Object System.Data.SQLite.SQLiteConnection("Data Source=g:\ENSATE\HD_Project\Backend\HDScheduler.db")
$db.Open()

Write-Host "`n=== Patients in Database ===" -ForegroundColor Green
$cmd = $db.CreateCommand()
$cmd.CommandText = "SELECT PatientID, MRN, Name FROM Patients ORDER BY PatientID"
$reader = $cmd.ExecuteReader()
while($reader.Read()) {
    Write-Host "ID: $($reader[0]), MRN: $($reader[1]), Name: $($reader[2])"
}
$reader.Close()

Write-Host "`n=== HD Schedules for Today ===" -ForegroundColor Green
$cmd = $db.CreateCommand()
$cmd.CommandText = @"
SELECT h.ScheduleID, p.Name, h.SessionDate, h.SlotID, h.BedNumber, h.IsDischarged 
FROM HDSchedule h 
JOIN Patients p ON h.PatientID = p.PatientID 
WHERE date(h.SessionDate) = date('now') 
ORDER BY h.IsDischarged, h.SlotID, h.BedNumber
"@
$reader = $cmd.ExecuteReader()
Write-Host "ScheduleID | PatientName | SessionDate | SlotID | Bed | Discharged"
Write-Host "-------------------------------------------------------------------"
while($reader.Read()) {
    $status = if($reader[5] -eq 1) { "YES" } else { "NO" }
    Write-Host "$($reader[0]) | $($reader[1]) | $($reader[2]) | $($reader[3]) | $($reader[4]) | $status"
}
$reader.Close()

Write-Host "`n=== ACTIVE (Non-Discharged) Schedules ===" -ForegroundColor Cyan
$cmd = $db.CreateCommand()
$cmd.CommandText = @"
SELECT h.ScheduleID, p.Name, h.SlotID, h.BedNumber 
FROM HDSchedule h 
JOIN Patients p ON h.PatientID = p.PatientID 
WHERE date(h.SessionDate) = date('now') AND h.IsDischarged = 0
ORDER BY h.SlotID, h.BedNumber
"@
$reader = $cmd.ExecuteReader()
$count = 0
while($reader.Read()) {
    Write-Host "$($reader[0]) | $($reader[1]) | Slot: $($reader[2]) | Bed: $($reader[3])"
    $count++
}
$reader.Close()
Write-Host "Total Active: $count" -ForegroundColor Yellow

$db.Close()
