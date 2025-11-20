# HD Scheduler - Start All Services Script
# This script starts the database, backend API, and frontend application

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "HD Scheduler System - Starting Services" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

# Get the project root directory
$projectRoot = $PSScriptRoot

# Function to start a process in a new window
function Start-ServiceInNewWindow {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )
    
    Write-Host "Starting $Title..." -ForegroundColor Yellow
    
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "& { Set-Location '$WorkingDirectory'; Write-Host '[$Title] Starting...' -ForegroundColor Green; $Command }"
    )
}

# Check if backend is built
if (-not (Test-Path "$projectRoot\Backend\bin")) {
    Write-Host "Backend not built. Building..." -ForegroundColor Yellow
    Set-Location "$projectRoot\Backend"
    dotnet build
}

# Check if frontend dependencies are installed
if (-not (Test-Path "$projectRoot\Frontend\hd-scheduler-app\node_modules")) {
    Write-Host "Frontend dependencies not installed. Installing..." -ForegroundColor Yellow
    Set-Location "$projectRoot\Frontend\hd-scheduler-app"
    npm install
}

# Start Backend API
Write-Host "`nStarting Backend API..." -ForegroundColor Cyan
Start-ServiceInNewWindow -Title "Backend API" -WorkingDirectory "$projectRoot\Backend" -Command "dotnet run"

# Wait a bit for backend to start
Start-Sleep -Seconds 3

# Start Frontend Application
Write-Host "Starting Frontend Application..." -ForegroundColor Cyan
Start-ServiceInNewWindow -Title "Frontend" -WorkingDirectory "$projectRoot\Frontend\hd-scheduler-app" -Command "ng serve --open"

Write-Host "`n===============================================" -ForegroundColor Green
Write-Host "Services Started!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

Write-Host "`nServices running:" -ForegroundColor Cyan
Write-Host "- Backend API: https://localhost:7001" -ForegroundColor White
Write-Host "- Swagger UI: https://localhost:7001/swagger" -ForegroundColor White
Write-Host "- Frontend: http://localhost:4200" -ForegroundColor White

Write-Host "`nDefault credentials:" -ForegroundColor Cyan
Write-Host "- Admin: admin / Admin@123" -ForegroundColor White
Write-Host "- Doctor: doctor1 / Doctor@123" -ForegroundColor White
Write-Host "- Nurse: nurse1 / Nurse@123" -ForegroundColor White

Write-Host "`nPress any key to exit (services will keep running)..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
