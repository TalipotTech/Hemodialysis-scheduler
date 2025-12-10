# Add missing DialyserModel column to Azure SQL HDSchedule table
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = Get-Content "add-dialyser-model-column.sql" -Raw

try {
    Write-Host "Adding DialyserModel column to HDSchedule table..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $reader = $command.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host "$($reader[0])" -ForegroundColor Yellow
    }
    
    $reader.Close()
    
    # Check if more data
    if ($reader.NextResult()) {
        Write-Host "`nVerification:" -ForegroundColor Cyan
        while ($reader.Read()) {
            Write-Host "Column: $($reader['COLUMN_NAME']), Type: $($reader['DATA_TYPE']), Length: $($reader['CHARACTER_MAXIMUM_LENGTH'])" -ForegroundColor Green
        }
    }
    
    $connection.Close()
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
