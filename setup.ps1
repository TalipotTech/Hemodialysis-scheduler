# HD Scheduler - Quick Setup Script
# This script automates the setup process for the HD Scheduler project

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "HD Scheduler System - Quick Setup Script" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

# Function to check if a command exists
function Test-CommandExists {
    param($command)
    $null = Get-Command $command -ErrorAction SilentlyContinue
    return $?
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

$allPrerequisitesMet = $true

if (Test-CommandExists "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    $allPrerequisitesMet = $false
}

if (Test-CommandExists "node") {
    $nodeVersion = node --version
    Write-Host "✓ Node.js found: $nodeVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js not found. Please install Node.js 18+" -ForegroundColor Red
    $allPrerequisitesMet = $false
}

if (Test-CommandExists "npm") {
    $npmVersion = npm --version
    Write-Host "✓ npm found: $npmVersion" -ForegroundColor Green
} else {
    Write-Host "✗ npm not found" -ForegroundColor Red
    $allPrerequisitesMet = $false
}

if (Test-CommandExists "sqlcmd") {
    Write-Host "✓ SQL Server command-line tools found" -ForegroundColor Green
} else {
    Write-Host "⚠ sqlcmd not found. Database setup will need to be done manually" -ForegroundColor Yellow
}

if (Test-CommandExists "ng") {
    $ngVersion = ng version 2>&1 | Select-String "Angular CLI" | Out-String
    Write-Host "✓ Angular CLI found" -ForegroundColor Green
} else {
    Write-Host "⚠ Angular CLI not found. Installing..." -ForegroundColor Yellow
    npm install -g @angular/cli@17
}

Write-Host ""

if (-not $allPrerequisitesMet) {
    Write-Host "Please install missing prerequisites before continuing." -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

# Ask user what they want to set up
Write-Host "What would you like to set up?" -ForegroundColor Cyan
Write-Host "1. Database only"
Write-Host "2. Backend only"
Write-Host "3. Frontend only"
Write-Host "4. Everything (Database + Backend + Frontend)"
Write-Host "5. Exit"
Write-Host ""
$choice = Read-Host "Enter your choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host "`nSetting up database..." -ForegroundColor Yellow
        
        $server = Read-Host "Enter SQL Server name (default: localhost)"
        if ([string]::IsNullOrWhiteSpace($server)) { $server = "localhost" }
        
        # Create database
        Write-Host "Creating database..." -ForegroundColor Yellow
        sqlcmd -S $server -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HDScheduler') CREATE DATABASE HDScheduler"
        
        # Run schema script
        Write-Host "Creating schema..." -ForegroundColor Yellow
        sqlcmd -S $server -d HDScheduler -i "Database\01_CreateSchema.sql"
        
        # Run seed data script
        Write-Host "Inserting seed data..." -ForegroundColor Yellow
        sqlcmd -S $server -d HDScheduler -i "Database\02_SeedData.sql"
        
        Write-Host "`n✓ Database setup complete!" -ForegroundColor Green
    }
    
    "2" {
        Write-Host "`nSetting up backend..." -ForegroundColor Yellow
        
        # Restore packages
        Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
        Set-Location Backend
        dotnet restore
        
        # Build
        Write-Host "Building backend..." -ForegroundColor Yellow
        dotnet build
        
        Write-Host "`n✓ Backend setup complete!" -ForegroundColor Green
        Write-Host "To run the API: cd Backend; dotnet run" -ForegroundColor Cyan
        Set-Location ..
    }
    
    "3" {
        Write-Host "`nSetting up frontend..." -ForegroundColor Yellow
        
        $createNew = Read-Host "Create new Angular project? (y/n)"
        
        if ($createNew -eq "y" -or $createNew -eq "Y") {
            Set-Location Frontend
            Write-Host "Creating Angular application..." -ForegroundColor Yellow
            ng new hd-scheduler-app --routing --style=scss --skip-git
            
            Set-Location hd-scheduler-app
            
            Write-Host "Installing Angular Material..." -ForegroundColor Yellow
            ng add @angular/material --defaults
            
            Write-Host "Installing additional dependencies..." -ForegroundColor Yellow
            npm install @auth0/angular-jwt
            
            Write-Host "`n✓ Frontend setup complete!" -ForegroundColor Green
            Write-Host "To run the app: cd Frontend/hd-scheduler-app; ng serve" -ForegroundColor Cyan
            Set-Location ..\..
        } else {
            Set-Location Frontend\hd-scheduler-app
            Write-Host "Installing dependencies..." -ForegroundColor Yellow
            npm install
            
            Write-Host "`n✓ Frontend dependencies installed!" -ForegroundColor Green
            Set-Location ..\..
        }
    }
    
    "4" {
        Write-Host "`nSetting up everything..." -ForegroundColor Yellow
        
        # Database
        Write-Host "`n[1/3] Database Setup" -ForegroundColor Cyan
        $server = Read-Host "Enter SQL Server name (default: localhost)"
        if ([string]::IsNullOrWhiteSpace($server)) { $server = "localhost" }
        
        sqlcmd -S $server -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HDScheduler') CREATE DATABASE HDScheduler"
        sqlcmd -S $server -d HDScheduler -i "Database\01_CreateSchema.sql"
        sqlcmd -S $server -d HDScheduler -i "Database\02_SeedData.sql"
        Write-Host "✓ Database complete" -ForegroundColor Green
        
        # Backend
        Write-Host "`n[2/3] Backend Setup" -ForegroundColor Cyan
        Set-Location Backend
        dotnet restore
        dotnet build
        Set-Location ..
        Write-Host "✓ Backend complete" -ForegroundColor Green
        
        # Frontend
        Write-Host "`n[3/3] Frontend Setup" -ForegroundColor Cyan
        $createNew = Read-Host "Create new Angular project? (y/n)"
        
        if ($createNew -eq "y" -or $createNew -eq "Y") {
            Set-Location Frontend
            ng new hd-scheduler-app --routing --style=scss --skip-git
            Set-Location hd-scheduler-app
            ng add @angular/material --defaults
            npm install @auth0/angular-jwt
            Set-Location ..\..
        } else {
            if (Test-Path "Frontend\hd-scheduler-app") {
                Set-Location Frontend\hd-scheduler-app
                npm install
                Set-Location ..\..
            }
        }
        Write-Host "✓ Frontend complete" -ForegroundColor Green
        
        Write-Host "`n===============================================" -ForegroundColor Green
        Write-Host "✓ Complete setup finished!" -ForegroundColor Green
        Write-Host "===============================================" -ForegroundColor Green
        
        Write-Host "`nNext steps:" -ForegroundColor Cyan
        Write-Host "1. Start Backend: cd Backend; dotnet run"
        Write-Host "2. Start Frontend: cd Frontend/hd-scheduler-app; ng serve"
        Write-Host "3. Open browser: http://localhost:4200"
        Write-Host "4. Login with admin/Admin@123`n"
    }
    
    "5" {
        Write-Host "Exiting..." -ForegroundColor Yellow
        exit
    }
    
    default {
        Write-Host "Invalid choice. Exiting..." -ForegroundColor Red
        exit
    }
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
