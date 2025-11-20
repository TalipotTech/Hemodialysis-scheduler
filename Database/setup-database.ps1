# HD Scheduler Database Setup Script
# This script creates and initializes the HDScheduler database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "HD Scheduler Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$server = "(localdb)\MSSQLLocalDB"
$database = "HDScheduler"

# Step 1: Check if database exists
Write-Host "Step 1: Checking if database exists..." -ForegroundColor Yellow
try {
    $result = sqlcmd -S $server -Q "SELECT name FROM sys.databases WHERE name = '$database'" -h -1
    if ($result -match $database) {
        Write-Host "  Database '$database' already exists." -ForegroundColor Green
        $response = Read-Host "  Do you want to recreate it? This will delete all data. (yes/no)"
        if ($response -eq "yes") {
            Write-Host "  Dropping existing database..." -ForegroundColor Yellow
            sqlcmd -S $server -Q "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];"
            Write-Host "  Database dropped successfully." -ForegroundColor Green
        } else {
            Write-Host "  Skipping database creation." -ForegroundColor Yellow
            $skipCreate = $true
        }
    }
} catch {
    Write-Host "  Database does not exist." -ForegroundColor Gray
}

# Step 2: Create database
if (-not $skipCreate) {
    Write-Host ""
    Write-Host "Step 2: Creating database..." -ForegroundColor Yellow
    try {
        sqlcmd -S $server -Q "CREATE DATABASE [$database];"
        Write-Host "  Database created successfully!" -ForegroundColor Green
    } catch {
        Write-Host "  Error creating database: $_" -ForegroundColor Red
        exit 1
    }
}

# Step 3: Run schema script
Write-Host ""
Write-Host "Step 3: Creating database schema..." -ForegroundColor Yellow
try {
    $output = sqlcmd -S $server -d $database -i "01_CreateSchema.sql" -o schema_output.txt
    $content = Get-Content schema_output.txt -Raw
    if ($content -match "Database schema created successfully") {
        Write-Host "  Schema created successfully!" -ForegroundColor Green
    } else {
        Write-Host "  Schema script executed." -ForegroundColor Green
    }
    Remove-Item schema_output.txt -ErrorAction SilentlyContinue
} catch {
    Write-Host "  Error creating schema: $_" -ForegroundColor Red
    exit 1
}

# Step 4: Insert seed data
Write-Host ""
Write-Host "Step 4: Inserting seed data..." -ForegroundColor Yellow
try {
    $output = sqlcmd -S $server -d $database -i "02_SeedData.sql" -o seed_output.txt
    $content = Get-Content seed_output.txt -Raw
    if ($content -match "Seed data inserted successfully") {
        Write-Host "  Seed data inserted successfully!" -ForegroundColor Green
    } else {
        Write-Host "  Seed data script executed." -ForegroundColor Green
    }
    Remove-Item seed_output.txt -ErrorAction SilentlyContinue
} catch {
    Write-Host "  Error inserting seed data: $_" -ForegroundColor Red
    exit 1
}

# Step 5: Update schema for HD Log
Write-Host ""
Write-Host "Step 5: Updating schema for HD Log features..." -ForegroundColor Yellow
try {
    $output = sqlcmd -S $server -d $database -i "03_UpdateSchemaForHDLog.sql" -o update_output.txt 2>&1
    Write-Host "  HD Log schema updates applied!" -ForegroundColor Green
    Remove-Item update_output.txt -ErrorAction SilentlyContinue
} catch {
    Write-Host "  Error updating schema: $_" -ForegroundColor Red
    exit 1
}

# Step 6: Verify installation
Write-Host ""
Write-Host "Step 6: Verifying installation..." -ForegroundColor Yellow
try {
    $tables = sqlcmd -S $server -d $database -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME" -h -1
    Write-Host "  Tables created:" -ForegroundColor Green
    $tables | ForEach-Object { 
        if ($_.Trim()) { 
            Write-Host "    - $($_.Trim())" -ForegroundColor White 
        }
    }
} catch {
    Write-Host "  Error verifying installation: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Login Credentials:" -ForegroundColor Yellow
Write-Host "  Admin:      admin / Admin@123" -ForegroundColor White
Write-Host "  HOD:        hod / Hod@123" -ForegroundColor White
Write-Host "  Doctor:     doctor1 / Doctor@123" -ForegroundColor White
Write-Host "  Nurse:      nurse1 / Nurse@123" -ForegroundColor White
Write-Host "  Technician: tech1 / Tech@123" -ForegroundColor White
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Yellow
Write-Host "  Server=$server;Database=$database;Trusted_Connection=True;" -ForegroundColor White
Write-Host ""
Write-Host "You can now start the backend and frontend!" -ForegroundColor Green
Write-Host ""
