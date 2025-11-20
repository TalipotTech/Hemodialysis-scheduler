using System;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        var connectionString = "Data Source=g:\\ENSATE\\HD_Project\\Backend\\HDScheduler.db";
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        
        Console.WriteLine("\n=== ALL PATIENTS ===");
        using (var cmd = new SQLiteCommand("SELECT PatientID, MRN, Name FROM Patients ORDER BY PatientID", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader[0]}, MRN: {reader[1]}, Name: {reader[2]}");
            }
        }
        
        Console.WriteLine("\n=== ALL HD SCHEDULES FOR TODAY ===");
        using (var cmd = new SQLiteCommand(@"
            SELECT h.ScheduleID, p.Name, h.SessionDate, h.SlotID, h.BedNumber, h.IsDischarged 
            FROM HDSchedule h 
            JOIN Patients p ON h.PatientID = p.PatientID 
            WHERE date(h.SessionDate) = date('now') 
            ORDER BY h.IsDischarged, h.SlotID, h.BedNumber", connection))
        using (var reader = cmd.ExecuteReader())
        {
            Console.WriteLine("ScheduleID | PatientName | SlotID | Bed | Discharged");
            Console.WriteLine("--------------------------------------------------------");
            while (reader.Read())
            {
                var status = reader.GetInt32(5) == 1 ? "YES" : "NO";
                Console.WriteLine($"{reader[0],10} | {reader[1],-20} | {reader[3],6} | {reader[4],3} | {status}");
            }
        }
        
        Console.WriteLine("\n=== ACTIVE (Non-Discharged) SCHEDULES ONLY ===");
        using (var cmd = new SQLiteCommand(@"
            SELECT h.ScheduleID, p.Name, h.SlotID, h.BedNumber 
            FROM HDSchedule h 
            JOIN Patients p ON h.PatientID = p.PatientID 
            WHERE date(h.SessionDate) = date('now') AND h.IsDischarged = 0
            ORDER BY h.SlotID, h.BedNumber", connection))
        using (var reader = cmd.ExecuteReader())
        {
            int count = 0;
            while (reader.Read())
            {
                Console.WriteLine($"{reader[0],10} | {reader[1],-20} | Slot: {reader[2]} | Bed: {reader[3]}");
                count++;
            }
            Console.WriteLine($"\nTotal Active Schedules: {count}");
        }
    }
}
