# Apply VenousPressure column migration to IntraDialyticMonitoring table

Write-Host "Applying VenousPressure and Symptoms column migration..." -ForegroundColor Cyan

# Read connection string from appsettings.json
try {
    $appsettings = Get-Content -Path ".\Backend\appsettings.json" | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "Using connection string from appsettings.json" -ForegroundColor Gray
}
catch {
    Write-Host "Error reading appsettings.json: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

try {
    # Read the SQL file
    $sqlScript = Get-Content -Path ".\add-venous-pressure-column.sql" -Raw
    
    # Create connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database" -ForegroundColor Green
    
    # Execute the migration
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.CommandTimeout = 300
    
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "Migration applied successfully" -ForegroundColor Green
    
    $connection.Close()
    Write-Host "`nVenousPressure and Symptoms column migration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error applying migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
