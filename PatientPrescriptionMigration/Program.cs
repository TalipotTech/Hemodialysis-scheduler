using Dapper;
using Microsoft.Data.Sqlite;

Console.WriteLine("===========================================");
Console.WriteLine("Patient Prescription Fields Migration");
Console.WriteLine("===========================================\n");

var dbPath = @"G:\ENSATE\HD_Project\Backend\HDScheduler.db";
var connectionString = $"Data Source={dbPath}";

try
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    Console.WriteLine("✓ Database connection established\n");

    var alterCommands = new[]
    {
        ("DialyserModel", "ALTER TABLE Patients ADD COLUMN DialyserModel TEXT"),
        ("PrescribedDuration", "ALTER TABLE Patients ADD COLUMN PrescribedDuration REAL"),
        ("PrescribedBFR", "ALTER TABLE Patients ADD COLUMN PrescribedBFR INTEGER"),
        ("DialysatePrescription", "ALTER TABLE Patients ADD COLUMN DialysatePrescription TEXT")
    };

    Console.WriteLine("Adding columns to Patients table...\n");

    foreach (var (columnName, sql) in alterCommands)
    {
        try
        {
            connection.Execute(sql);
            Console.WriteLine($"  ✓ Added column: {columnName}");
        }
        catch (SqliteException ex)
        {
            if (ex.Message.Contains("duplicate column"))
            {
                Console.WriteLine($"  ○ Column already exists: {columnName}");
            }
            else
            {
                Console.WriteLine($"  ✗ Error adding {columnName}: {ex.Message}");
            }
        }
    }

    connection.Close();

    Console.WriteLine("\n===========================================");
    Console.WriteLine("✅ Migration completed successfully!");
    Console.WriteLine("===========================================\n");
    Console.WriteLine("The following fields can now be saved:");
    Console.WriteLine("  • Dialyser Model");
    Console.WriteLine("  • Prescribed Duration (hours)");
    Console.WriteLine("  • Prescribed BFR (mL/min)");
    Console.WriteLine("  • Dialysate Prescription");
    Console.WriteLine("\nPlease restart the backend server for changes to take effect.");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Migration failed: {ex.Message}");
    return 1;
}

return 0;
