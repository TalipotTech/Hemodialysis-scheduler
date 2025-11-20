# Run Migration 12: Add SessionStartTime column
$projectPath = "G:\ENSATE\HD_Project\Database\MigrationTool"
$sqlFile = "G:\ENSATE\HD_Project\Database\12_AddSessionStartTime.sql"
$dbPath = "G:\ENSATE\HD_Project\Backend\hd_scheduler.db"

Write-Host "Running Migration 12: Add SessionStartTime column..." -ForegroundColor Cyan

# Read SQL content
$sql = Get-Content $sqlFile -Raw

# Run using dotnet script approach
$code = @"
using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        string dbPath = @"$dbPath";
        string sql = @"$($sql -replace '"', '""')";
        
        using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                try
                {
                    int result = command.ExecuteNonQuery();
                    Console.WriteLine($"Migration 12 completed successfully! Rows affected: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(1);
                }
            }
        }
    }
}
"@

# Create temp C# file
$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
$code | Out-File -FilePath $tempFile -Encoding UTF8

# Compile and run
dotnet script $tempFile

# Cleanup
Remove-Item $tempFile -ErrorAction SilentlyContinue

Write-Host "Migration complete!" -ForegroundColor Green
