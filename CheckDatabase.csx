#!/usr/bin/env dotnet-script
#r "nuget: System.Data.SQLite.Core, 1.0.118"

using System.Data.SQLite;

var dbPath = @"G:\ENSATE\HD_Project\Backend\HDScheduler.db";
var connString = $"Data Source={dbPath};Version=3;";

using (var conn = new SQLiteConnection(connString))
{
    conn.Open();
    
    Console.WriteLine("=== Session 91 Status ===");
    using (var cmd = new SQLiteCommand("SELECT ScheduleID, PatientID, SessionDate, IsDischarged, IsMovedToHistory, SessionStatus FROM HDSchedule WHERE ScheduleID = 91", conn))
    {
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"ScheduleID: {reader["ScheduleID"]}");
                Console.WriteLine($"PatientID: {reader["PatientID"]}");
                Console.WriteLine($"SessionDate: {reader["SessionDate"]}");
                Console.WriteLine($"IsDischarged: {reader["IsDischarged"]}");
                Console.WriteLine($"IsMovedToHistory: {reader["IsMovedToHistory"]}");
                Console.WriteLine($"SessionStatus: {reader["SessionStatus"]}");
            }
        }
    }
    
    Console.WriteLine("\n=== Patient 53 Counters ===");
    using (var cmd = new SQLiteCommand("SELECT PatientID, Name, DialyserCount, BloodTubingCount, TotalDialysisCompleted FROM Patients WHERE PatientID = 53", conn))
    {
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"PatientID: {reader["PatientID"]}");
                Console.WriteLine($"Name: {reader["Name"]}");
                Console.WriteLine($"DialyserCount: {reader["DialyserCount"]}");
                Console.WriteLine($"BloodTubingCount: {reader["BloodTubingCount"]}");
                Console.WriteLine($"TotalDialysisCompleted: {reader["TotalDialysisCompleted"]}");
            }
        }
    }
}
