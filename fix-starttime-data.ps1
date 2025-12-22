# PowerShell script to fix StartTime data in schedule 165

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
    
    # Clear the bad StartTime data from schedule 165
    $query = @"
UPDATE HDSchedule
SET StartTime = NULL
WHERE ScheduleID = 165;
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $rowsAffected = $command.ExecuteNonQuery()
    
    Write-Host "Fixed schedule 165: $rowsAffected row(s) updated" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
