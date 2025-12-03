# Populate monitoring data for all patients
$baseUrl = "http://localhost:5001/api"

Write-Host "=== Populating Monitoring Data ===" -ForegroundColor Cyan

# Login
$loginBody = @{ username = "admin"; password = "Admin@123" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "Logged in successfully (Token: $($token.Substring(0,20))...)" -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get schedules
$schedulesResponse = Invoke-RestMethod -Uri "$baseUrl/hdschedule" -Method Get -Headers $headers
$schedules = $schedulesResponse.data
Write-Host "Found $($schedules.Count) sessions`n" -ForegroundColor Green

$totalAdded = 0

foreach ($schedule in $schedules) {
    $scheduleId = $schedule.scheduleID
    $patientId = $schedule.patientID
    $sessionDate = $schedule.sessionDate.Split('T')[0]
    
    # Check existing
    try {
        $existing = Invoke-RestMethod -Uri "$baseUrl/hdschedule/$scheduleId/monitoring" -Method Get -Headers $headers -ErrorAction SilentlyContinue
        if ($existing.data.Count -gt 0) {
            Write-Host "Schedule $scheduleId - Already has data" -ForegroundColor Gray
            continue
        }
    } catch {}
    
    # Add 4 monitoring records
    $readings = @(
        @{ BP="130/80"; Pulse=70; Temp=36.5; UF=0.5; VP=120 },
        @{ BP="135/85"; Pulse=72; Temp=36.7; UF=1.2; VP=125 },
        @{ BP="140/90"; Pulse=75; Temp=36.8; UF=1.8; VP=130 },
        @{ BP="138/88"; Pulse=73; Temp=36.9; UF=2.4; VP=128 }
    )
    
    foreach ($reading in $readings) {
        $data = @{
            patientID = $patientId
            scheduleID = $scheduleId
            sessionDate = $sessionDate
            timeRecorded = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
            bloodPressure = $reading.BP
            pulseRate = $reading.Pulse
            temperature = $reading.Temp
            ufVolume = $reading.UF
            venousPressure = $reading.VP
            notes = "Sample data"
        } | ConvertTo-Json
        
        try {
            $response = Invoke-RestMethod -Uri "$baseUrl/hdschedule/monitoring" -Method Post -Headers $headers -Body $data
            if ($response.success) { $totalAdded++ }
        } catch {
            Write-Host "Error: $_" -ForegroundColor Red
        }
        Start-Sleep -Milliseconds 100
    }
    
    Write-Host "Added 4 records to Schedule $scheduleId" -ForegroundColor Green
}

Write-Host "`nTotal records added: $totalAdded" -ForegroundColor Cyan
Write-Host "Done! Refresh your browser." -ForegroundColor Green
