# Initialize Slots Configuration
# This script sets up default time slots with bed capacity

Write-Host "🔧 Initializing Slot Configuration..." -ForegroundColor Cyan

$dbPath = Join-Path $PSScriptRoot "Database\HDScheduler.db"
$sqlFile = Join-Path $PSScriptRoot "Database\initialize-slots.sql"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "Please run setup.ps1 first to create the database" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $sqlFile)) {
    Write-Host "❌ SQL file not found at: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "📂 Database: $dbPath" -ForegroundColor Gray
Write-Host "📄 SQL Script: $sqlFile" -ForegroundColor Gray

try {
    # Read SQL content
    $sqlContent = Get-Content $sqlFile -Raw
    
    # Execute using sqlite3
    Write-Host "`n⚙️  Executing SQL script..." -ForegroundColor Yellow
    
    $output = & sqlite3 $dbPath $sqlContent 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Slots initialized successfully!" -ForegroundColor Green
        Write-Host "`n📊 Slot Configuration:" -ForegroundColor Cyan
        Write-Host $output
    } else {
        Write-Host "❌ Error executing SQL script" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n✨ Slot initialization complete!" -ForegroundColor Green
Write-Host "You can now configure bed capacity in System Settings" -ForegroundColor Cyan
