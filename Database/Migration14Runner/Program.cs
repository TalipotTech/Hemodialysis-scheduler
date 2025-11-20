using System;
using System.IO;
using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Backend", "HDScheduler.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Running Migration 14: Add Session Phase Tracking");
Console.WriteLine($"Database: {dbPath}\n");

try
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    // Read migration SQL
    var migrationPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "14_AddSessionPhases.sql");
    var migrationSQL = File.ReadAllText(migrationPath);

    // Execute migration
    using var command = connection.CreateCommand();
    command.CommandText = migrationSQL;
    
    try
    {
        command.ExecuteNonQuery();
        Console.WriteLine("✓ Migration 14 completed successfully!");
        Console.WriteLine("  - Added SessionPhase column");
        Console.WriteLine("  - Added phase timestamp columns");
        Console.WriteLine("  - Added lock flag columns");
        Console.WriteLine("  - Added post-dialysis fields");
        Console.WriteLine("  - Updated existing sessions");
    }
    catch (SqliteException ex)
    {
        if (ex.Message.Contains("duplicate column name"))
        {
            Console.WriteLine("⊘ Migration 14 already applied (columns already exist)");
        }
        else
        {
            throw;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Migration failed: {ex.Message}");
    return 1;
}

return 0;
