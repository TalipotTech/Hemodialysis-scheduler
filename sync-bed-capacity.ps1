# Sync BedCapacity with MaxBeds
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    Write-Host "Syncing BedCapacity with MaxBeds..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!`n" -ForegroundColor Green
    
    # Update BedCapacity to match MaxBeds
    $updateQuery = @"
UPDATE Slots 
SET BedCapacity = MaxBeds 
WHERE BedCapacity != MaxBeds
"@
    
    $updateCommand = New-Object System.Data.SqlClient.SqlCommand($updateQuery, $connection)
    $rowsAffected = $updateCommand.ExecuteNonQuery()
    
    Write-Host "Updated $rowsAffected slot(s)" -ForegroundColor Green
    
    # Show updated values
    Write-Host "`nUpdated Slots Configuration:" -ForegroundColor Yellow
    $selectQuery = "SELECT SlotID, SlotName, BedCapacity, MaxBeds FROM Slots ORDER BY SlotID"
    $selectCommand = New-Object System.Data.SqlClient.SqlCommand($selectQuery, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectCommand)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    $dataset.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
    
    Write-Host "`nBedCapacity synced with MaxBeds! The schedule page should now show 12 beds for Morning slot." -ForegroundColor Green
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
