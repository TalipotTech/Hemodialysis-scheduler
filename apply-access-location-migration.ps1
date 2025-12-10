# Apply AccessLocation Column Migration to Azure SQL
# Run this script to add the missing AccessLocation column

$serverName = "hds-dev-sqlserver-cin.database.windows.net"
$databaseName = "hds-dev-db"
$sqlFile = "add-access-location-column.sql"

Write-Host "Applying AccessLocation column migration to Azure SQL..." -ForegroundColor Cyan

# Check if SQL file exists
if (-not (Test-Path $sqlFile)) {
    Write-Host "Error: SQL file '$sqlFile' not found!" -ForegroundColor Red
    exit 1
}

# Try using sqlcmd if available
$sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue

if ($sqlcmdPath) {
    Write-Host "Using sqlcmd to apply migration..." -ForegroundColor Yellow
    Write-Host "You may be prompted for authentication..." -ForegroundColor Yellow
    
    sqlcmd -S $serverName -d $databaseName -G -i $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nMigration applied successfully!" -ForegroundColor Green
    } else {
        Write-Host "`nMigration failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "sqlcmd not found. Please install SQL Server tools or run the migration manually:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Install sqlcmd from: https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Option 2: Run the SQL script manually using Azure Data Studio or SSMS:" -ForegroundColor Cyan
    Write-Host "  - Server: $serverName" -ForegroundColor White
    Write-Host "  - Database: $databaseName" -ForegroundColor White
    Write-Host "  - Script: $sqlFile" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 3: Run via Azure Portal Query Editor" -ForegroundColor Cyan
    
    # Display the SQL content
    Write-Host "`nSQL Script Content:" -ForegroundColor Yellow
    Write-Host "===================" -ForegroundColor Yellow
    Get-Content $sqlFile | Write-Host -ForegroundColor White
}
