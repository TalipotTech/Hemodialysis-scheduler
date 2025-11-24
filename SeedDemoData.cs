using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

class SeedDemoData
{
    static void Main(string[] args)
    {
        var connectionString = "Data Source=HDScheduler.db";
        var sqlScript = File.ReadAllText("../Database/22_SeedDemoData.sql");
        
        // Split by semicolons and execute each statement
        var statements = sqlScript.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            
            foreach (var statement in statements)
            {
                var trimmedStatement = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmedStatement) || trimmedStatement.StartsWith("--"))
                    continue;
                    
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = trimmedStatement;
                        
                        if (trimmedStatement.ToUpper().Contains("SELECT"))
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
                            var rowsAffected = command.ExecuteNonQuery();
                            Console.WriteLine($"✓ Executed statement, {rowsAffected} rows affected");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error executing statement: {ex.Message}");
                    Console.WriteLine($"Statement: {trimmedStatement.Substring(0, Math.Min(100, trimmedStatement.Length))}...");
                }
            }
            
            connection.Close();
        }
        
        Console.WriteLine("\n✅ Demo data seeding completed!");
    }
}
