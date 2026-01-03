# Apply missed appointment migration
$migrationFile = ".\Database\add-missed-appointment-columns.sql"
$migrationSql = Get-Content $migrationFile -Raw

Write-Host "🔄 Applying missed appointment migration..." -ForegroundColor Cyan

# You can run this migration manually by copying the SQL from add-missed-appointment-columns.sql
# Or use your preferred SQL management tool

Write-Host "📋 Migration file location: $migrationFile" -ForegroundColor Green
Write-Host "✅ Please apply this migration to your database" -ForegroundColor Yellow
Write-Host ""
Write-Host "Migration SQL:" -ForegroundColor Magenta
Write-Host $migrationSql -ForegroundColor White
