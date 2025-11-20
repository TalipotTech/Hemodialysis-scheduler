# Simple RBAC Test Script
$baseUrl = "http://localhost:5001/api"

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "HD Scheduler RBAC Testing" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Test login for each role
$users = @{
    "Admin" = @{ user = "admin"; pass = "Admin@123" }
    "Doctor" = @{ user = "doctor1"; pass = "Doctor@123" }
    "Nurse" = @{ user = "nurse1"; pass = "Nurse@123" }
    "Technician" = @{ user = "tech1"; pass = "Tech@123" }
    "HOD" = @{ user = "hod"; pass = "HOD@123" }
}

foreach ($role in $users.Keys) {
    Write-Host "Testing $role Role..." -ForegroundColor Yellow
    
    # Login
    $loginBody = @{
        username = $users[$role].user
        password = $users[$role].pass
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
        $token = $response.data.token
        Write-Host "  ✓ Login successful" -ForegroundColor Green
        
        # Test viewing HD schedules (all roles should work)
        $headers = @{ "Authorization" = "Bearer $token" }
        $schedules = Invoke-RestMethod -Uri "$baseUrl/hdschedule" -Headers $headers -Method GET
        Write-Host "  ✓ Can view HD schedules" -ForegroundColor Green
        
        # Test creating HD schedule (only Admin, Doctor, Nurse should work)
        try {
            $newSchedule = @{
                patientID = 1
                scheduleDate = (Get-Date).ToString("yyyy-MM-dd")
                slotID = 1
                bedNumber = 5
                dialyserType = "HI"
                duration = 4.0
                bloodFlowRate = 300
                dialysateFlowRate = 500
            } | ConvertTo-Json
            
            $createResult = Invoke-RestMethod -Uri "$baseUrl/hdschedule" -Headers $headers -Method POST -Body $newSchedule -ContentType "application/json"
            Write-Host "  ✓ Can CREATE HD schedules" -ForegroundColor Green
        }
        catch {
            if ($_.Exception.Response.StatusCode.value__ -eq 403) {
                Write-Host "  ✗ CANNOT create HD schedules (403 Forbidden)" -ForegroundColor Yellow
            }
            else {
                Write-Host "  ✗ Error creating schedule: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        
        Write-Host ""
    }
    catch {
        Write-Host "  ✗ Login failed for $role" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
    }
}

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "Expected Results:" -ForegroundColor White
Write-Host "  Admin/Doctor/Nurse: Can view AND create schedules" -ForegroundColor White
Write-Host "  Technician/HOD: Can view but CANNOT create (403)" -ForegroundColor White
Write-Host "==================================" -ForegroundColor Cyan
