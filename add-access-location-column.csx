using System;
using System.Data.SqlClient;

// Quick C# script to add AccessLocation column to Azure SQL
// Run with: dotnet script add-access-location-column.csx

var connectionString = "Server=hds-dev-sqlserver-cin.database.windows.net;Database=hds-dev-db;Authentication=Active Directory Interactive;Encrypt=True;";

var sql = @"
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') 
    AND name = 'AccessLocation'
)
BEGIN
    ALTER TABLE [dbo].[HDSchedule]
    ADD AccessLocation NVARCHAR(200) NULL;
    
    PRINT 'Column AccessLocation added to HDSchedule table successfully.';
END
ELSE
BEGIN
    PRINT 'Column AccessLocation already exists in HDSchedule table.';
END";

try
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("Connected to Azure SQL Database...");
    
    using var command = new SqlCommand(sql, connection);
    command.ExecuteNonQuery();
    
    Console.WriteLine("Migration applied successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
