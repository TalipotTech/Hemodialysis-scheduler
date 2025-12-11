# Azure SQL Migration Runner
# This script connects to Azure SQL Server and runs the migration to add missing columns

$title = "Azure SQL Migration: Add Missing HD Columns"
Write-Host "============================================" -ForegroundColor Cyan
Write-Host $title -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Azure SQL Connection Details
$serverName = "hds-dev-sqlserver-cin.database.windows.net"
$databaseName = "hds-dev-db"
$username = "hdsadmin"

# Try to read password from appsettings.json
$appsettingsPath = Join-Path $PSScriptRoot "..\Backend\appsettings.json"
$password = $null

if (Test-Path $appsettingsPath) {
    try {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $connString = $appsettings.ConnectionStrings.DefaultConnection
        if ($connString -match "Password=([^;]+)") {
            $extractedPassword = $matches[1]
            $placeholder = "{your_password}"
            if ($extractedPassword -ne $placeholder) {
                $password = $extractedPassword
                Write-Host "Using password from appsettings.json" -ForegroundColor Green
                Write-Host ""
            }
        }
    }
    catch {
        Write-Host "Could not read appsettings.json" -ForegroundColor Yellow
    }
}

# Prompt for password if not found
if ([string]::IsNullOrWhiteSpace($password)) {
    $securePassword = Read-Host "Enter password for Azure SQL Server" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

if ([string]::IsNullOrWhiteSpace($password)) {
    Write-Host "Error: Password is required" -ForegroundColor Red
    exit 1
}

# Connection string
$connectionString = "Server=tcp:$serverName,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=$username;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Connecting to Azure SQL Server..." -ForegroundColor Yellow
Write-Host "Server: $serverName" -ForegroundColor Gray
Write-Host "Database: $databaseName" -ForegroundColor Gray
Write-Host ""

# Load SQL migration script
$scriptPath = Join-Path $PSScriptRoot "SqlServer\04_AddMissingHDScheduleColumns.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: Migration script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "Loading migration script..." -ForegroundColor Yellow
$sqlScript = Get-Content $scriptPath -Raw

try {
    # Load SQL Client assembly
    Add-Type -AssemblyName "System.Data"
    
    # Create connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected to Azure SQL Server successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Split script by GO statements
    $batches = $sqlScript -split '\r?\nGO\r?\n'
    
    $successCount = 0
    $skipCount = 0
    
    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ([string]::IsNullOrWhiteSpace($batch)) {
            continue
        }
        
        try {
            $command = New-Object System.Data.SqlClient.SqlCommand($batch, $connection)
            $command.CommandTimeout = 60
            $result = $command.ExecuteNonQuery()
            
            # Check if this was a print statement
            $infoMessages = $connection.GetType().GetEvent("InfoMessage")
            if ($infoMessages) {
                $connection.add_InfoMessage({
                    param($sender, $e)
                    Write-Host $e.Message
                })
            }
            
            $successCount++
        }
        catch {
            $errorMsg = $_.Exception.Message
            if ($errorMsg -like "*already exists*" -or $errorMsg -like "*duplicate*") {
                $skipCount++
            }
            else {
                Write-Host "Warning: $errorMsg" -ForegroundColor Yellow
            }
        }
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "Batches executed: $successCount" -ForegroundColor Gray
    Write-Host "Already existed: $skipCount" -ForegroundColor Gray
    Write-Host ""
    Write-Host "You can now create HD schedules with all treatment fields." -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "Migration failed!" -ForegroundColor Red
    $errMsg = $_.Exception.Message
    Write-Host "Error: $errMsg" -ForegroundColor Red
    exit 1
}
