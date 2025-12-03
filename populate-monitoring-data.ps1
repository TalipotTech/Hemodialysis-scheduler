# PowerShell script to populate monitoring data for all patients

$baseUrl = "http://localhost:5001/api"
$token = ""

Write-Host "=== Populating Monitoring Data for All Patients ===" -ForegroundColor Cyan
Write-Host ""

# Login first to get token
Write-Host "Logging in as admin..." -ForegroundColor Yellow
$loginBody = @{
    username = "admin"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Logged in successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get all recent schedules
Write-Host "`nFetching active HD sessions..." -ForegroundColor Yellow
try {
    $schedulesResponse = Invoke-RestMethod -Uri "$baseUrl/hdschedule" -Method Get -Headers $headers
    $schedules = $schedulesResponse.data
    Write-Host "✓ Found $($schedules.Count) sessions" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to fetch schedules: $_" -ForegroundColor Red
    exit 1
}

# Monitoring templates (4 readings per session)
$monitoringTemplates = @(
    @{ Hour = 1; BP = "130/80"; Pulse = 70; Temp = 36.5; UFVol = 0.5; VenPress = 120; Note = "First hourly check" },
    @{ Hour = 2; BP = "135/85"; Pulse = 72; Temp = 36.7; UFVol = 1.2; VenPress = 125; Note = "Second hourly check" },
    @{ Hour = 3; BP = "140/90"; Pulse = 75; Temp = 36.8; UFVol = 1.8; VenPress = 130; Note = "Third hourly check" },
    @{ Hour = 4; BP = "138/88"; Pulse = 73; Temp = 36.9; UFVol = 2.4; VenPress = 128; Note = "Final check" }
)

$totalAdded = 0
$skipped = 0
$errors = 0

Write-Host "`nAdding monitoring records..." -ForegroundColor Yellow

foreach ($schedule in $schedules) {
    $scheduleId = $schedule.scheduleID
    $patientId = $schedule.patientID
    $sessionDate = $schedule.sessionDate.Split('T')[0]
    
    # Check if already has monitoring data
    try {
        $existingRecords = Invoke-RestMethod -Uri "$baseUrl/hdschedule/$scheduleId/monitoring" -Method Get -Headers $headers
        if ($existingRecords.data -and $existingRecords.data.Count -gt 0) {
            Write-Host "⏭ Schedule $scheduleId - Already has $($existingRecords.data.Count) records (skipped)" -ForegroundColor Gray
            $skipped++
            continue
        }
    } catch {
        # Continue if endpoint fails
    }
    
    # Add monitoring records
    $recordsAdded = 0
    foreach ($template in $monitoringTemplates) {
        $timeRecorded = Get-Date -Format "yyyy-MM-ddTHH:mm:ss"
        
        $monitoringData = @{
            patientID = $patientId
            scheduleID = $scheduleId
            sessionDate = $sessionDate
            timeRecorded = $timeRecorded
            bloodPressure = $template.BP
            pulseRate = $template.Pulse
            temperature = $template.Temp
            ufVolume = $template.UFVol
            venousPressure = $template.VenPress
            notes = $template.Note
        } | ConvertTo-Json
        
        try {
            $response = Invoke-RestMethod -Uri "$baseUrl/hdschedule/monitoring" -Method Post -Headers $headers -Body $monitoringData
            if ($response.success) {
                $recordsAdded++
                $totalAdded++
            }
        } catch {
            Write-Host "✗ Error adding record to Schedule $scheduleId : $_" -ForegroundColor Red
            $errors++
        }
        
        Start-Sleep -Milliseconds 100  # Small delay to avoid overwhelming the server
    }
    
    if ($recordsAdded -gt 0) {
        Write-Host "✓ Added $recordsAdded records to Schedule $scheduleId (Patient $patientId)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Sessions processed: $($schedules.Count)"
Write-Host "Sessions skipped (already have data): $skipped"
Write-Host "Monitoring records added: $totalAdded"
Write-Host "Errors: $errors"
Write-Host ""
Write-Host "Done! Refresh your browser to see monitoring data for all patients." -ForegroundColor Green
