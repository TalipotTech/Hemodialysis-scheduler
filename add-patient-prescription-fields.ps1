# PowerShell script to add missing prescription fields to Patients table
Write-Host "Adding prescription fields to Patients table..." -ForegroundColor Cyan

$dbPath = "G:\ENSATE\HD_Project\Backend\HDScheduler.db"

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "Database not found at: $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "Database found at: $dbPath" -ForegroundColor Green

# SQL commands to add the missing columns
$sqlCommands = @"
-- Add DialyserModel column if it doesn't exist
ALTER TABLE Patients ADD COLUMN DialyserModel TEXT;

-- Add PrescribedDuration column if it doesn't exist
ALTER TABLE Patients ADD COLUMN PrescribedDuration REAL;

-- Add PrescribedBFR column if it doesn't exist
ALTER TABLE Patients ADD COLUMN PrescribedBFR INTEGER;

-- Add DialysatePrescription column if it doesn't exist
ALTER TABLE Patients ADD COLUMN DialysatePrescription TEXT;
"@

# Execute SQL commands
try {
    # Use sqlite3 command if available, otherwise use System.Data.SQLite
    $sqliteExe = Get-Command sqlite3 -ErrorAction SilentlyContinue
    
    if ($sqliteExe) {
        Write-Host "Using sqlite3 command line tool..." -ForegroundColor Yellow
        $sqlCommands | sqlite3 $dbPath
    } else {
        Write-Host "Using .NET SQLite library..." -ForegroundColor Yellow
        
        # Load SQLite assembly
        Add-Type -Path "System.Data.SQLite.dll" -ErrorAction SilentlyContinue
        
        $connectionString = "Data Source=$dbPath;Version=3;"
        $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
        $connection.Open()
        
        foreach ($sql in ($sqlCommands -split ';')) {
            $sql = $sql.Trim()
            if ($sql) {
                try {
                    $command = $connection.CreateCommand()
                    $command.CommandText = $sql
                    $command.ExecuteNonQuery() | Out-Null
                    Write-Host "Executed: $($sql.Substring(0, [Math]::Min(50, $sql.Length)))..." -ForegroundColor Gray
                } catch {
                    if ($_.Exception.Message -like "*duplicate column name*") {
                        Write-Host "Column already exists, skipping..." -ForegroundColor Yellow
                    } else {
                        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
                    }
                }
            }
        }
        
        $connection.Close()
    }
    
    Write-Host "`nColumns added successfully!" -ForegroundColor Green
    Write-Host "The following fields are now available in the Patients table:" -ForegroundColor Cyan
    Write-Host "  - DialyserModel (TEXT)" -ForegroundColor White
    Write-Host "  - PrescribedDuration (REAL)" -ForegroundColor White
    Write-Host "  - PrescribedBFR (INTEGER)" -ForegroundColor White
    Write-Host "  - DialysatePrescription (TEXT)" -ForegroundColor White
    
} catch {
    Write-Host "Error executing SQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nMigration completed!" -ForegroundColor Green
