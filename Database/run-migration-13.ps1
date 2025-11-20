# Run Migration 13: Add Bed Usage Tracking
Write-Host "Running Migration 13: Bed Usage Tracking System..." -ForegroundColor Cyan

$dbPath = "G:\ENSATE\HD_Project\Backend\hd_scheduler.db"
$sqlPath = "G:\ENSATE\HD_Project\Database\13_AddBedUsageTracking.sql"

# Read SQL
$sql = Get-Content $sqlPath -Raw

# Create a temporary C# file
$tempCsFile = [System.IO.Path]::GetTempFileName() + ".cs"

$csharpCode = @"
using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        try
        {
            string connectionString = "Data Source=$dbPath";
            string sql = File.ReadAllText(@"$sqlPath");
            
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            
            Console.WriteLine("SUCCESS");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Environment.Exit(1);
        }
    }
}
"@

$csharpCode | Out-File -FilePath $tempCsFile -Encoding UTF8

# Compile and run
$dllPath = "G:\ENSATE\HD_Project\Backend\bin\Debug\net8.0\System.Data.SQLite.dll"
$output = & csc /reference:$dllPath /out:"$tempCsFile.exe" $tempCsFile 2>&1

if ($LASTEXITCODE -eq 0) {
    $result = & "$tempCsFile.exe"
    if ($result -like "*SUCCESS*") {
        Write-Host "✓ Migration 13 completed successfully!" -ForegroundColor Green
        Write-Host "  - BedUsageTracking table created" -ForegroundColor Gray
        Write-Host "  - Auto-update triggers added" -ForegroundColor Gray
        Write-Host "  - 20 beds initialized" -ForegroundColor Gray
    } else {
        Write-Host "✗ Migration failed: $result" -ForegroundColor Red
    }
    Remove-Item $tempCsFile -ErrorAction SilentlyContinue
    Remove-Item "$tempCsFile.exe" -ErrorAction SilentlyContinue
} else {
    Write-Host "✗ Compilation failed. Using inline approach..." -ForegroundColor Yellow
    
    # Fallback: Try direct execution
    Add-Type -Path $dllPath -ErrorAction Stop
    $conn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath")
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "✓ Migration 13 completed via fallback method!" -ForegroundColor Green
}
