#!/usr/bin/env dotnet-script
#r "nuget: System.Data.SQLite.Core, 1.0.118"

using System.Data.SQLite;

var dbPath = @"G:\ENSATE\HD_Project\Backend\HDScheduler.db";
var connString = $"Data Source={dbPath};Version=3;";

using (var conn = new SQLiteConnection(connString))
{
    conn.Open();
    
    Console.WriteLine("=== Discharging Session 91 for Patient 53 ===\n");
    
    // Get patient ID from session 91
    int patientId = 0;
    using (var cmd = new SQLiteCommand("SELECT PatientID FROM HDSchedule WHERE ScheduleID = 91", conn))
    {
        var result = cmd.ExecuteScalar();
        if (result != null)
        {
            patientId = Convert.ToInt32(result);
            Console.WriteLine($"Patient ID: {patientId}");
        }
    }
    
    if (patientId > 0)
    {
        // Update equipment counters (same logic as DischargeAsync)
        using (var cmd = new SQLiteCommand(@"
            UPDATE Patients
            SET DialyserCount = DialyserCount + 1,
                BloodTubingCount = BloodTubingCount + 1,
                TotalDialysisCompleted = TotalDialysisCompleted + 1,
                UpdatedAt = datetime('now')
            WHERE PatientID = @PatientID", conn))
        {
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Updated {rows} patient record(s)");
        }
        
        // Mark session as discharged
        using (var cmd = new SQLiteCommand(@"
            UPDATE HDSchedule
            SET IsDischarged = 1,
                SessionStatus = 'Completed',
                UpdatedAt = datetime('now')
            WHERE ScheduleID = 91", conn))
        {
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Marked {rows} session(s) as discharged");
        }
        
        Console.WriteLine("\n=== Verification ===");
        
        // Verify session status
        using (var cmd = new SQLiteCommand("SELECT IsDischarged, SessionStatus FROM HDSchedule WHERE ScheduleID = 91", conn))
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine($"Session IsDischarged: {reader["IsDischarged"]}");
                    Console.WriteLine($"Session Status: {reader["SessionStatus"]}");
                }
            }
        }
        
        // Verify patient counters
        using (var cmd = new SQLiteCommand("SELECT DialyserCount, BloodTubingCount, TotalDialysisCompleted FROM Patients WHERE PatientID = @PatientID", conn))
        {
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine($"DialyserCount: {reader["DialyserCount"]}");
                    Console.WriteLine($"BloodTubingCount: {reader["BloodTubingCount"]}");
                    Console.WriteLine($"TotalDialysisCompleted: {reader["TotalDialysisCompleted"]}");
                }
            }
        }
        
        Console.WriteLine("\n✅ Session 91 discharged successfully!");
    }
    else
    {
        Console.WriteLine("❌ Session 91 not found!");
    }
}
