# Run Migration 11: Equipment Usage Alerts
# This script adds the EquipmentUsageAlerts table to track dialyser and blood tubing usage

$ErrorActionPreference = "Stop"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Equipment Usage Alerts Migration" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Get the database path from appsettings
$appsettingsPath = ".\Backend\appsettings.json"
if (-Not (Test-Path $appsettingsPath)) {
    Write-Host "Error: appsettings.json not found at $appsettingsPath" -ForegroundColor Red
    exit 1
}

$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

# Extract database path from connection string
if ($connectionString -match "Data Source=([^;]+)") {
    $dbPath = $matches[1]
    Write-Host "Database: $dbPath" -ForegroundColor Yellow
} else {
    Write-Host "Error: Could not extract database path from connection string" -ForegroundColor Red
    exit 1
}

# Check if database exists
if (-Not (Test-Path $dbPath)) {
    Write-Host "Error: Database file not found at $dbPath" -ForegroundColor Red
    exit 1
}

# Run migration
$migrationFile = ".\Database\11_AddEquipmentUsageAlerts.sql"
if (-Not (Test-Path $migrationFile)) {
    Write-Host "Error: Migration file not found at $migrationFile" -ForegroundColor Red
    exit 1
}

Write-Host "`nRunning migration: 11_AddEquipmentUsageAlerts.sql" -ForegroundColor Yellow

try {
    # Read and execute SQL
    $sql = Get-Content $migrationFile -Raw
    
    # Use sqlite3 command if available, otherwise use .NET
    $sqlite3 = Get-Command sqlite3 -ErrorAction SilentlyContinue
    
    if ($sqlite3) {
        Write-Host "Using sqlite3 CLI..." -ForegroundColor Green
        $sql | sqlite3 $dbPath
    } else {
        Write-Host "Using System.Data.SQLite..." -ForegroundColor Green
        
        # Load SQLite assembly
        Add-Type -Path "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll" -ErrorAction SilentlyContinue
        
        # Create connection
        $connection = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath")
        $connection.Open()
        
        # Execute SQL
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $command.ExecuteNonQuery() | Out-Null
        
        $connection.Close()
    }
    
    Write-Host "`n✓ Migration completed successfully!" -ForegroundColor Green
    Write-Host "`nEquipment Usage Alerts table created with:" -ForegroundColor Cyan
    Write-Host "  - Dialyser Max Usage: 7 times" -ForegroundColor White
    Write-Host "  - Blood Tubing Max Usage: 12 times" -ForegroundColor White
    Write-Host "  - Automatic alert generation for Warning/Critical/Expired status" -ForegroundColor White
    
} catch {
    Write-Host "`nError running migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`n====================================" -ForegroundColor Cyan
Write-Host "Migration Complete!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Cyan
