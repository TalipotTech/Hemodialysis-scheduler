# PowerShell script to create IntraDialyticMonitoring table

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
    
    # Read and execute SQL script
    $sqlScript = Get-Content "create-monitoring-table.sql" -Raw
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    
    Write-Host "Executing migration..." -ForegroundColor Yellow
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "Result: $result" -ForegroundColor Cyan
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor Red
    exit 1
}
