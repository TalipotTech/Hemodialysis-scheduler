using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        var connectionString = "Data Source=../Backend/hdscheduler.db";
        var migrationSql = File.ReadAllText("08_AddScheduleIDToHDLogs.sql");

        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            // Split by semicolon and execute each statement
            var statements = migrationSql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var statement in statements)
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--"))
                    continue;
                    
                Console.WriteLine($"Executing: {trimmed.Substring(0, Math.Min(80, trimmed.Length))}...");
                
                using var command = new SQLiteCommand(trimmed, connection, transaction);
                command.ExecuteNonQuery();
            }
            
            transaction.Commit();
            Console.WriteLine("\nMigration completed successfully!");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"\nMigration failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }
}
