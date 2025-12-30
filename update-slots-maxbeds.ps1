# Update Slots MaxBeds to 12 for Morning slot
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    Write-Host "Updating Slots MaxBeds..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!`n" -ForegroundColor Green
    
    # First, check current values
    Write-Host "Current Slots Configuration:" -ForegroundColor Yellow
    $selectQuery = "SELECT SlotID, SlotName, BedCapacity, MaxBeds FROM Slots ORDER BY SlotID"
    $selectCommand = New-Object System.Data.SqlClient.SqlCommand($selectQuery, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectCommand)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    $dataset.Tables[0] | Format-Table -AutoSize
    
    # Update MaxBeds for all slots based on BedCapacity if MaxBeds is null or different
    Write-Host "`nUpdating MaxBeds to match BedCapacity..." -ForegroundColor Cyan
    $updateQuery = @"
UPDATE Slots 
SET MaxBeds = BedCapacity 
WHERE MaxBeds IS NULL OR MaxBeds != BedCapacity
"@
    
    $updateCommand = New-Object System.Data.SqlClient.SqlCommand($updateQuery, $connection)
    $rowsAffected = $updateCommand.ExecuteNonQuery()
    
    Write-Host "Updated $rowsAffected slot(s)" -ForegroundColor Green
    
    # Show updated values
    Write-Host "`nUpdated Slots Configuration:" -ForegroundColor Yellow
    $adapter2 = New-Object System.Data.SqlClient.SqlDataAdapter($selectCommand)
    $dataset2 = New-Object System.Data.DataSet
    $adapter2.Fill($dataset2) | Out-Null
    $dataset2.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
    
    Write-Host "`nMaxBeds updated successfully! Please refresh the schedule page." -ForegroundColor Green
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
