# PowerShell script to apply PatientActivityLog migration
$scriptPath = "add-patient-activity-log.sql"
$dbPath = "Backend/Data/hd_scheduler.db"

Write-Host "🔄 Applying PatientActivityLog migration..." -ForegroundColor Cyan

if (Test-Path $dbPath) {
    try {
        # Using sqlite3 CLI if available
        if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
            sqlite3 $dbPath ".read $scriptPath"
            Write-Host "✅ PatientActivityLog table created successfully!" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️ sqlite3 CLI not found. Please run migration manually or through backend." -ForegroundColor Yellow
            Write-Host "📋 SQL script ready at: $scriptPath" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Error "❌ Failed to apply migration: $_"
    }
}
else {
    Write-Host "⚠️ Database not found at $dbPath" -ForegroundColor Yellow
    Write-Host "📋 Migration will be applied when backend starts" -ForegroundColor Cyan
}
