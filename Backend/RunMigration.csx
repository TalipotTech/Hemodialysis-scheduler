using System;
using System.Data.SQLite;

// Simple migration runner
var dbPath = "HDScheduler.db";
var connectionString = $"Data Source={dbPath};Version=3;";

Console.WriteLine("\n=== HD Treatment Fields Migration ===\n");

if (!System.IO.File.Exists(dbPath))
{
    Console.WriteLine($"ERROR: Database not found: {dbPath}");
    return 1;
}

Console.WriteLine($"Database: {dbPath}\n");

using var connection = new SQLiteConnection(connectionString);
connection.Open();

var migrations = new (string Name, string SQL)[]
{
    ("DryWeight", "ALTER TABLE Patients ADD COLUMN DryWeight REAL;"),
    ("HDCycle", "ALTER TABLE Patients ADD COLUMN HDCycle TEXT;"),
    ("HDStartDate", "ALTER TABLE Patients ADD COLUMN HDStartDate TEXT;"),
    ("DialyserType", "ALTER TABLE Patients ADD COLUMN DialyserType TEXT;"),
    ("DialyserCount", "ALTER TABLE Patients ADD COLUMN DialyserCount INTEGER DEFAULT 0;"),
    ("BloodTubingCount", "ALTER TABLE Patients ADD COLUMN BloodTubingCount INTEGER DEFAULT 0;"),
    ("TotalDialysisCompleted", "ALTER TABLE Patients ADD COLUMN TotalDialysisCompleted INTEGER DEFAULT 0;")
};

int added = 0, skipped = 0;

foreach (var (name, sql) in migrations)
{
    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
        Console.WriteLine($"[OK] Added: {name}");
        added++;
    }
    catch (SQLiteException ex) when (ex.Message.Contains("duplicate column"))
    {
        Console.WriteLine($"[SKIP] Already exists: {name}");
        skipped++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {name}: {ex.Message}");
    }
}

Console.WriteLine($"\n=== Results ===");
Console.WriteLine($"Added: {added}, Skipped: {skipped}");
Console.WriteLine("\nMigration completed!\n");

return 0;
