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
    
    # Check all columns in HDSchedule table
    $Query = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HDSchedule'
ORDER BY ORDINAL_POSITION;
"@
    
    $SqlCmd = New-Object System.Data.SqlClient.SqlCommand
    $SqlCmd.CommandText = $Query
    $SqlCmd.Connection = $SqlConnection
    
    $SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
    $SqlAdapter.SelectCommand = $SqlCmd
    $DataSet = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet) | Out-Null
    
    Write-Host "`nAll columns in HDSchedule table:" -ForegroundColor Yellow
    $DataSet.Tables[0] | Format-Table -AutoSize
    
    Write-Host "`nTotal columns: $($DataSet.Tables[0].Rows.Count)" -ForegroundColor Cyan
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    $SqlConnection.Close()
}
