# Check Slots table MaxBeds values
$connectionString = "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=Talipot@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    Write-Host "Checking Slots table MaxBeds..." -ForegroundColor Cyan
    
    Add-Type -AssemblyName "System.Data"
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!`n" -ForegroundColor Green
    
    $query = "SELECT SlotID, SlotName, StartTime, EndTime, BedCapacity, MaxBeds, IsActive FROM Slots ORDER BY SlotID"
    
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "=== Slots Table Data ===" -ForegroundColor Yellow
    $dataset.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
    
    Write-Host "`nIf MaxBeds is NULL or incorrect, the schedule page will not show all beds." -ForegroundColor Cyan
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
