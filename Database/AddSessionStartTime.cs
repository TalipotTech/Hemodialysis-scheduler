using System;
using System.Data.SQLite;
using System.IO;

class AddSessionStartTimeColumn
{
    static void Main()
    {
        string dbPath = @"G:\ENSATE\HD_Project\Backend\hd_scheduler.db";
        string sqlFilePath = @"G:\ENSATE\HD_Project\Database\12_AddSessionStartTime.sql";
        
        try
        {
            string sql = File.ReadAllText(sqlFilePath);
            
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    int result = command.ExecuteNonQuery();
                    Console.WriteLine($"✓ Migration 12 completed successfully!");
                    Console.WriteLine($"  SessionStartTime column added to HDSchedule table");
                }
            }
        }
        catch (SQLiteException ex) when (ex.Message.Contains("duplicate column"))
        {
            Console.WriteLine("✓ SessionStartTime column already exists");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
