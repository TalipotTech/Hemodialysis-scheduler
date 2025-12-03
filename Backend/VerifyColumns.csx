using Microsoft.Data.Sqlite;
using System;

var connectionString = "Data Source=HDScheduler.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = "PRAGMA table_info(Patients);";

Console.WriteLine("\n=== Patients Table Columns ===");
using var reader = command.ExecuteReader();
var foundNewColumns = false;
while (reader.Read())
{
    var name = reader.GetString(1);
    var type = reader.GetString(2);
    
    if (name == "DryWeight" || name == "HDCycle" || name == "DialyserCount")
    {
        foundNewColumns = true;
        Console.WriteLine($"✓ {name} ({type}) - NEW");
    }
    else
    {
        Console.WriteLine($"  {name} ({type})");
    }
}

Console.WriteLine("\n=== Verification ===");
if (foundNewColumns)
{
    Console.WriteLine("✅ Migration successful! New HD treatment fields are present.");
}
else
{
    Console.WriteLine("⚠️  New columns not found. Migration may not have run.");
}
