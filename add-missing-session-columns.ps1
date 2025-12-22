# PowerShell script to add missing columns to HDSchedule table
# This script connects to Azure SQL Server and adds the missing columns

$serverName = "hds-dev-sqlserver-cin.database.windows.net"
$databaseName = "hds-dev-db"
$userId = "hdsadmin"
$password = "Talipot@123"

$query = @"
-- Add missing columns to HDSchedule table for HD session data
-- Missing columns identified: PreBPSitting, StartTime, PreWeight, PreTemperature, AccessBleedingTime, AccessStatus, Complications

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreBPSitting')
BEGIN
    ALTER TABLE HDSchedule ADD PreBPSitting NVARCHAR(50) NULL;
    PRINT 'Added PreBPSitting column';
END
ELSE
BEGIN
    PRINT 'PreBPSitting column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'StartTime')
BEGIN
    ALTER TABLE HDSchedule ADD StartTime TIME NULL;
    PRINT 'Added StartTime column';
END
ELSE
BEGIN
    PRINT 'StartTime column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreWeight')
BEGIN
    ALTER TABLE HDSchedule ADD PreWeight DECIMAL(5,2) NULL;
    PRINT 'Added PreWeight column';
END
ELSE
BEGIN
    PRINT 'PreWeight column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'PreTemperature')
BEGIN
    ALTER TABLE HDSchedule ADD PreTemperature DECIMAL(4,1) NULL;
    PRINT 'Added PreTemperature column';
END
ELSE
BEGIN
    PRINT 'PreTemperature column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'AccessBleedingTime')
BEGIN
    ALTER TABLE HDSchedule ADD AccessBleedingTime INT NULL;
    PRINT 'Added AccessBleedingTime column';
END
ELSE
BEGIN
    PRINT 'AccessBleedingTime column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'AccessStatus')
BEGIN
    ALTER TABLE HDSchedule ADD AccessStatus NVARCHAR(50) NULL;
    PRINT 'Added AccessStatus column';
END
ELSE
BEGIN
    PRINT 'AccessStatus column already exists';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HDSchedule' AND COLUMN_NAME = 'Complications')
BEGIN
    ALTER TABLE HDSchedule ADD Complications NVARCHAR(MAX) NULL;
    PRINT 'Added Complications column';
END
ELSE
BEGIN
    PRINT 'Complications column already exists';
END

SELECT 'Migration completed successfully!' AS Result;
"@

# Execute the query
try {
    $connectionString = "Server=tcp:$serverName,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=$userId;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $command.CommandTimeout = 60
    
    Write-Host "Executing migration on $serverName/$databaseName..." -ForegroundColor Cyan
    $reader = $command.ExecuteReader()
    
    # Read all result sets
    do {
        while ($reader.Read()) {
            for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                Write-Host "$($reader.GetName($i)): $($reader.GetValue($i))" -ForegroundColor Green
            }
        }
    } while ($reader.NextResult())
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error executing migration: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
    exit 1
}
