Write-Host "`n=== HD Treatment Fields Migration ===" -ForegroundColor Cyan

$dbPath = "HDScheduler.db"
if (!(Test-Path $dbPath)) {
    Write-Host "❌ Database not found" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Database found: $dbPath`n" -ForegroundColor Green

# Check for sqlite3
$sqlite3Path = Get-Command sqlite3 -ErrorAction SilentlyContinue

if (!$sqlite3Path) {
    Write-Host "❌ sqlite3.exe not found in PATH`n" -ForegroundColor Red
    Write-Host "Please download SQLite tools from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    Write-Host "Or use DB Browser for SQLite to run: ..\Database\20_AddHDTreatmentFieldsToPatients.sql" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Using sqlite3.exe`n" -ForegroundColor Green

# Execute each ALTER TABLE statement
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
    $result = & sqlite3 $dbPath $sqls[$i] 2>&1
    if ($?) {
        Write-Host "✓ Added: $($columns[$i])" -ForegroundColor Green
    } elseif ($result -like "*duplicate*") {
        Write-Host "  ⏭️  Already exists: $($columns[$i])" -ForegroundColor Yellow
    } else {
        Write-Host "❌ Error: $($columns[$i]) - $result" -ForegroundColor Red
    }
}

Write-Host "`n✅ Migration completed!" -ForegroundColor Green
