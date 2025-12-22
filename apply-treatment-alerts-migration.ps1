# PowerShell script to create TreatmentAlerts table
# Run this from the project root directory

$server = "hds-dev-sqlserver-cin.database.windows.net"
$database = "hds-dev-db"
$username = "hdsadmin"
$password = "Talipot@123"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Creating TreatmentAlerts Table" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Target Database: $database" -ForegroundColor Yellow
Write-Host "Creating table for tracking treatment alerts and incidents" -ForegroundColor Yellow
Write-Host ""

$sqlFile = "create-treatment-alerts-table.sql"

if (-not (Test-Path $sqlFile)) {
    Write-Host "ERROR: SQL file not found: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "Executing SQL migration..." -ForegroundColor Green
sqlcmd -S $server -d $database -U $username -P $password -i $sqlFile

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ TreatmentAlerts table created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Verifying table structure..." -ForegroundColor Cyan
    sqlcmd -S $server -d $database -U $username -P $password -Q "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TreatmentAlerts' ORDER BY ORDINAL_POSITION"
} else {
    Write-Host ""
    Write-Host "✗ Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
