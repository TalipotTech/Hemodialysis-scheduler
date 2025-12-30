# Initialize Hospital Configuration for Bed Naming
# Run this script to add the bed naming configuration to your database

Write-Host "🏥 Initializing Hospital Configuration..." -ForegroundColor Cyan

$dbPath = ".\Backend\hd-scheduler.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "Please ensure the database exists and the path is correct." -ForegroundColor Yellow
    exit 1
}

try {
    # Load SQLite assembly
    Add-Type -Path ".\Backend\bin\Debug\net8.0\Microsoft.Data.Sqlite.dll" -ErrorAction Stop
    
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    $connection.Open()

    Write-Host "Connected to database" -ForegroundColor Green

    # Create HospitalConfiguration table
    $createTableSql = @"
CREATE TABLE IF NOT EXISTS HospitalConfiguration (
    ConfigID INTEGER PRIMARY KEY AUTOINCREMENT,
    ConfigKey TEXT NOT NULL UNIQUE,
    ConfigValue TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now'))
);
"@

    $command = $connection.CreateCommand()
    $command.CommandText = $createTableSql
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "Successfully created HospitalConfiguration table" -ForegroundColor Green

    # Insert default configuration
    $insertSql = @"
INSERT OR REPLACE INTO HospitalConfiguration (ConfigKey, ConfigValue, Description) VALUES
('BedNamingPattern', 'NUMERIC', 'Bed naming format: NUMERIC, PREFIXED_NUMERIC, ALPHA_NUMERIC, ALPHABETIC, CUSTOM'),
('BedPrefix', 'Bed', 'Prefix for bed names (used with PREFIXED_NUMERIC or CUSTOM)'),
('BedsPerGroup', '5', 'Number of beds per letter group (used with ALPHA_NUMERIC pattern)'),
('BedCustomFormat', 'Bed {n}', 'Custom format string: {n}=number, {a}=letter, {g}=group');
"@

    $command.CommandText = $insertSql
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "Default bed naming configuration inserted" -ForegroundColor Green

    # Verify
    $selectCmd = $connection.CreateCommand()
    $selectCmd.CommandText = "SELECT ConfigKey, ConfigValue FROM HospitalConfiguration WHERE ConfigKey LIKE 'Bed%'"
    $reader = $selectCmd.ExecuteReader()

    Write-Host "`nCurrent Configuration:" -ForegroundColor Cyan
    while ($reader.Read()) {
        Write-Host "  $($reader.GetString(0)): $($reader.GetString(1))" -ForegroundColor White
    }
    $reader.Close()

    $connection.Close()

    Write-Host "`nInitialization complete!" -ForegroundColor Green
    Write-Host "Navigate to System Settings → Bed Naming to configure your preferred bed naming pattern" -ForegroundColor Yellow
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
