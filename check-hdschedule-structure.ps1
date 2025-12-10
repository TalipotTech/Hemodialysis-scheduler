# Check HDSchedule table columns in Azure SQL
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$sql = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HDSchedule'
ORDER BY ORDINAL_POSITION;
"@

try {
    Write-Host "Checking HDSchedule table structure in Azure SQL..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "`nHDSchedule Table Columns:" -ForegroundColor Yellow
    Write-Host "=" * 80 -ForegroundColor Yellow
    
    $dataset.Tables[0] | Format-Table -AutoSize
    
    Write-Host "`nTotal Columns: $($dataset.Tables[0].Rows.Count)" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
