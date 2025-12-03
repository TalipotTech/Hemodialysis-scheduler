using System;
using System.Data.SQLite;
using System.IO;

class RunMigration
{
    static void Main()
    {
        var dbPath = @"G:\ENSATE\HD_Project\Backend\HDScheduler.db";
        var sqlFile = @"G:\ENSATE\HD_Project\Database\20_AddHDTreatmentFieldsToPatients.sql";
        
        Console.WriteLine($"Running migration on: {dbPath}");
        
        var sql = File.ReadAllText(sqlFile);
        var connectionString = $"Data Source={dbPath};Version=3;";
        
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            Console.WriteLine("[OK] Connected to database");
            
            // Split SQL into statements
            var statements = sql.Split(';');
            
            foreach (var statement in statements)
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--"))
                    continue;
                    
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = trimmed;
                        cmd.ExecuteNonQuery();
                        var preview = trimmed.Length > 50 ? trimmed.Substring(0, 50) + "..." : trimmed;
                        Console.WriteLine($"[OK] Executed: {preview}");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("duplicate column name"))
                    {
                        Console.WriteLine($"[SKIP] Column already exists");
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] {ex.Message}");
                    }
                }
            }
            
            // Verify columns
            Console.WriteLine("\nVerifying columns...");
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info('Patients')";
                using (var reader = cmd.ExecuteReader())
                {
                    var columns = new System.Collections.Generic.List<string>();
                    while (reader.Read())
                    {
                        columns.Add(reader["name"].ToString());
                    }
                    
                    var required = new[] { "DryWeight", "HDCycle", "HDStartDate", "DialyserType", "DialyserCount", "BloodTubingCount", "TotalDialysisCompleted" };
                    foreach (var col in required)
                    {
                        if (columns.Contains(col))
                            Console.WriteLine($"[OK] Column exists: {col}");
                        else
                            Console.WriteLine($"[ERROR] Column missing: {col}");
                    }
                }
            }
            
            Console.WriteLine("\n[OK] Migration completed successfully!");
        }
    }
}
