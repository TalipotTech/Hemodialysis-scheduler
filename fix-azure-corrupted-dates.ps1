# Fix corrupted dates in Azure SQL Database
# Run this script to find and remove HDSchedule records with dates before 2020

$ServerName = "hds-dev-sqlserver-cin.database.windows.net"
$DatabaseName = "hds-dev-db"
$Username = "hdsadmin"
$Password = "Talipot@123"

Write-Host "========================================"
Write-Host "Connecting to Azure SQL Database..."
Write-Host "========================================"
Write-Host "Server: $ServerName"
Write-Host "Database: $DatabaseName"
Write-Host ""

# Build connection string
$ConnectionString = "Server=tcp:$ServerName,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$Username;Password=$Password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    # Create connection
    $Connection = New-Object System.Data.SqlClient.SqlConnection
    $Connection.ConnectionString = $ConnectionString
    $Connection.Open()
    
    Write-Host "Connected successfully!"
    Write-Host ""
    
    # Query 1: Find corrupted records
    Write-Host "========================================"
    Write-Host "Finding corrupted session dates..."
    Write-Host "========================================"
    
    $Query1 = "SELECT s.ScheduleID, s.PatientID, p.Name AS PatientName, s.SessionDate, s.SessionStatus, s.SlotID, s.BedNumber FROM HDSchedule s LEFT JOIN Patients p ON s.PatientID = p.PatientID WHERE YEAR(s.SessionDate) " + "< 2020 ORDER BY s.SessionDate ASC;"
    
    $Command1 = $Connection.CreateCommand()
    $Command1.CommandText = $Query1
    $Adapter1 = New-Object System.Data.SqlClient.SqlDataAdapter $Command1
    $DataSet1 = New-Object System.Data.DataSet
    $Adapter1.Fill($DataSet1) | Out-Null
    
    $CorruptedRecords = $DataSet1.Tables[0]
    
    if ($CorruptedRecords.Rows.Count -gt 0) {
        $count = $CorruptedRecords.Rows.Count
        Write-Host "Found $count corrupted records:"
        $CorruptedRecords | Format-Table -AutoSize
        Write-Host ""
        
        # Ask for confirmation
        $Confirmation = Read-Host "Do you want to DELETE these corrupted records? (yes/no)"
        
        if ($Confirmation -eq "yes") {
            Write-Host ""
            Write-Host "Deleting corrupted records..."
            
            $DeleteQuery = "DELETE FROM HDSchedule WHERE YEAR(SessionDate) " + "< 2020;"
            $DeleteCommand = $Connection.CreateCommand()
            $DeleteCommand.CommandText = $DeleteQuery
            $RowsAffected = $DeleteCommand.ExecuteNonQuery()
            
            Write-Host "Deleted $RowsAffected corrupted records"
            Write-Host ""
        } else {
            Write-Host "Deletion cancelled by user."
        }
    } else {
        Write-Host "No corrupted records found. Database is clean!"
    }
    
    Write-Host ""
    Write-Host "========================================"
    Write-Host "Patient ID 39 sessions:"
    Write-Host "========================================"
    
    $Query2 = "SELECT s.ScheduleID, s.PatientID, p.Name AS PatientName, s.SessionDate, s.SessionStatus, s.SlotID, s.BedNumber FROM HDSchedule s LEFT JOIN Patients p ON s.PatientID = p.PatientID WHERE s.PatientID = 39 ORDER BY s.SessionDate DESC;"
    
    $Command2 = $Connection.CreateCommand()
    $Command2.CommandText = $Query2
    $Adapter2 = New-Object System.Data.SqlClient.SqlDataAdapter $Command2
    $DataSet2 = New-Object System.Data.DataSet
    $Adapter2.Fill($DataSet2) | Out-Null
    
    $Patient39Records = $DataSet2.Tables[0]
    
    if ($Patient39Records.Rows.Count -gt 0) {
        $Patient39Records | Format-Table -AutoSize
    } else {
        Write-Host "No sessions found for Patient ID 39"
    }
    
    $Connection.Close()
    Write-Host ""
    Write-Host "Done! You can now try activating the patient again."
    
} catch {
    $ErrorMsg = $_.Exception.Message
    Write-Host "ERROR: $ErrorMsg"
    if ($Connection.State -eq 'Open') {
        $Connection.Close()
    }
}
