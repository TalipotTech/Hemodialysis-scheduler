using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Backend", "HDScheduler.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine($"Database: {dbPath}");
Console.WriteLine("Running migrations...\n");

try
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        // Check which migrations are already applied
        bool hasHDCycle = CheckColumnExists(connection, "Patients", "HDCycle");
        bool hasIntraDialyticMonitoring = CheckTableExists(connection, "IntraDialyticMonitoring");
        bool hasHDFrequency = CheckColumnExists(connection, "Patients", "HDFrequency");

        // Migration 15: Add HDCycle column
        if (!hasHDCycle)
        {
            Console.WriteLine("Running migration 15: Add HDCycle column...");
            var migration15 = File.ReadAllText("..\\15_AddHDCycleColumn.sql");
            ExecuteMigration(connection, migration15);
            Console.WriteLine("✓ Migration 15 completed\n");
        }
        else
        {
            Console.WriteLine("⊘ Migration 15 already applied (HDCycle column exists)\n");
        }

        // Migration 16: Create IntraDialyticMonitoring table
        if (!hasIntraDialyticMonitoring)
        {
            Console.WriteLine("Running migration 16: Create IntraDialyticMonitoring table...");
            var migration16 = File.ReadAllText("..\\16_CreateIntraDialyticMonitoring.sql");
            ExecuteMigration(connection, migration16);
            Console.WriteLine("✓ Migration 16 completed\n");
        }
        else
        {
            Console.WriteLine("⊘ Migration 16 already applied (IntraDialyticMonitoring table exists)\n");
        }

        // Migration 17: Add HDFrequency column
        if (!hasHDFrequency)
        {
            Console.WriteLine("Running migration 17: Add HDFrequency column...");
            var migration17 = File.ReadAllText("..\\17_AddHDFrequencyColumn.sql");
            ExecuteMigration(connection, migration17);
            Console.WriteLine("✓ Migration 17 completed\n");
        }
        else
        {
            Console.WriteLine("⊘ Migration 17 already applied (HDFrequency column exists)\n");
        }

        // Verify all migrations
        Console.WriteLine("Verifying migrations...");
        VerifyColumn(connection, "Patients", "HDCycle");
        VerifyColumn(connection, "Patients", "HDFrequency");
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

void ExecuteMigration(SqliteConnection connection, string sql)
{
    // Remove comments and split by semicolons
    var lines = sql.Split('\n');
    var cleanedSql = string.Join('\n', lines.Where(line => !line.Trim().StartsWith("--")));
    var statements = cleanedSql.Split(';', StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var statement in statements)
    {
        var trimmed = statement.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = trimmed;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Statement: {trimmed.Substring(0, Math.Min(100, trimmed.Length))}...");
                throw;
            }
        }
    }
}

void VerifyColumn(SqliteConnection connection, string tableName, string columnName)
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

void VerifyTable(SqliteConnection connection, string tableName)
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

bool CheckColumnExists(SqliteConnection connection, string tableName, string columnName)
{
    using (var command = connection.CreateCommand())
    {
        command.CommandText = $"PRAGMA table_info({tableName})";
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (reader.GetString(1) == columnName)
                {
                    return true;
                }
            }
        }
    }
    return false;
}

bool CheckTableExists(SqliteConnection connection, string tableName)
{
    using (var command = connection.CreateCommand())
    {
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        var result = command.ExecuteScalar();
        return result != null;
    }
}
