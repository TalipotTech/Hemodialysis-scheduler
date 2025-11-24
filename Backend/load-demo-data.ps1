# Load Demo Data into SQLite Database
$ErrorActionPreference = "Stop"

$dbPath = "HDScheduler.db"
$sqlFile = "..\LOAD_DEMO_DATA.sql"

if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Database not found at $dbPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $sqlFile)) {
    Write-Host "ERROR: SQL file not found at $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "Loading demo data into database..." -ForegroundColor Cyan

# Read SQL file
$sqlContent = Get-Content $sqlFile -Raw

# Split into individual statements
$statements = $sqlContent -split ';' | Where-Object { 
    $_.Trim() -and 
    $_.Trim() -notmatch '^\s*--' -and 
    $_.Trim() -notmatch '^\s*$'
}

Write-Host "Found $($statements.Count) SQL statements to execute" -ForegroundColor Yellow

# Load System.Data.Common
Add-Type -AssemblyName "System.Data.Common"

# Use Microsoft.Data.Sqlite from the project
$dllPath = ".\bin\Debug\net8.0\Microsoft.Data.Sqlite.dll"
if (Test-Path $dllPath) {
    Add-Type -Path $dllPath
    Write-Host "Loaded Microsoft.Data.Sqlite.dll" -ForegroundColor Green
} else {
    Write-Host "Using .NET built-in SQLite support" -ForegroundColor Yellow
}

try {
    # Create connection using the DLL from the project
    $connectionString = "Data Source=$dbPath"
    
    # Try to use reflection to create connection
    $assembly = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $dllPath))
    $connectionType = $assembly.GetType("Microsoft.Data.Sqlite.SqliteConnection")
    $connection = [Activator]::CreateInstance($connectionType, $connectionString)
    $connection.Open()
    
    Write-Host "Connected to database successfully!" -ForegroundColor Green
    
    $successCount = 0
    $errorCount = 0
    
    foreach ($statement in $statements) {
        $trimmed = $statement.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        
        try {
            $commandType = $connectionType.Assembly.GetType("Microsoft.Data.Sqlite.SqliteCommand")
            $command = [Activator]::CreateInstance($commandType, $trimmed, $connection)
            
            if ($trimmed -match '^\s*SELECT') {
                $reader = $command.ExecuteReader()
                while ($reader.Read()) {
                    $output = ""
                    for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                        $output += "$($reader.GetName($i)): $($reader.GetValue($i))  "
                    }
                    Write-Host $output -ForegroundColor Cyan
                }
                $reader.Close()
            } else {
                $rows = $command.ExecuteNonQuery()
                $successCount++
                if ($rows -gt 0) {
                    Write-Host "[OK] Statement executed: $rows rows affected" -ForegroundColor Green
                }
            }
            $command.Dispose()
        }
        catch {
            $errorCount++
            $errorMsg = $_.Exception.Message
            # Ignore "duplicate column" errors (means migration already done)
            if ($errorMsg -match "duplicate column name") {
                Write-Host "[INFO] Column already exists, skipping..." -ForegroundColor Yellow
                $errorCount--
            } else {
                Write-Host "[ERROR] $errorMsg" -ForegroundColor Red
                $preview = $trimmed.Substring(0, [Math]::Min(80, $trimmed.Length))
                Write-Host "  Statement: $preview..." -ForegroundColor DarkGray
            }
        }
    }
    
    $connection.Close()
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Demo data loading completed!" -ForegroundColor Green
    Write-Host "  Success: $successCount statements" -ForegroundColor Green
    if ($errorCount -gt 0) {
        Write-Host "  Errors: $errorCount statements" -ForegroundColor Red
    }
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "`nNext step: Restart the backend to see the demo data!" -ForegroundColor Yellow
}
catch {
    Write-Host "FATAL ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}
