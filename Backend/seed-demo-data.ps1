# PowerShell Script to Seed Demo Data into SQLite Database
param(
    [string]$DatabasePath = "HDScheduler.db",
    [string]$SqlScriptPath = "../Database/22_SeedDemoData.sql"
)

# Load SQLite assembly
Add-Type -AssemblyName System.Data

# Read SQL script
$sqlScript = Get-Content $SqlScriptPath -Raw

# Split statements by semicolon
$statements = $sqlScript -split ';' | Where-Object { $_.Trim() -and $_.Trim() -notmatch '^--' }

# Create connection
$connectionString = "Data Source=$DatabasePath"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    Write-Host "Connected to database: $DatabasePath" -ForegroundColor Green
    
    $successCount = 0
    $errorCount = 0
    
    foreach ($statement in $statements) {
        $trimmedStatement = $statement.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmedStatement)) {
            continue
        }
        
        try {
            $command = $connection.CreateCommand()
            $command.CommandText = $trimmedStatement
            
            if ($trimmedStatement -match '^\s*SELECT') {
                $reader = $command.ExecuteReader()
                while ($reader.Read()) {
                    $row = @{}
                    for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                        $row[$reader.GetName($i)] = $reader.GetValue($i)
                    }
                    Write-Host ($row | Out-String) -ForegroundColor Cyan
                }
                $reader.Close()
            }
            else {
                $rowsAffected = $command.ExecuteNonQuery()
                $successCount++
                Write-Host "[OK] Executed statement successfully ($rowsAffected rows affected)" -ForegroundColor Green
            }
            
            $command.Dispose()
        }
        catch {
            $errorCount++
            Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "  Statement: $($trimmedStatement.Substring(0, [Math]::Min(100, $trimmedStatement.Length)))..." -ForegroundColor Yellow
        }
    }
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Demo data seeding completed!" -ForegroundColor Green
    Write-Host "   Success: $successCount statements" -ForegroundColor Green
    Write-Host "   Errors: $errorCount statements" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
    Write-Host "========================================`n" -ForegroundColor Cyan
}
catch {
    Write-Host "Fatal error: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
