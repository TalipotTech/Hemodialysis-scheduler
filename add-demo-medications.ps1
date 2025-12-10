# Add demo medications to Azure SQL database
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = Get-Content "add-demo-medications.sql" -Raw

try {
    Write-Host "Adding demo medications to patient history..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host "Adding medications for HD sessions..." -ForegroundColor Yellow
    
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
        Write-Host "`nMedications by Session:" -ForegroundColor Yellow
        Write-Host ("{0,-15} {1,-15} {2}" -f "SessionDate", "MedCount", "Medications") -ForegroundColor Cyan
        Write-Host ("-" * 80) -ForegroundColor Gray
        
        while ($reader.Read()) {
            $date = ([DateTime]$reader['SessionDate']).ToString('yyyy-MM-dd')
            $count = $reader['MedicationCount']
            $meds = if ($reader['Medications'] -ne [DBNull]::Value) { $reader['Medications'] } else { "None" }
            Write-Host ("{0,-15} {1,-15} {2}" -f $date, $count, $meds) -ForegroundColor White
        }
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nDemo medications added successfully!" -ForegroundColor Green
    Write-Host "Refresh the patient history page to see the medications." -ForegroundColor Cyan
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
