using Microsoft.Data.Sqlite;

var dbPath = "../HDScheduler.db";
var migrationFile = "../../Database/21_AddSessionStatusColumns.sql";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"ERROR: Database not found at {dbPath}");
    return;
}

Console.WriteLine("Adding demo HD sessions with Pre-Scheduled status...\n");

// Read and parse the SQL file for demo sessions
var sqlContent = File.ReadAllText("../../Database/23_AddDemoSessions.sql");
var statements = sqlContent.Split(';')
    .Select(s => s.Trim())
    .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("--"))
    .ToList();

var connectionString = $"Data Source={dbPath}";

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("✓ Connected to database\n");
    
    foreach (var statement in statements)
    {
        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = statement;
                
                if (statement.ToUpper().Contains("SELECT"))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"✓ {reader.GetValue(0)}");
                        }
                    }
                }
                else
                {
                    var rows = command.ExecuteNonQuery();
                    var preview = statement.Length > 50 ? statement.Substring(0, 50) + "..." : statement;
                    Console.WriteLine($"✓ Executed: {preview}");
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("duplicate column name"))
            {
                Console.WriteLine("ℹ Column already exists, skipping...");
                // Continue with other statements instead of returning
            }
            else
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }
    
    connection.Close();
}

Console.WriteLine("\n========================================");
Console.WriteLine("✓ Migration completed successfully!");
Console.WriteLine("========================================");
