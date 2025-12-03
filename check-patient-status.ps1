# Check patient IsActive status
Add-Type -Path "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll"

$connectionString = "Data Source=Backend/HDScheduler.db;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = "SELECT PatientID, Name, IsActive FROM Patients ORDER BY PatientID"

$reader = $command.ExecuteReader()
Write-Host "`nPatient Status in Database:"
Write-Host "=============================="
while ($reader.Read()) {
    $id = $reader["PatientID"]
    $name = $reader["Name"]
    $isActive = $reader["IsActive"]
    $status = if ($isActive -eq 1) { "ACTIVE" } else { "DISCHARGED" }
    Write-Host "ID: $id | Name: $name | IsActive: $isActive | Status: $status"
}

$reader.Close()
$connection.Close()
