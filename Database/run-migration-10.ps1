$dbPath = "g:\ENSATE\HD_Project\Backend\HDScheduler.db"
$sqlPath = "g:\ENSATE\HD_Project\Database\10_AddAdditionalHDScheduleColumns.sql"

# Read the SQL migration script
$sql = Get-Content $sqlPath -Raw

# Load SQLite assembly
Add-Type -Path "System.Data.SQLite"

try {
    # Create connection
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected to database: $dbPath" -ForegroundColor Green
    
    # Split SQL into individual statements and execute each
    $statements = $sql -split ';' | Where-Object { $_.Trim() -ne '' }
    
    foreach ($statement in $statements) {
        $trimmedStatement = $statement.Trim()
        if ($trimmedStatement -ne '') {
            try {
                $command = $connection.CreateCommand()
                $command.CommandText = $trimmedStatement
                $result = $command.ExecuteNonQuery()
                Write-Host "Executed: $($trimmedStatement.Substring(0, [Math]::Min(50, $trimmedStatement.Length)))..." -ForegroundColor Cyan
            }
            catch {
                # Ignore "duplicate column" errors as columns may already exist
                if ($_.Exception.Message -notlike "*duplicate column*") {
                    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
        }
    }
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
    
    # Close connection
    $connection.Close()
}
catch {
    Write-Host "Database error: $($_.Exception.Message)" -ForegroundColor Red
}
