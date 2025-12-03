# Check database tables using simple approach
$dbPath = "G:\ENSATE\HD_Project\Backend\hd_scheduler.db"

Write-Host "`n=== Checking Database ===" -ForegroundColor Cyan
Write-Host "Database path: $dbPath`n"

if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Database file not found!" -ForegroundColor Red
    exit 1
}

# Use Data folder as backup
$dataDbPath = "G:\ENSATE\HD_Project\Backend\Data\hd_scheduler.db"
if (Test-Path $dataDbPath) {
    Write-Host "Also found database in Data folder: $dataDbPath`n"
}

Write-Host "Creating test C# program to check tables..." -ForegroundColor Yellow

$csharpCode = @'
using System;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        string dbPath = @"G:\ENSATE\HD_Project\Backend\hd_scheduler.db";
        using var conn = new SQLiteConnection($"Data Source={dbPath}");
        conn.Open();
        
        // Get all tables
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        using var reader = cmd.ExecuteReader();
        
        Console.WriteLine("\n=== EXISTING TABLES ===");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"  {++count}. {reader.GetString(0)}");
        }
        reader.Close();
        
        // Check IntraDialyticRecords columns
        cmd.CommandText = "PRAGMA table_info(IntraDialyticRecords);";
        using var colReader = cmd.ExecuteReader();
        
        Console.WriteLine("\n=== IntraDialyticRecords COLUMNS ===");
        while (colReader.Read())
        {
            Console.WriteLine($"  - {colReader.GetString(1)} ({colReader.GetString(2)})");
        }
    }
}
'@

Set-Content -Path "G:\ENSATE\HD_Project\check-db.cs" -Value $csharpCode
Write-Host "Compiling and running checker..."

cd "G:\ENSATE\HD_Project\Backend"
csc /r:System.Data.SQLite.dll /out:check-db.exe "G:\ENSATE\HD_Project\check-db.cs" 2>&1 | Out-Null

if (Test-Path "check-db.exe") {
    ./check-db.exe
    Remove-Item "check-db.exe" -Force
} else {
    Write-Host "Compilation failed, using dotnet approach..." -ForegroundColor Yellow
}
