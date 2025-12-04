// InitializeSlots.csx - Run with: dotnet script InitializeSlots.csx
#r "nuget: Microsoft.Data.Sqlite, 7.0.0"

using Microsoft.Data.Sqlite;

var dbPath = "Database/HDScheduler.db";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"❌ Database not found at: {dbPath}");
    return;
}

using var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

// Clear existing slots
var deleteCmd = connection.CreateCommand();
deleteCmd.CommandText = "DELETE FROM Slots";
deleteCmd.ExecuteNonQuery();
Console.WriteLine("✓ Cleared existing slots");

// Insert 4 default slots
var insertCmd = connection.CreateCommand();
insertCmd.CommandText = @"
    INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, MaxBeds, BedCapacity, IsActive) VALUES
    (1, 'Slot 1 - Morning', '06:00', '10:00', 10, 10, 1),
    (2, 'Slot 2 - Afternoon', '11:00', '15:00', 10, 10, 1),
    (3, 'Slot 3 - Evening', '16:00', '20:00', 10, 10, 1),
    (4, 'Slot 4 - Night', '21:00', '01:00', 10, 10, 1)";
insertCmd.ExecuteNonQuery();
Console.WriteLine("✓ Inserted 4 default slots");

// Verify
var selectCmd = connection.CreateCommand();
selectCmd.CommandText = "SELECT SlotID, SlotName, MaxBeds FROM Slots";
using var reader = selectCmd.ExecuteReader();

Console.WriteLine("\n📊 Slots Created:");
while (reader.Read())
{
    Console.WriteLine($"  Slot {reader.GetInt32(0)}: {reader.GetString(1)} - {reader.GetInt32(2)} beds");
}

Console.WriteLine("\n✅ Initialization complete!");
Console.WriteLine("Navigate to: http://localhost:4200/admin/system-settings");
