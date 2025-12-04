# Check Session 91 status and Patient 53 counters
Add-Type -Path "G:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\System.Data.SQLite.dll"

$conn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=G:\ENSATE\HD_Project\Backend\HDScheduler.db")
$conn.Open()

Write-Host "`n=== Session 91 Status ===" -ForegroundColor Cyan
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ScheduleID, PatientID, SessionDate, IsDischarged, IsMovedToHistory, SessionStatus FROM HDSchedule WHERE ScheduleID = 91"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "ScheduleID: $($reader['ScheduleID'])"
    Write-Host "PatientID: $($reader['PatientID'])"
    Write-Host "SessionDate: $($reader['SessionDate'])"
    Write-Host "IsDischarged: $($reader['IsDischarged'])"
    Write-Host "IsMovedToHistory: $($reader['IsMovedToHistory'])"
    Write-Host "SessionStatus: $($reader['SessionStatus'])"
}
$reader.Close()

Write-Host "`n=== Patient 53 Counters ===" -ForegroundColor Cyan
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT PatientID, Name, DialyserCount, BloodTubingCount, TotalDialysisCompleted FROM Patients WHERE PatientID = 53"
$reader2 = $cmd2.ExecuteReader()
while ($reader2.Read()) {
    Write-Host "PatientID: $($reader2['PatientID'])"
    Write-Host "Name: $($reader2['Name'])"
    Write-Host "DialyserCount: $($reader2['DialyserCount'])"
    Write-Host "BloodTubingCount: $($reader2['BloodTubingCount'])"
    Write-Host "TotalDialysisCompleted: $($reader2['TotalDialysisCompleted'])"
}
$reader2.Close()

$conn.Close()
Write-Host ""
