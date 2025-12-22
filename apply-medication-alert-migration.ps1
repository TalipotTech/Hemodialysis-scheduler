# Apply medication and alert columns migration to database
# Run this script to add the missing columns to HDSchedule table

$scriptPath = "add-medication-alert-columns.sql"
$server = "localhost"  # Change if needed
$database = "HDSchedulerDB"

Write-Host "Applying medication and alert columns migration..." -ForegroundColor Yellow

try {
    # Using integrated Windows authentication
    sqlcmd -S $server -d $database -i $scriptPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration applied successfully!" -ForegroundColor Green
        Write-Host "The HDSchedule table now has medication and alert columns." -ForegroundColor Cyan
    } else {
        Write-Host "✗ Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error running migration: $_" -ForegroundColor Red
    Write-Host "`nPlease run the SQL script manually in SQL Server Management Studio or Azure Data Studio" -ForegroundColor Yellow
    Write-Host "Script location: $scriptPath" -ForegroundColor Yellow
}
