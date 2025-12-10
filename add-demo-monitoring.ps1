# Add demo intra-dialytic monitoring records
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = Get-Content "add-demo-monitoring.sql" -Raw

try {
    Write-Host "Adding demo intra-dialytic monitoring records..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host "Adding hourly monitoring data for each session..." -ForegroundColor Yellow
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $command.CommandTimeout = 60
    $reader = $command.ExecuteReader()
    
    Write-Host "`nResults:" -ForegroundColor Cyan
    
    # First result set
    if ($reader.Read()) {
        Write-Host "$($reader['Result']): $($reader['Count'])" -ForegroundColor Green
    }
    
    # Second result set
    if ($reader.NextResult()) {
        Write-Host "`nMonitoring Records by Session:" -ForegroundColor Yellow
        Write-Host ("{0,-15} {1}" -f "SessionDate", "RecordCount") -ForegroundColor Cyan
        Write-Host ("-" * 40) -ForegroundColor Gray
        
        while ($reader.Read()) {
            $date = ([DateTime]$reader['SessionDate']).ToString('yyyy-MM-dd')
            $count = $reader['RecordCount']
            Write-Host ("{0,-15} {1}" -f $date, $count) -ForegroundColor White
        }
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nDemo monitoring records added successfully!" -ForegroundColor Green
    Write-Host "Refresh the patient history page to see complete session data." -ForegroundColor Cyan
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
