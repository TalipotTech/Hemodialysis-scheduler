# Migration 14: Add Session Phase Tracking
$dbPath = "G:\ENSATE\HD_Project\Backend\HDScheduler.db"

Write-Host "Running Migration 14..." -ForegroundColor Cyan

Add-Type -Path "C:\Program Files\PackageManagement\NuGet\Packages\System.Data.SQLite.Core.1.0.118\lib\net46\System.Data.SQLite.dll" -ErrorAction SilentlyContinue

$connectionString = "Data Source=$dbPath"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    $command = $connection.CreateCommand()
    
    $migrationSQL = Get-Content "14_AddSessionPhases.sql" -Raw
    $statements = $migrationSQL -split ";" | Where-Object { $_.Trim() -ne "" }
    
    foreach ($stmt in $statements) {
        $trimmed = $stmt.Trim()
        if ($trimmed -and -not $trimmed.StartsWith("--")) {
            try {
                $command.CommandText = $trimmed
                $command.ExecuteNonQuery() | Out-Null
            }
            catch {
                if ($_.Exception.Message -notlike "*duplicate column name*" -and 
                    $_.Exception.Message -notlike "*already exists*") {
                    Write-Host "Warning: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
        }
    }
    
    Write-Host "Migration 14 completed!" -ForegroundColor Green
}
catch {
    Write-Host "Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    throw
}
finally {
    $connection.Close()
}
