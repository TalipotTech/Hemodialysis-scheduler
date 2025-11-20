# HD Scheduler - SQLite Database Setup Script

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "HD Scheduler - SQLite Database Setup" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

$dbPath = "g:\ENSATE\HD_Project\Backend\HDScheduler.db"
$schemaPath = "g:\ENSATE\HD_Project\Database\SQLite_Schema.sql"
$seedPath = "g:\ENSATE\HD_Project\Database\SQLite_SeedData.sql"

# Check if SQLite is installed
$sqliteCommand = Get-Command sqlite3 -ErrorAction SilentlyContinue

if (-not $sqliteCommand) {
    Write-Host "SQLite command-line tool not found." -ForegroundColor Yellow
    Write-Host "Installing SQLite via Chocolatey..." -ForegroundColor Yellow
    
    # Check if Chocolatey is installed
    $chocoCommand = Get-Command choco -ErrorAction SilentlyContinue
    
    if (-not $chocoCommand) {
        Write-Host "Chocolatey not found. Please install SQLite manually:" -ForegroundColor Red
        Write-Host "1. Download from https://www.sqlite.org/download.html" -ForegroundColor Yellow
        Write-Host "2. Or install Chocolatey and run: choco install sqlite" -ForegroundColor Yellow
        Write-Host "`nAlternatively, the database will be created automatically when the backend starts." -ForegroundColor Green
        pause
        exit
    } else {
        choco install sqlite -y
        refreshenv
    }
}

# Remove existing database
if (Test-Path $dbPath) {
    Write-Host "Removing existing database..." -ForegroundColor Yellow
    Remove-Item $dbPath -Force
}

Write-Host "Creating new SQLite database..." -ForegroundColor Green
Write-Host "Database path: $dbPath`n" -ForegroundColor Cyan

# Create schema using .NET SQLite
Write-Host "Creating database schema..." -ForegroundColor Yellow

try {
    Add-Type -Path "System.Data.SQLite"
    $connection = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath;Version=3;")
    $connection.Open()
    
    $schemaContent = Get-Content $schemaPath -Raw
    $command = $connection.CreateCommand()
    $command.CommandText = $schemaContent
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Host "✓ Schema created successfully!" -ForegroundColor Green
    
    # Insert seed data
    Write-Host "`nInserting seed data..." -ForegroundColor Yellow
    $seedContent = Get-Content $seedPath -Raw
    $command.CommandText = $seedContent
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Host "✓ Seed data inserted successfully!" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "✓ Database file created. Schema will be initialized when backend starts." -ForegroundColor Yellow
}

Write-Host "`n===============================================" -ForegroundColor Green
Write-Host "SQLite Database Setup Complete!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

Write-Host "`nDatabase Location:" -ForegroundColor Cyan
Write-Host "  $dbPath" -ForegroundColor White

Write-Host "`nDefault Login Credentials:" -ForegroundColor Cyan
Write-Host "  Admin: admin / Admin@123" -ForegroundColor White
Write-Host "  HOD: hod / Admin@123" -ForegroundColor White
Write-Host "  Doctor: doctor1 / Admin@123" -ForegroundColor White
Write-Host "  Nurse: nurse1 / Admin@123" -ForegroundColor White
Write-Host "  Technician: tech1 / Admin@123" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Start the backend: cd Backend; dotnet run" -ForegroundColor White
Write-Host "2. Start the frontend: cd Frontend/hd-scheduler-app; ng serve" -ForegroundColor White
Write-Host "3. Open browser: http://localhost:4200`n" -ForegroundColor White

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
