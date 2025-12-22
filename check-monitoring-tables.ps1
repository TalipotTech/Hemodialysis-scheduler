# PowerShell script to check what monitoring tables exist

$Server = "tcp:hds-dev-sqlserver-cin.database.windows.net,1433"
$Database = "hds-dev-db"
$UserId = "hdsadmin"
$Password = "Talipot@123"

$ConnectionString = "Server=$Server;Initial Catalog=$Database;User ID=$UserId;Password=$Password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    $connection.Open()
    
    Write-Host "Connected to database: $Database" -ForegroundColor Green
    
    # Check for monitoring-related tables
    $query = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Monitoring%' OR TABLE_NAME LIKE '%IntraDialytic%'
ORDER BY TABLE_NAME;
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()
    
    Write-Host "`nMonitoring/IntraDialytic tables found:" -ForegroundColor Yellow
    while ($reader.Read()) {
        Write-Host "  - $($reader[0])" -ForegroundColor Cyan
    }
    
    $reader.Close()
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
