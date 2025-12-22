$Server = "tcp:hds-dev-sqlserver-cin.database.windows.net,1433"
$Database = "hds-dev-db"
$Username = "hdsadmin"
$Password = "Talipot@123"

$ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;Encrypt=True;TrustServerCertificate=False;"

$SqlConnection = New-Object System.Data.SqlClient.SqlConnection
$SqlConnection.ConnectionString = $ConnectionString

try {
    $SqlConnection.Open()
    Write-Host "Connected to database: $Database" -ForegroundColor Green
    
    # Check what monitoring tables exist
    Write-Host "`nChecking for monitoring-related tables..." -ForegroundColor Cyan
    $TablesQuery = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_NAME LIKE '%dialytic%' OR TABLE_NAME LIKE '%monitoring%'
ORDER BY TABLE_NAME;
"@
    
    $SqlCmd = New-Object System.Data.SqlClient.SqlCommand
    $SqlCmd.CommandText = $TablesQuery
    $SqlCmd.Connection = $SqlConnection
    
    $SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
    $SqlAdapter.SelectCommand = $SqlCmd
    $DataSet = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet) | Out-Null
    
    Write-Host "Found tables:" -ForegroundColor Yellow
    $DataSet.Tables[0] | Format-Table -AutoSize
    
    # Check IntraDialyticRecords table schema if it exists
    Write-Host "`nChecking IntraDialyticRecords table schema..." -ForegroundColor Cyan
    $SchemaQuery = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'IntraDialyticRecords'
ORDER BY ORDINAL_POSITION;
"@
    
    $SqlCmd.CommandText = $SchemaQuery
    $DataSet2 = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet2) | Out-Null
    
    if ($DataSet2.Tables[0].Rows.Count -gt 0) {
        Write-Host "IntraDialyticRecords columns:" -ForegroundColor Yellow
        $DataSet2.Tables[0] | Format-Table -AutoSize
    } else {
        Write-Host "IntraDialyticRecords table does not exist" -ForegroundColor Red
    }
    
    # Check IntraDialyticMonitoring table schema if it exists
    Write-Host "`nChecking IntraDialyticMonitoring table schema..." -ForegroundColor Cyan
    $SchemaQuery2 = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'IntraDialyticMonitoring'
ORDER BY ORDINAL_POSITION;
"@
    
    $SqlCmd.CommandText = $SchemaQuery2
    $DataSet3 = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet3) | Out-Null
    
    if ($DataSet3.Tables[0].Rows.Count -gt 0) {
        Write-Host "IntraDialyticMonitoring columns:" -ForegroundColor Yellow
        $DataSet3.Tables[0] | Format-Table -AutoSize
    } else {
        Write-Host "IntraDialyticMonitoring table does not exist" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    $SqlConnection.Close()
}
