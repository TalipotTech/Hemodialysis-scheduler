# Check Patients table data
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$queries = @{
    "Patients" = "SELECT TOP 5 PatientID, Name, Age, Gender, MRN, HDCycle, HDStartDate, IsActive FROM Patients ORDER BY PatientID"
    "HDSchedule" = "SELECT COUNT(*) as TotalSchedules FROM HDSchedule"
    "HDLogs" = "SELECT COUNT(*) as TotalLogs FROM HDLogs"
    "Slots" = "SELECT SlotID, SlotName, StartTime, EndTime, BedCapacity FROM Slots"
}

try {
    Write-Host "Checking database tables..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!`n" -ForegroundColor Green
    
    foreach ($table in $queries.Keys) {
        Write-Host "=== $table Table ===" -ForegroundColor Yellow
        
        $command = New-Object System.Data.SqlClient.SqlCommand($queries[$table], $connection)
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        
        if ($dataset.Tables[0].Rows.Count -eq 0) {
            Write-Host "No data found!" -ForegroundColor Red
        } else {
            $dataset.Tables[0] | Format-Table -AutoSize
        }
        Write-Host ""
    }
    
    $connection.Close()
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
