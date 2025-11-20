using System;
using System.Data.SQLite;
using System.IO;

class MigrateDatabaseColumns
{
    static void Main()
    {
        var dbPath = @"g:\ENSATE\HD_Project\Backend\HDScheduler.db";
        var sqlPath = @"g:\ENSATE\HD_Project\Database\10_AddAdditionalHDScheduleColumns.sql";

        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"Database file not found: {dbPath}");
            return;
        }

        if (!File.Exists(sqlPath))
        {
            Console.WriteLine($"SQL file not found: {sqlPath}");
            return;
        }

        var connectionString = $"Data Source={dbPath};Version=3;";
        var sql = File.ReadAllText(sqlPath);

        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        Console.WriteLine($"Connected to database: {dbPath}");

        // Split SQL into individual statements
        var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var statement in statements)
        {
            var trimmedStatement = statement.Trim();
            if (string.IsNullOrWhiteSpace(trimmedStatement) || 
                trimmedStatement.StartsWith("--") ||
                trimmedStatement.StartsWith("SELECT") ||
                trimmedStatement.StartsWith("PRAGMA"))
            {
                continue;
            }

            try
            {
                using var command = new SQLiteCommand(trimmedStatement, connection);
                command.ExecuteNonQuery();
                
                // Extract column name from ALTER TABLE statement
                var columnName = ExtractColumnName(trimmedStatement);
                Console.WriteLine($"✓ Added column: {columnName}");
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("duplicate column"))
                {
                    var columnName = ExtractColumnName(trimmedStatement);
                    Console.WriteLine($"⊙ Column already exists: {columnName}");
                }
                else
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                }
            }
        }

        connection.Close();
        Console.WriteLine("\n✓ Migration completed successfully!");
    }

    static string ExtractColumnName(string alterStatement)
    {
        // Extract column name from "ALTER TABLE HDSchedule ADD COLUMN ColumnName TYPE"
        var parts = alterStatement.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 5)
        {
            return parts[4]; // Index 4 should be the column name
        }
        return "Unknown";
    }
}
