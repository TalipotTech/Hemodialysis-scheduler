using System;
using System.Data.SQLite;

class FixYesterdaySessions
{
    static void Main(string[] args)
    {
        var connectionString = "Data Source=../../Backend/HDScheduler.db";
        
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        
        var query = @"
            UPDATE HDSchedule
            SET IsDischarged = 1,
                IsMovedToHistory = 1,
                UpdatedAt = datetime('now')
            WHERE IsDischarged = 0 
              AND date(SessionDate) < date('now')";
        
        using var command = new SQLiteCommand(query, connection);
        var affected = command.ExecuteNonQuery();
        
        Console.WriteLine($"✓ Marked {affected} session(s) from yesterday as completed");
        
        // Show updated sessions
        var selectQuery = @"
            SELECT 
                ScheduleID,
                PatientID,
                SessionDate,
                SlotID,
                BedNumber,
                IsDischarged,
                IsMovedToHistory
            FROM HDSchedule
            WHERE date(SessionDate) = date('now', '-1 day')";
        
        using var selectCommand = new SQLiteCommand(selectQuery, connection);
        using var reader = selectCommand.ExecuteReader();
        
        Console.WriteLine("\nYesterday's sessions:");
        while (reader.Read())
        {
            Console.WriteLine($"  Schedule {reader["ScheduleID"]}: Patient {reader["PatientID"]}, " +
                            $"Slot {reader["SlotID"]}, Bed {reader["BedNumber"]}, " +
                            $"Discharged: {reader["IsDischarged"]}, Moved: {reader["IsMovedToHistory"]}");
        }
    }
}
