# Populate demo historical data for patient Achu
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = Get-Content "populate-demo-history.sql" -Raw

try {
    Write-Host "Populating demo historical HD sessions..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host "Creating 7 historical sessions for patient Achu..." -ForegroundColor Yellow
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $command.CommandTimeout = 60
    $reader = $command.ExecuteReader()
    
    Write-Host "`nResults:" -ForegroundColor Cyan
    do {
        while ($reader.Read()) {
            Write-Host "$($reader['Result']): $($reader['Count'])" -ForegroundColor Green
        }
    } while ($reader.NextResult())
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nDemo data populated successfully!" -ForegroundColor Green
    Write-Host "You can now view patient history in the application." -ForegroundColor Cyan
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor DarkRed
    exit 1
}
