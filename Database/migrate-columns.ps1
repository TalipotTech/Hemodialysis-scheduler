$dbPath = "g:\ENSATE\HD_Project\Backend\HDScheduler.db"

Write-Host "Starting database migration..." -ForegroundColor Cyan
Write-Host "Database: $dbPath" -ForegroundColor Yellow

# Load System.Data.SQLite from NuGet package
$sqliteDll = Get-ChildItem -Path "g:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\" -Filter "System.Data.SQLite.dll" -Recurse | Select-Object -First 1

if (-not $sqliteDll) {
    Write-Host "Error: Could not find System.Data.SQLite.dll" -ForegroundColor Red
    exit
}

Add-Type -Path $sqliteDll.FullName
Write-Host "Loaded SQLite library`n" -ForegroundColor Green

$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

$connection.Open()
Write-Host "Connected to database`n" -ForegroundColor Green

$columns = @(
    @("DialyserModel", "TEXT"),
    @("AccessLocation", "TEXT"),
    @("StartTime", "TEXT"),
    @("PreWeight", "REAL"),
    @("PreBPSitting", "TEXT"),
    @("PreTemperature", "REAL"),
    @("AccessBleedingTime", "TEXT"),
    @("AccessStatus", "TEXT"),
    @("Complications", "TEXT"),
    @("MonitoringTime", "TEXT"),
    @("HeartRate", "INTEGER"),
    @("ActualBFR", "INTEGER"),
    @("VenousPressure", "INTEGER"),
    @("ArterialPressure", "INTEGER"),
    @("CurrentUFR", "REAL"),
    @("TotalUFAchieved", "REAL"),
    @("TmpPressure", "INTEGER"),
    @("Interventions", "TEXT"),
    @("StaffInitials", "TEXT"),
    @("MedicationType", "TEXT"),
    @("MedicationName", "TEXT"),
    @("Dose", "TEXT"),
    @("Route", "TEXT"),
    @("AdministeredAt", "TEXT"),
    @("AlertType", "TEXT"),
    @("AlertMessage", "TEXT"),
    @("Severity", "TEXT"),
    @("Resolution", "TEXT")
)

$successCount = 0
$skipCount = 0

foreach ($col in $columns) {
    $colName = $col[0]
    $colType = $col[1]
    $sql = "ALTER TABLE HDSchedule ADD COLUMN $colName $colType;"
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    
    try {
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "✓ Added column: $colName" -ForegroundColor Green
        $successCount++
    }
    catch {
        if ($_.Exception.Message -like "*duplicate column*") {
            Write-Host "⊙ Column already exists: $colName" -ForegroundColor Yellow
            $skipCount++
        }
        else {
            Write-Host "✗ Error with $colName : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

$connection.Close()

Write-Host "`n========================================"  -ForegroundColor Cyan
Write-Host "Migration Summary:" -ForegroundColor Cyan
Write-Host "  Columns Added: $successCount" -ForegroundColor Green
Write-Host "  Already Existed: $skipCount" -ForegroundColor Yellow
Write-Host "  Total: $($columns.Count)" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
Write-Host "✓ Migration completed!" -ForegroundColor Green
