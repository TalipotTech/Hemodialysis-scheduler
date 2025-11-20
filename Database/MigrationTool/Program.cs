using System.Data.SQLite;

class Program
{
    static void Main(string[] args)
    {
        string dbPath = @"..\..\Backend\HDScheduler.db";
        string connectionString = $"Data Source={dbPath};Version=3;";
        
        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                
                // Check if column already exists
                string checkColumnQuery = "PRAGMA table_info(HDSchedule)";
                bool columnExists = false;
                
                using (var checkCmd = new SQLiteCommand(checkColumnQuery, connection))
                using (var reader = checkCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == "IsMovedToHistory")
                        {
                            columnExists = true;
                            break;
                        }
                    }
                }
                
                if (columnExists)
                {
                    Console.WriteLine("IsMovedToHistory column already exists. Skipping migration.");
                    return;
                }
                
                // Add IsMovedToHistory column
                Console.WriteLine("Adding IsMovedToHistory column to HDSchedule table...");
                string addColumnQuery = @"
                    ALTER TABLE HDSchedule 
                    ADD COLUMN IsMovedToHistory INTEGER DEFAULT 0 NOT NULL;
                ";
                
                using (var cmd = new SQLiteCommand(addColumnQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                
                // Create index
                Console.WriteLine("Creating index...");
                string createIndexQuery = @"
                    CREATE INDEX IF NOT EXISTS idx_hdschedule_history 
                    ON HDSchedule(IsMovedToHistory, SessionDate, SlotID);
                ";
                
                using (var cmd = new SQLiteCommand(createIndexQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                
                // Update existing records
                Console.WriteLine("Updating existing records...");
                string updateQuery = @"
                    UPDATE HDSchedule
                    SET IsMovedToHistory = 1
                    WHERE date(SessionDate) < date('now', '-1 day')
                      AND IsDischarged = 0;
                ";
                
                using (var cmd = new SQLiteCommand(updateQuery, connection))
                {
                    int affected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Updated {affected} existing records to history.");
                }
                
                // Verify the changes
                Console.WriteLine("\nVerifying changes...");
                string verifyQuery = @"
                    SELECT COUNT(*) as TotalRecords, 
                           SUM(CASE WHEN IsMovedToHistory = 1 THEN 1 ELSE 0 END) as HistoryRecords,
                           SUM(CASE WHEN IsMovedToHistory = 0 THEN 1 ELSE 0 END) as ActiveRecords
                    FROM HDSchedule;
                ";
                
                using (var cmd = new SQLiteCommand(verifyQuery, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Total Records: {reader["TotalRecords"]}");
                        Console.WriteLine($"History Records: {reader["HistoryRecords"]}");
                        Console.WriteLine($"Active Records: {reader["ActiveRecords"]}");
                    }
                }
                
                Console.WriteLine("\n✓ Migration completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error during migration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
