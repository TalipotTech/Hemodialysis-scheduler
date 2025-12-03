using System;
using Dapper;
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=HDScheduler.db;";

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    
    try
    {
        // Check if columns exist
        var tableInfo = connection.Query("PRAGMA table_info(Patients)");
        bool hasDialysersPurchased = false;
        bool hasBloodTubingPurchased = false;
        
        foreach (dynamic column in tableInfo)
        {
            if (column.name == "DialysersPurchased") hasDialysersPurchased = true;
            if (column.name == "BloodTubingPurchased") hasBloodTubingPurchased = true;
        }
        
        if (hasDialysersPurchased && hasBloodTubingPurchased)
        {
            Console.WriteLine("✓ Columns already exist. Migration already applied.");
            return;
        }
        
        // Apply migration
        if (!hasDialysersPurchased)
        {
            connection.Execute("ALTER TABLE Patients ADD COLUMN DialysersPurchased INTEGER NOT NULL DEFAULT 0");
            Console.WriteLine("✓ Added DialysersPurchased column");
        }
        
        if (!hasBloodTubingPurchased)
        {
            connection.Execute("ALTER TABLE Patients ADD COLUMN BloodTubingPurchased INTEGER NOT NULL DEFAULT 0");
            Console.WriteLine("✓ Added BloodTubingPurchased column");
        }
        
        Console.WriteLine("✓ Migration completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration failed: {ex.Message}");
    }
}
