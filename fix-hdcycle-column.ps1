# PowerShell script to fix HDCycle column size

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
    $sqlScript = Get-Content "fix-hdcycle-column.sql" -Raw
    
    # Split by GO and execute each batch
    $batches = $sqlScript -split '\r?\nGO\r?\n'
    
    foreach ($batch in $batches) {
        if ($batch.Trim() -ne '') {
            $command = $connection.CreateCommand()
            $command.CommandText = $batch
            
            Write-Host "Executing batch..." -ForegroundColor Yellow
            $reader = $command.ExecuteReader()
            
            # Display results
            while ($reader.Read()) {
                for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                    Write-Host "$($reader.GetName($i)): $($reader[$i])" -ForegroundColor Cyan
                }
            }
            $reader.Close()
        }
    }
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
