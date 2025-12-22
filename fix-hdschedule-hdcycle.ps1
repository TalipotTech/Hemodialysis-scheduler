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
    
    # First check current column size
    $CheckQuery = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HDSchedule' 
AND COLUMN_NAME = 'HDCycle';
"@
    
    $SqlCmd = New-Object System.Data.SqlClient.SqlCommand
    $SqlCmd.CommandText = $CheckQuery
    $SqlCmd.Connection = $SqlConnection
    
    $SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
    $SqlAdapter.SelectCommand = $SqlCmd
    $DataSet = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet) | Out-Null
    
    Write-Host "`nCurrent HDCycle column configuration:" -ForegroundColor Yellow
    $DataSet.Tables[0] | Format-Table -AutoSize
    
    # Now alter the column to NVARCHAR(100)
    Write-Host "`nIncreasing HDCycle column size to NVARCHAR(100)..." -ForegroundColor Cyan
    
    $AlterQuery = @"
ALTER TABLE HDSchedule
ALTER COLUMN HDCycle NVARCHAR(100) NULL;
"@
    
    $SqlCmd.CommandText = $AlterQuery
    $SqlCmd.ExecuteNonQuery() | Out-Null
    
    Write-Host "✅ Column altered successfully!" -ForegroundColor Green
    
    # Verify the change
    $SqlCmd.CommandText = $CheckQuery
    $DataSet2 = New-Object System.Data.DataSet
    $SqlAdapter.Fill($DataSet2) | Out-Null
    
    Write-Host "`nUpdated HDCycle column configuration:" -ForegroundColor Yellow
    $DataSet2.Tables[0] | Format-Table -AutoSize
    
    Write-Host "Migration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    $SqlConnection.Close()
}
