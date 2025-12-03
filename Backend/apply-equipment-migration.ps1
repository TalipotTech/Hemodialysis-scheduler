# Apply Equipment Purchase Migration directly
Write-Host "Applying Equipment Purchase Migration..." -ForegroundColor Cyan

$dbPath = "..\Database\HDScheduler.db"
$migrationSql = @"
ALTER TABLE Patients ADD COLUMN DialysersPurchased INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Patients ADD COLUMN BloodTubingPurchased INTEGER NOT NULL DEFAULT 0;
"@

# Check if database exists
if (!(Test-Path $dbPath)) {
    Write-Host "Database not found at: $dbPath" -ForegroundColor Red
    exit 1
}

# Use Dapper with the existing backend
$code = @"
using System;
using System.Data.SQLite;
using Dapper;

var connectionString = "Data Source=$dbPath;Version=3;";
using (var connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    
    try {
        // Check if columns already exist
        var tableInfo = connection.Query("PRAGMA table_info(Patients)");
        bool hasDialysersPurchased = false;
        bool hasBloodTubingPurchased = false;
        
        foreach (var column in tableInfo)
        {
            if (column.name == "DialysersPurchased") hasDialysersPurchased = true;
            if (column.name == "BloodTubingPurchased") hasBloodTubingPurchased = true;
        }
        
        if (hasDialysersPurchased && hasBloodTubingPurchased)
        {
            Console.WriteLine("Columns already exist. Migration already applied.");
            return;
        }
        
        // Apply migration
        if (!hasDialysersPurchased)
        {
            connection.Execute("ALTER TABLE Patients ADD COLUMN DialysersPurchased INTEGER NOT NULL DEFAULT 0");
            Console.WriteLine("Added DialysersPurchased column");
        }
        
        if (!hasBloodTubingPurchased)
        {
            connection.Execute("ALTER TABLE Patients ADD COLUMN BloodTubingPurchased INTEGER NOT NULL DEFAULT 0");
            Console.WriteLine("Added BloodTubingPurchased column");
        }
        
        Console.WriteLine("✓ Migration completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration failed: {ex.Message}");
    }
}
"@

# Save to temp file and run
$tempFile = [System.IO.Path]::GetTempFileName() + ".csx"
Set-Content -Path $tempFile -Value $code

try {
    dotnet script $tempFile
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
