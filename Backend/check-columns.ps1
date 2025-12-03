# Check if the new columns exist in the Patients table
$dbPath = "G:\ENSATE\HD_Project\Backend\HDScheduler.db"

if (Test-Path $dbPath) {
    Write-Host "✓ Database found at: $dbPath"
    Write-Host "`nChecking Patients table schema..."
    
    # Use sqlite3 if available, otherwise create a simple C# checker
    $checkScript = @"
using Microsoft.Data.Sqlite;
using System;

var connectionString = "Data Source=HDScheduler.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = "PRAGMA table_info(Patients);";

Console.WriteLine("\nPatients Table Columns:");
Console.WriteLine("----------------------------------------");
using var reader = command.ExecuteReader();
while (reader.Read())
{
    var name = reader.GetString(1);
    var type = reader.GetString(2);
    Console.WriteLine($"  {name} ({type})");
}
Console.WriteLine("----------------------------------------");
"@
    
    $checkScript | Out-File -FilePath "check-schema.csx" -Encoding UTF8
    dotnet script check-schema.csx
} else {
    Write-Host "✗ Database not found at: $dbPath"
}
