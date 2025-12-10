# Quick fix script to add AccessLocation column using .NET SqlClient
# This uses the same connection string as your backend

$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = @"
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') 
    AND name = 'AccessLocation'
)
BEGIN
    ALTER TABLE [dbo].[HDSchedule]
    ADD AccessLocation NVARCHAR(200) NULL;
    
    SELECT 'SUCCESS: Column AccessLocation added to HDSchedule table.' as Result;
END
ELSE
BEGIN
    SELECT 'INFO: Column AccessLocation already exists in HDSchedule table.' as Result;
END
"@

try {
    Write-Host "Connecting to Azure SQL Database..." -ForegroundColor Cyan
    
    # Load SQL Client assembly
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host "Applying migration..." -ForegroundColor Yellow
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        Write-Host "`n$($reader["Result"])" -ForegroundColor Green
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
    Write-Host "You can now restart your backend API." -ForegroundColor Cyan
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
