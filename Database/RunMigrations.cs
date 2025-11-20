using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

class RunMigrations
{
    static void Main(string[] args)
    {
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Backend", "HDScheduler.db");
        var connectionString = $"Data Source={dbPath}";

        Console.WriteLine($"Database: {dbPath}");
        Console.WriteLine("Running migrations...\n");

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Migration 15: Add HDCycle column
                Console.WriteLine("Running migration 15: Add HDCycle column...");
                var migration15 = File.ReadAllText("15_AddHDCycleColumn.sql");
                ExecuteMigration(connection, migration15);
                Console.WriteLine("✓ Migration 15 completed\n");

                // Migration 16: Create IntraDialyticMonitoring table
                Console.WriteLine("Running migration 16: Create IntraDialyticMonitoring table...");
                var migration16 = File.ReadAllText("16_CreateIntraDialyticMonitoring.sql");
                ExecuteMigration(connection, migration16);
                Console.WriteLine("✓ Migration 16 completed\n");

                // Verify migrations
                Console.WriteLine("Verifying migrations...");
                VerifyColumn(connection, "Patients", "HDCycle");
                VerifyTable(connection, "IntraDialyticMonitoring");

                Console.WriteLine("\n✓ All migrations completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    static void ExecuteMigration(SqliteConnection connection, string sql)
    {
        // Split by GO statements or semicolons and execute each batch
        var statements = sql.Split(new[] { "GO", ";" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var statement in statements)
        {
            var trimmed = statement.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("--"))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = trimmed;
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    static void VerifyColumn(SqliteConnection connection, string tableName, string columnName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"PRAGMA table_info({tableName})";
            using (var reader = command.ExecuteReader())
            {
                bool found = false;
                while (reader.Read())
                {
                    if (reader.GetString(1) == columnName)
                    {
                        found = true;
                        Console.WriteLine($"  ✓ Column '{columnName}' exists in table '{tableName}'");
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception($"Column '{columnName}' not found in table '{tableName}'");
                }
            }
        }
    }

    static void VerifyTable(SqliteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            var result = command.ExecuteScalar();
            if (result == null)
            {
                throw new Exception($"Table '{tableName}' not found");
            }
            Console.WriteLine($"  ✓ Table '{tableName}' exists");
        }
    }
}
