#r "nuget: System.Data.SQLite, 1.0.118"

using System;
using System.Data.SQLite;

var dbPath = "HDScheduler.db";
var conn = new SQLiteConnection($"Data Source={dbPath}");
conn.Open();

try 
{
    var cmd = conn.CreateCommand();
    cmd.CommandText = "ALTER TABLE Patients ADD COLUMN DialysersPurchased INTEGER NOT NULL DEFAULT 0";
    cmd.ExecuteNonQuery();
    Console.WriteLine("✓ Added DialysersPurchased column");
} 
catch (Exception ex)
{
    if (ex.Message.Contains("duplicate column"))
        Console.WriteLine("DialysersPurchased column already exists");
    else
        Console.WriteLine($"Error adding DialysersPurchased: {ex.Message}");
}

try 
{
    var cmd2 = conn.CreateCommand();
    cmd2.CommandText = "ALTER TABLE Patients ADD COLUMN BloodTubingPurchased INTEGER NOT NULL DEFAULT 0";
    cmd2.ExecuteNonQuery();
    Console.WriteLine("✓ Added BloodTubingPurchased column");
} 
catch (Exception ex)
{
    if (ex.Message.Contains("duplicate column"))
        Console.WriteLine("BloodTubingPurchased column already exists");
    else
        Console.WriteLine($"Error adding BloodTubingPurchased: {ex.Message}");
}

conn.Close();
Console.WriteLine("\n✅ Migration completed!");
