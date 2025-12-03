# Apply Database Migration - Add HD Treatment Fields to Patients Table
# Run this script to add the new HD treatment fields to the Patients table

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "HD Scheduler - Database Migration" -ForegroundColor Cyan
Write-Host "Adding HD Treatment Fields to Patients Table" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

$projectRoot = $PSScriptRoot
$migrationFile = Join-Path $projectRoot "Database\20_AddHDTreatmentFieldsToPatients.sql"
$dbPath = Join-Path $projectRoot "Backend\hdscheduler.db"

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Database file not found at: $dbPath" -ForegroundColor Red
    Write-Host "Please run the project first to create the database." -ForegroundColor Yellow
    exit 1
}

# Check if migration file exists
if (-not (Test-Path $migrationFile)) {
    Write-Host "ERROR: Migration file not found at: $migrationFile" -ForegroundColor Red
    exit 1
}

Write-Host "Database: $dbPath" -ForegroundColor Green
Write-Host "Migration: $migrationFile`n" -ForegroundColor Green

# Check if sqlite3 is available
$sqlite3Path = "sqlite3"
try {
    $null = & $sqlite3Path -version 2>&1
} catch {
    Write-Host "ERROR: sqlite3 command not found" -ForegroundColor Red
    Write-Host "Please install SQLite3 command-line tools" -ForegroundColor Yellow
    Write-Host "Download from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    exit 1
}

Write-Host "Applying migration..." -ForegroundColor Yellow

# Apply migration
try {
    Get-Content $migrationFile | & $sqlite3Path $dbPath
    Write-Host "`nMigration applied successfully!" -ForegroundColor Green
    
    # Verify the changes
    Write-Host "`nVerifying new columns..." -ForegroundColor Yellow
    $verifyQuery = "SELECT name FROM pragma_table_info('Patients') WHERE name IN ('DryWeight', 'HDCycle', 'HDStartDate', 'DialyserType', 'DialyserCount', 'BloodTubingCount', 'TotalDialysisCompleted');"
    $result = $verifyQuery | & $sqlite3Path $dbPath
    
    if ($result) {
        Write-Host "`nColumns added successfully:" -ForegroundColor Green
        $result | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
    } else {
        Write-Host "`nWARNING: Could not verify columns" -ForegroundColor Yellow
    }
} catch {
    Write-Host "`nERROR: Failed to apply migration" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`n===============================================" -ForegroundColor Green
Write-Host "Migration Complete!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host "`nNew fields added to Patients table:" -ForegroundColor Cyan
Write-Host "  1. DryWeight (REAL) - Dry weight in kg" -ForegroundColor White
Write-Host "  2. HDCycle (TEXT) - HD frequency (e.g., 3x/week)" -ForegroundColor White
Write-Host "  3. HDStartDate (TEXT) - Date HD started" -ForegroundColor White
Write-Host "  4. DialyserType (TEXT) - Hi Flux or Lo Flux" -ForegroundColor White
Write-Host "  5. DialyserCount (INTEGER) - Current dialyser usage" -ForegroundColor White
Write-Host "  6. BloodTubingCount (INTEGER) - Current blood tubing usage" -ForegroundColor White
Write-Host "  7. TotalDialysisCompleted (INTEGER) - Total sessions completed`n" -ForegroundColor White

Write-Host "You can now restart the backend to use the new fields." -ForegroundColor Yellow
