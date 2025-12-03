# Add equipment purchase tracking columns to SQLite database
$ErrorActionPreference = "Stop"

Write-Host "Adding equipment purchase tracking columns to database..." -ForegroundColor Cyan

$dbPath = "Database\HDScheduler.db"

if (!(Test-Path $dbPath)) {
    Write-Host "Database not found at: $dbPath" -ForegroundColor Red
    exit 1
}

# Create a temporary C# file
$csharpCode = @"
using System;
using System.Data.SQLite;

class AddColumns
{
    static void Main()
    {
        var dbPath = "Database/HDScheduler.db";
        using (var conn = new SQLiteConnection("Data Source=" + dbPath))
        {
            conn.Open();
            
            // Check if columns exist
            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "PRAGMA table_info(Patients)";
            var reader = checkCmd.ExecuteReader();
            
            bool hasDialysersPurchased = false;
            bool hasBloodTubingPurchased = false;
            
            while (reader.Read())
            {
                string colName = reader.GetString(1);
                if (colName == "DialysersPurchased") hasDialysersPurchased = true;
                if (colName == "BloodTubingPurchased") hasBloodTubingPurchased = true;
            }
            reader.Close();
            
            // Add columns if they don't exist
            if (!hasDialysersPurchased)
            {
                var cmd1 = conn.CreateCommand();
                cmd1.CommandText = "ALTER TABLE Patients ADD COLUMN DialysersPurchased INTEGER NOT NULL DEFAULT 0";
                cmd1.ExecuteNonQuery();
                Console.WriteLine("✓ Added DialysersPurchased column");
            }
            else
            {
                Console.WriteLine("DialysersPurchased column already exists");
            }
            
            if (!hasBloodTubingPurchased)
            {
                var cmd2 = conn.CreateCommand();
                cmd2.CommandText = "ALTER TABLE Patients ADD COLUMN BloodTubingPurchased INTEGER NOT NULL DEFAULT 0";
                cmd2.ExecuteNonQuery();
                Console.WriteLine("✓ Added BloodTubingPurchased column");
            }
            else
            {
                Console.WriteLine("BloodTubingPurchased column already exists");
            }
            
            Console.WriteLine("Migration completed successfully!");
        }
    }
}
"@

$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
Set-Content -Path $tempFile -Value $csharpCode

try {
    # Compile and run
    $refs = @(
        (Get-ChildItem "Backend\bin\Debug\net8.0\System.Data.SQLite.dll").FullName
    )
    
    Add-Type -TypeDefinition $csharpCode -ReferencedAssemblies $refs
    [AddColumns]::Main()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
}
