# AI Integration Migration Script
# Applies the AI integration database changes to Azure SQL Server

param(
    [string]$ServerName = "hds-dev-sqlserver-cin.database.windows.net",
    [string]$DatabaseName = "hds-dev-db",
    [string]$Username = "hdsadmin",
    [string]$Password
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "AI Integration Database Migration" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if password is provided
if ([string]::IsNullOrEmpty($Password)) {
    $Password = Read-Host "Enter SQL Server password" -AsSecureString
    $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
}

# Build connection string
$connectionString = "Server=tcp:$ServerName,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$Username;Password=$Password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Connecting to Azure SQL Server..." -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray
Write-Host ""

try {
    # Load SQL script
    $scriptPath = Join-Path $PSScriptRoot "Backend\Migrations\001_AI_Integration.sql"
    
    if (!(Test-Path $scriptPath)) {
        throw "Migration script not found: $scriptPath"
    }
    
    $migrationScript = Get-Content $scriptPath -Raw
    
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "✓ Connected to database" -ForegroundColor Green
    Write-Host ""
    Write-Host "Applying AI Integration migration..." -ForegroundColor Yellow
    
    # Execute migration - split on GO statements
    $command = $connection.CreateCommand()
    $command.CommandTimeout = 120
    
    $batches = $migrationScript -split '\r?\nGO\r?\n'
    foreach ($batch in $batches) {
        $trimmedBatch = $batch.Trim()
        if ($trimmedBatch -ne "") {
            $command.CommandText = $trimmedBatch
            $result = $command.ExecuteNonQuery()
        }
    }
    
    Write-Host ""
    Write-Host "================================" -ForegroundColor Green
    Write-Host "✓ Migration completed successfully!" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "AI Integration tables created:" -ForegroundColor Cyan
    Write-Host "  - AISettings (for configuration)" -ForegroundColor White
    Write-Host "  - AIUsageLogs (for cost tracking)" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Restart the backend API" -ForegroundColor White
    Write-Host "2. Navigate to Settings > AI Integration in the app" -ForegroundColor White
    Write-Host "3. Enter your Gemini API key" -ForegroundColor White
    Write-Host "4. Enable AI features" -ForegroundColor White
    Write-Host ""
    
    # Close connection
    $connection.Close()
}
catch {
    Write-Host ""
    Write-Host "✗ Error during migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack trace:" -ForegroundColor Gray
    Write-Host $_.Exception.StackTrace -ForegroundColor Gray
    exit 1
}
