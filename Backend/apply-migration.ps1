Write-Host ""
Write-Host "=== HD Treatment Fields Migration ===" -ForegroundColor Cyan
Write-Host ""

$dbPath = "HDScheduler.db"
if (!(Test-Path $dbPath)) {
    Write-Host "ERROR: Database not found" -ForegroundColor Red
    exit 1
}

Write-Host "Database found: $dbPath" -ForegroundColor Green
Write-Host ""

# Check for sqlite3
$sqlite3Path = Get-Command sqlite3 -ErrorAction SilentlyContinue

if (!$sqlite3Path) {
    Write-Host "ERROR: sqlite3.exe not found in PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please download SQLite tools from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    Write-Host "Or use DB Browser for SQLite to manually run the migration SQL" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using sqlite3.exe" -ForegroundColor Green
Write-Host ""

# Execute migration
$columns = @("DryWeight", "HDCycle", "HDStartDate", "DialyserType", "DialyserCount", "BloodTubingCount", "TotalDialysisCompleted")
$sqls = @(
    "ALTER TABLE Patients ADD COLUMN DryWeight REAL;",
    "ALTER TABLE Patients ADD COLUMN HDCycle TEXT;",
    "ALTER TABLE Patients ADD COLUMN HDStartDate TEXT;",
    "ALTER TABLE Patients ADD COLUMN DialyserType TEXT;",
    "ALTER TABLE Patients ADD COLUMN DialyserCount INTEGER DEFAULT 0;",
    "ALTER TABLE Patients ADD COLUMN BloodTubingCount INTEGER DEFAULT 0;",
    "ALTER TABLE Patients ADD COLUMN TotalDialysisCompleted INTEGER DEFAULT 0;"
)

for ($i = 0; $i -lt $columns.Length; $i++) {
    $colName = $columns[$i]
    $sql = $sqls[$i]
    $result = & sqlite3 $dbPath $sql 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Added: $colName" -ForegroundColor Green
    } else {
        $errMsg = $result | Out-String
        if ($errMsg -like "*duplicate*") {
            Write-Host "[SKIP] Already exists: $colName" -ForegroundColor Yellow
        } else {
            Write-Host "[ERROR] $colName - $errMsg" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Migration process completed!" -ForegroundColor Green
