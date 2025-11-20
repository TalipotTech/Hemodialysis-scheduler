# Test Database Connection Script
Write-Host "Testing HDScheduler Database Connection..." -ForegroundColor Cyan

try {
    # Connection string from appsettings
    $connectionString = "Server=(localdb)\MSSQLLocalDB;Database=HDScheduler;Trusted_Connection=True;TrustServerCertificate=True;"
    
    # Create connection
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    
    # Open connection
    $connection.Open()
    Write-Host "✓ Database connection successful!" -ForegroundColor Green
    
    # Test if tables exist
    $command = $connection.CreateCommand()
    $command.CommandText = @"
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME
"@
    
    $reader = $command.ExecuteReader()
    
    Write-Host "`nExisting Tables:" -ForegroundColor Yellow
    $tableCount = 0
    while ($reader.Read()) {
        Write-Host "  - $($reader[0])" -ForegroundColor White
        $tableCount++
    }
    $reader.Close()
    
    if ($tableCount -eq 0) {
        Write-Host "`n⚠ No tables found! You need to run the schema scripts." -ForegroundColor Red
        Write-Host "  Run: sqlcmd -S (localdb)\MSSQLLocalDB -d HDScheduler -i Database\01_CreateSchema.sql" -ForegroundColor Yellow
        Write-Host "  Then: sqlcmd -S (localdb)\MSSQLLocalDB -d HDScheduler -i Database\02_SeedData.sql" -ForegroundColor Yellow
        Write-Host "  And: sqlcmd -S (localdb)\MSSQLLocalDB -d HDScheduler -i Database\03_UpdateSchemaForHDLog.sql" -ForegroundColor Yellow
    } else {
        Write-Host "`n✓ Found $tableCount tables" -ForegroundColor Green
        
        # Check for new HD Log fields in Patients table
        $command.CommandText = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Patients' AND COLUMN_NAME IN ('MRN', 'AccessType', 'HDUnitNumber')
"@
        $reader = $command.ExecuteReader()
        $hdLogFieldsCount = 0
        while ($reader.Read()) {
            $hdLogFieldsCount++
        }
        $reader.Close()
        
        if ($hdLogFieldsCount -gt 0) {
            Write-Host "✓ HD Log fields are present in Patients table" -ForegroundColor Green
        } else {
            Write-Host "⚠ HD Log fields are missing. Run: Database\03_UpdateSchemaForHDLog.sql" -ForegroundColor Yellow
        }
    }
    
    $connection.Close()
    
} catch {
    Write-Host "`n✗ Database connection failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nPossible solutions:" -ForegroundColor Yellow
    Write-Host "1. Make sure SQL Server LocalDB is installed" -ForegroundColor White
    Write-Host "2. Create the database: sqlcmd -S (localdb)\MSSQLLocalDB -Q `"CREATE DATABASE HDScheduler`"" -ForegroundColor White
    Write-Host "3. Run the schema scripts in the Database folder" -ForegroundColor White
}

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
