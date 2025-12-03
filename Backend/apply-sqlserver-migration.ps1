# Apply Equipment Purchase Migration to SQL Server LocalDB
Write-Host "Applying Equipment Purchase Migration to SQL Server..." -ForegroundColor Cyan

$migrationFile = "Migrations\20251202_AddEquipmentPurchaseTracking.sql"
$serverInstance = "(localdb)\MSSQLLocalDB"
$database = "HDScheduler"

if (!(Test-Path $migrationFile)) {
    Write-Host "Migration file not found: $migrationFile" -ForegroundColor Red
    exit 1
}

try {
    # Run the migration using sqlcmd
    Write-Host "Executing migration on $serverInstance\$database..." -ForegroundColor Yellow
    
    sqlcmd -S $serverInstance -d $database -i $migrationFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error running migration: $_" -ForegroundColor Red
}
