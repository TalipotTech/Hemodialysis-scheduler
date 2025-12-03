# Run Equipment Purchase Tracking Migration
Write-Host "Running Equipment Purchase Tracking Migration..." -ForegroundColor Cyan

$migrationFile = "Migrations\20251202_AddEquipmentPurchaseTracking.sql"
$dbPath = "..\Database\HDScheduler.db"

if (Test-Path $migrationFile) {
    Write-Host "Found migration file: $migrationFile" -ForegroundColor Green
    
    # Load System.Data.SQLite
    Add-Type -Path "bin\Debug\net8.0\System.Data.SQLite.dll"
    
    # Read migration SQL
    $migrationSql = Get-Content $migrationFile -Raw
    
    Write-Host "Applying migration to database..." -ForegroundColor Yellow
    
    try {
        $connectionString = "Data Source=$dbPath;Version=3;"
        $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $migrationSql
        $command.ExecuteNonQuery() | Out-Null
        
        $connection.Close()
        
        Write-Host "Migration completed successfully!" -ForegroundColor Green
    } catch {
        Write-Host "Migration failed: $_" -ForegroundColor Red
    }
} else {
    Write-Host "Migration file not found: $migrationFile" -ForegroundColor Red
}
