using System;
using Microsoft.Data.Sqlite;

namespace HDScheduler.API.Migrations;

class AddPostDialysisColumns
{
    static void Main(string[] args)
    {
        var connectionString = "Data Source=HDScheduler.db";
        
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var columns = new[]
        {
            ("PostWeight", "REAL"),
            ("PostSBP", "INTEGER"),
            ("PostDBP", "INTEGER"),
            ("PostHR", "INTEGER"),
            ("PostAccessStatus", "TEXT"),
            ("TotalFluidRemoved", "REAL"),
            ("Notes", "TEXT")
        };

        foreach (var (columnName, columnType) in columns)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = $"ALTER TABLE HDSchedule ADD COLUMN {columnName} {columnType};";
                command.ExecuteNonQuery();
                Console.WriteLine($"✓ Added column: {columnName}");
            }
            catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
            {
                Console.WriteLine($"→ Column already exists: {columnName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error adding {columnName}: {ex.Message}");
            }
        }

        Console.WriteLine("\nMigration completed!");
    }
}
