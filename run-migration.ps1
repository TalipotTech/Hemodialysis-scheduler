# PowerShell script to run the HD Treatment Fields migration

$dbPath = "G:\ENSATE\HD_Project\Backend\HDScheduler.db"
$sqlFile = "G:\ENSATE\HD_Project\Database\20_AddHDTreatmentFieldsToPatients.sql"

Write-Host "Running migration on database: $dbPath" -ForegroundColor Cyan

# Load SQLite assembly from NuGet package
$sqliteAssembly = Get-ChildItem -Path "G:\ENSATE\HD_Project\Backend\bin" -Recurse -Filter "System.Data.SQLite.dll" | Select-Object -First 1

if ($sqliteAssembly) {
    Add-Type -Path $sqliteAssembly.FullName
    Write-Host "[OK] Loaded SQLite assembly" -ForegroundColor Green
} else {
    Write-Host "[ERROR] SQLite assembly not found. Please build the project first." -ForegroundColor Red
    exit 1
}

try {
    # Read SQL migration script
    $sql = Get-Content $sqlFile -Raw
    
    # Split into individual statements
    $statements = $sql -split ';' | Where-Object { $_.Trim() -ne '' -and $_.Trim() -notmatch '^--' }
    
    # Connect to database
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    Write-Host "[OK] Connected to database" -ForegroundColor Green
    
    # Execute each statement
    foreach ($statement in $statements) {
        $trimmedStatement = $statement.Trim()
        if ($trimmedStatement -ne '') {
            try {
                $command = $connection.CreateCommand()
                $command.CommandText = $trimmedStatement
                $command.ExecuteNonQuery() | Out-Null
                Write-Host "[OK] Executed: $($trimmedStatement.Substring(0, [Math]::Min(50, $trimmedStatement.Length)))..." -ForegroundColor Green
            }
            catch {
                if ($_.Exception.Message -match "duplicate column name") {
                    Write-Host "[SKIP] Column already exists: $($trimmedStatement.Substring(0, [Math]::Min(50, $trimmedStatement.Length)))..." -ForegroundColor Yellow
                }
                else {
                    Write-Host "[ERROR] Error: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    }
    
    # Verify columns exist
    Write-Host "" 
    Write-Host "Verifying columns..." -ForegroundColor Cyan
    $command = $connection.CreateCommand()
    $command.CommandText = "PRAGMA table_info('Patients')"
    $reader = $command.ExecuteReader()
    
    $columns = @()
    while ($reader.Read()) {
        $columns += $reader["name"]
    }
    $reader.Close()
    
    $requiredColumns = @('DryWeight', 'HDCycle', 'HDStartDate', 'DialyserType', 'DialyserCount', 'BloodTubingCount', 'TotalDialysisCompleted')
    foreach ($col in $requiredColumns) {
        if ($columns -contains $col) {
            Write-Host "[OK] Column exists: $col" -ForegroundColor Green
        }
        else {
            Write-Host "[ERROR] Column missing: $col" -ForegroundColor Red
        }
    }
    
    $connection.Close()
    Write-Host ""
    Write-Host "[OK] Migration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
    exit 1
}
