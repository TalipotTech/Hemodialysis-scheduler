using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        string dbPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "HDScheduler.db");
        string connectionString = $"Data Source={dbPath};Version=3;";
        
        // Delete existing database
        if (File.Exists(dbPath))
        {
            Console.WriteLine("Removing existing database...");
            File.Delete(dbPath);
        }
        
        Console.WriteLine("Creating SQLite database...");
        SQLiteConnection.CreateFile(dbPath);
        
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        
        // Read and execute schema
        string schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "SQLite_Schema.sql");
        string schema = File.ReadAllText(schemaPath);
        
        Console.WriteLine("Creating schema...");
        using (var command = new SQLiteCommand(schema, connection))
        {
            command.ExecuteNonQuery();
        }
        
        // Read and execute seed data
        string seedPath = Path.Combine(Directory.GetCurrentDirectory(), "SQLite_SeedData.sql");
        string seedData = File.ReadAllText(seedPath);
        
        Console.WriteLine("Inserting seed data...");
        using (var command = new SQLiteCommand(seedData, connection))
        {
            command.ExecuteNonQuery();
        }
        
        Console.WriteLine("\n✓ Database created successfully!");
        Console.WriteLine($"Location: {dbPath}");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
