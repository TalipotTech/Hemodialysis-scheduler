# PowerShell script to add post-dialysis columns to HDSchedule table
$dbPath = "../Backend/HDScheduler.db"

# Load SQLite assembly
Add-Type -Path "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.SqlClient.dll" -ErrorAction SilentlyContinue

$columns = @(
    @{Name="PostWeight"; Type="REAL"},
    @{Name="PostSBP"; Type="INTEGER"},
    @{Name="PostDBP"; Type="INTEGER"},
    @{Name="PostHR"; Type="INTEGER"},
    @{Name="PostAccessStatus"; Type="TEXT"},
    @{Name="TotalFluidRemoved"; Type="REAL"},
    @{Name="Notes"; Type="TEXT"}
)

Write-Host "Adding post-dialysis columns to HDSchedule table..." -ForegroundColor Cyan

foreach ($column in $columns) {
    $sql = "ALTER TABLE HDSchedule ADD COLUMN $($column.Name) $($column.Type);"
    Write-Host "Executing: $sql" -ForegroundColor Yellow
    
    try {
        # Using System.Data.SQLite if available
        $connectionString = "Data Source=$dbPath;Version=3;"
        $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $command.ExecuteNonQuery() | Out-Null
        
        $connection.Close()
        Write-Host "✓ Added column: $($column.Name)" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Message -like "*duplicate column*") {
            Write-Host "→ Column already exists: $($column.Name)" -ForegroundColor Gray
        }
        else {
            Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nMigration completed!" -ForegroundColor Green
