$dbPath = "g:\ENSATE\HD_Project\Backend\HDScheduler.db"

# List of ALTER TABLE statements
$alterStatements = @(
    "ALTER TABLE HDSchedule ADD COLUMN DialyserModel TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN AccessLocation TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN StartTime TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN PreWeight REAL;",
    "ALTER TABLE HDSchedule ADD COLUMN PreBPSitting TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN PreTemperature REAL;",
    "ALTER TABLE HDSchedule ADD COLUMN AccessBleedingTime TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN AccessStatus TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN Complications TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN MonitoringTime TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN HeartRate INTEGER;",
    "ALTER TABLE HDSchedule ADD COLUMN ActualBFR INTEGER;",
    "ALTER TABLE HDSchedule ADD COLUMN VenousPressure INTEGER;",
    "ALTER TABLE HDSchedule ADD COLUMN ArterialPressure INTEGER;",
    "ALTER TABLE HDSchedule ADD COLUMN CurrentUFR REAL;",
    "ALTER TABLE HDSchedule ADD COLUMN TotalUFAchieved REAL;",
    "ALTER TABLE HDSchedule ADD COLUMN TmpPressure INTEGER;",
    "ALTER TABLE HDSchedule ADD COLUMN Interventions TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN StaffInitials TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN MedicationType TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN MedicationName TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN Dose TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN Route TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN AdministeredAt TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN AlertType TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN AlertMessage TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN Severity TEXT;",
    "ALTER TABLE HDSchedule ADD COLUMN Resolution TEXT;"
)

Write-Host "Starting database migration..." -ForegroundColor Cyan
Write-Host "Database: $dbPath`n" -ForegroundColor Yellow

# Load System.Data.SQLite from NuGet package
$sqliteDll = Get-ChildItem -Path "g:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\" -Filter "System.Data.SQLite.dll" -Recurse | Select-Object -First 1

if ($sqliteDll) {
    Add-Type -Path $sqliteDll.FullName
    Write-Host "Loaded SQLite library from: $($sqliteDll.FullName)`n" -ForegroundColor Green
    
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    
    try {
        $connection.Open()
        Write-Host "Connected to database successfully`n" -ForegroundColor Green
        
        $successCount = 0
        $skipCount = 0
        
        foreach ($sql in $alterStatements) {
            $command = $connection.CreateCommand()
            $command.CommandText = $sql
            
            try {
                $command.ExecuteNonQuery() | Out-Null
                
                # Extract column name
                $columnName = ($sql -split " ADD COLUMN ")[1] -split " " | Select-Object -First 1
                Write-Host "✓ Added column: $columnName" -ForegroundColor Green
                $successCount++
            }
            catch {
                if ($_.Exception.Message -like "*duplicate column*") {
                    $columnName = ($sql -split " ADD COLUMN ")[1] -split " " | Select-Object -First 1
                    Write-Host "⊙ Column already exists: $columnName" -ForegroundColor Yellow
                    $skipCount++
                }
                else {
                    Write-Host "✗ Error executing: $sql" -ForegroundColor Red
                    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
        
        $connection.Close()
        
        Write-Host "`n========================================" -ForegroundColor Cyan
        Write-Host "Migration Summary:" -ForegroundColor Cyan
        Write-Host "  Columns Added: $successCount" -ForegroundColor Green
        Write-Host "  Already Existed: $skipCount" -ForegroundColor Yellow
        Write-Host "  Total Statements: $($alterStatements.Count)" -ForegroundColor Cyan
        Write-Host "========================================`n" -ForegroundColor Cyan
        Write-Host "✓ Migration completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "Database connection error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
else {
    Write-Host "Error: Could not find System.Data.SQLite.dll in the Backend bin directory" -ForegroundColor Red
    Write-Host "Please build the Backend project first: cd Backend; dotnet build" -ForegroundColor Yellow
}
