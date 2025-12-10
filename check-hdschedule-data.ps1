# Check actual data in HDSchedule table
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = @"
SELECT TOP 10
    ScheduleID, PatientID, SessionDate, DryWeight, DialyserType, DialyserModel,
    PrescribedDuration, UFGoal, SlotID, BedNumber, SessionStatus, IsDischarged
FROM HDSchedule
ORDER BY SessionDate DESC;
"@

try {
    Write-Host "Checking HDSchedule table data..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "`nHDSchedule Table Data (Last 10 sessions):" -ForegroundColor Yellow
    Write-Host "=" * 120 -ForegroundColor Yellow
    
    if ($dataset.Tables[0].Rows.Count -eq 0) {
        Write-Host "No data found in HDSchedule table!" -ForegroundColor Red
        Write-Host "The table exists but has no records." -ForegroundColor Yellow
    } else {
        $dataset.Tables[0] | Format-Table -AutoSize
        Write-Host "`nTotal Records Found: $($dataset.Tables[0].Rows.Count)" -ForegroundColor Green
    }
    
    $connection.Close()
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
