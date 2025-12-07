using Microsoft.Data.SqlClient;

namespace HDScheduler.API.Data;

/// <summary>
/// Run this once to add HD treatment fields to Patients table
/// </summary>
public class PatientFieldsMigration
{
    public static void ApplyMigration(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Add DryWeight column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN DryWeight REAL;");
        
        // Add HDCycle column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN HDCycle TEXT;");
        
        // Add HDStartDate column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN HDStartDate TEXT;");
        
        // Add DialyserType column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN DialyserType TEXT CHECK (DialyserType IN ('Hi Flux', 'Lo Flux', NULL));");
        
        // Add DialyserCount column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN DialyserCount INTEGER DEFAULT 0;");
        
        // Add BloodTubingCount column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN BloodTubingCount INTEGER DEFAULT 0;");
        
        // Add TotalDialysisCompleted column
        ExecuteNonQuery(connection, "ALTER TABLE Patients ADD COLUMN TotalDialysisCompleted INTEGER DEFAULT 0;");

        Console.WriteLine("✅ Migration applied successfully!");
        Console.WriteLine("New columns added to Patients table:");
        Console.WriteLine("  - DryWeight");
        Console.WriteLine("  - HDCycle");
        Console.WriteLine("  - HDStartDate");
        Console.WriteLine("  - DialyserType");
        Console.WriteLine("  - DialyserCount");
        Console.WriteLine("  - BloodTubingCount");
        Console.WriteLine("  - TotalDialysisCompleted");
    }

    private static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            // Ignore "duplicate column name" errors (column already exists)
            if (!ex.Message.Contains("duplicate column name") && !ex.Message.Contains("already an object"))
            {
                throw;
            }
        }
    }
}
