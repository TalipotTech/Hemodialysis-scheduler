# PowerShell script to add post-dialysis vitals columns to HDSchedule table
# Run this from the project root directory

$server = "hds-dev-sqlserver-cin.database.windows.net"
$database = "hds-dev-db"
$username = "hdsadmin"
$password = "Talipot@123"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Adding Post-Dialysis Vitals Columns" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Target Database: $database" -ForegroundColor Yellow
Write-Host "Adding columns: PostWeight, PostSBP, PostDBP, PostHR, TotalFluidRemoved, PostAccessStatus" -ForegroundColor Yellow
Write-Host ""

$sqlFile = "add-postdialysis-vitals-columns.sql"

if (-not (Test-Path $sqlFile)) {
    Write-Host "ERROR: SQL file not found: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "Executing SQL migration..." -ForegroundColor Green
sqlcmd -S $server -d $database -U $username -P $password -i $sqlFile

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Post-dialysis vitals columns added successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Verifying columns..." -ForegroundColor Cyan
    sqlcmd -S $server -d $database -U $username -P $password -Q "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME IN ('PostWeight', 'PostSBP', 'PostDBP', 'PostHR', 'TotalFluidRemoved', 'PostAccessStatus') ORDER BY COLUMN_NAME"
} else {
    Write-Host ""
    Write-Host "✗ Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
