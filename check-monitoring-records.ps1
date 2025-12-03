Add-Type -Path "G:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\System.Data.SQLite.dll"

$connectionString = "Data Source=G:\ENSATE\HD_Project\Backend\hd_scheduler.db"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
$connection.Open()

# Check all monitoring records
$command = $connection.CreateCommand()
$command.CommandText = "SELECT RecordID, ScheduleID, PatientID, TimeRecorded, BloodPressure, PulseRate, Temperature FROM IntraDialyticRecords ORDER BY CreatedAt DESC LIMIT 10"
$reader = $command.ExecuteReader()

Write-Host "`n=== Latest Monitoring Records ===" -ForegroundColor Cyan
Write-Host "RecordID | ScheduleID | PatientID | TimeRecorded | BloodPressure | PulseRate | Temperature"
Write-Host "------------------------------------------------------------------------"

while ($reader.Read()) {
    $recordId = $reader["RecordID"]
    $scheduleId = $reader["ScheduleID"]
    $patientId = $reader["PatientID"]
    $timeRecorded = $reader["TimeRecorded"]
    $bp = $reader["BloodPressure"]
    $pulse = $reader["PulseRate"]
    $temp = $reader["Temperature"]
    Write-Host "$recordId | $scheduleId | $patientId | $timeRecorded | $bp | $pulse | $temp"
}

$reader.Close()

# Check count by ScheduleID
$command2 = $connection.CreateCommand()
$command2.CommandText = "SELECT ScheduleID, COUNT(*) as Count FROM IntraDialyticRecords GROUP BY ScheduleID"
$reader2 = $command2.ExecuteReader()

Write-Host "`n=== Record Count by Schedule ===" -ForegroundColor Cyan
Write-Host "ScheduleID | Count"
Write-Host "-------------------"

while ($reader2.Read()) {
    $schedId = $reader2["ScheduleID"]
    $count = $reader2["Count"]
    Write-Host "$schedId | $count"
}

$reader2.Close()
$connection.Close()

Write-Host "`nDone!" -ForegroundColor Green
