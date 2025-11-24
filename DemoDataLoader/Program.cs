using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

var dbPath = "../HDScheduler.db";
var sqlFile = "../../LOAD_DEMO_DATA.sql";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"ERROR: Database not found at {dbPath}");
    return;
}

if (!File.Exists(sqlFile))
{
    Console.WriteLine($"ERROR: SQL file not found at {sqlFile}");
    return;
}

Console.WriteLine("Loading demo data into database...");

var sqlContent = File.ReadAllText(sqlFile);
var statements = sqlContent.Split(';')
    .Select(s => s.Trim())
    .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("--"))
    .ToList();

Console.WriteLine($"Found {statements.Count} SQL statements to execute\n");

var connectionString = $"Data Source={dbPath}";
int successCount = 0;
int errorCount = 0;

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
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i)}: {reader.GetValue(i)}  ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
                else
                {
                    var rows = command.ExecuteNonQuery();
                    successCount++;
                    if (rows > 0)
                    {
                        Console.WriteLine($"✓ {rows} rows affected");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Ignore duplicate column errors (migration already done)
            if (ex.Message.Contains("duplicate column name"))
            {
                Console.WriteLine("ℹ Column already exists, skipping...");
            }
            else
            {
                errorCount++;
                Console.WriteLine($"✗ Error: {ex.Message}");
                var preview = statement.Length > 80 ? statement.Substring(0, 80) : statement;
                Console.WriteLine($"  Statement: {preview}...");
            }
        }
    }
    
    connection.Close();
}

Console.WriteLine("\n========================================");
Console.WriteLine("✓ Demo data loading completed!");
Console.WriteLine($"  Success: {successCount} statements");
if (errorCount > 0)
    Console.WriteLine($"  Errors: {errorCount} statements");
Console.WriteLine("========================================");
Console.WriteLine("\nRefresh your frontend to see the demo data!");
