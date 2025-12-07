using Dapper;
using Microsoft.Data.SqlClient;

namespace HDScheduler.API.Migrations;

public class Migration_20_AddPostDialysisVitalSignsColumns
{
    public static void Execute(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Add Post-Dialysis Vital Signs columns
        var commands = new[]
        {
            "ALTER TABLE HDSchedule ADD COLUMN PostWeight REAL;",
            "ALTER TABLE HDSchedule ADD COLUMN PostSBP INTEGER;",
            "ALTER TABLE HDSchedule ADD COLUMN PostDBP INTEGER;",
            "ALTER TABLE HDSchedule ADD COLUMN PostHR INTEGER;",
            "ALTER TABLE HDSchedule ADD COLUMN PostAccessStatus TEXT;",
            "ALTER TABLE HDSchedule ADD COLUMN TotalFluidRemoved REAL;",
            "ALTER TABLE HDSchedule ADD COLUMN Notes TEXT;"
        };

        foreach (var command in commands)
        {
            try
            {
                connection.Execute(command);
                Console.WriteLine($"Executed: {command}");
            }
            catch (SqlException ex) when (ex.Message.Contains("duplicate column") || ex.Message.Contains("already an object"))
            {
                Console.WriteLine($"Column already exists, skipping: {command}");
            }
        }

        Console.WriteLine("Post-Dialysis Vital Signs columns migration completed successfully");
    }
}
