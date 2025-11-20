@echo off
echo ================================================
echo Starting HD Project - All Components
echo ================================================

echo.
echo [1/3] Checking Database...
cd /d g:\ENSATE\HD_Project
csc CheckSchedule.cs
if errorlevel 1 (
    echo Error compiling CheckSchedule.cs
    pause
    exit /b 1
)
CheckSchedule.exe
echo.
echo Database check complete. Press any key to start servers...
pause

echo.
echo [2/3] Starting Backend Server...
start "HD Backend" cmd /k "cd /d g:\ENSATE\HD_Project\backend && echo Installing dependencies... && npm install && echo Starting backend... && npm run dev"

echo Waiting for backend to initialize...
timeout /t 5 /nobreak > nul

echo [3/3] Starting Frontend Server...
start "HD Frontend" cmd /k "cd /d g:\ENSATE\HD_Project\frontend && echo Installing dependencies... && npm install && echo Starting frontend... && npm run dev"

echo.
echo ================================================
echo All services are starting...
echo ================================================
echo Backend: Check the "HD Backend" window
echo Frontend: Check the "HD Frontend" window
echo.
echo If servers don't start, check the individual windows for errors.
echo Common issues:
echo - Node.js not installed
echo - Wrong port already in use
echo - Missing package.json files
echo.
pause
