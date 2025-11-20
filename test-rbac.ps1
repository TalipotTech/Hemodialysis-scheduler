# RBAC Testing Script for HD Scheduler
$baseUrl = "http://localhost:5001/api"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "HD Scheduler RBAC Testing Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Test users
$testUsers = @(
    @{ Username = "admin"; Password = "Admin@123"; Role = "Admin" }
    @{ Username = "doctor1"; Password = "Doctor@123"; Role = "Doctor" }
    @{ Username = "nurse1"; Password = "Nurse@123"; Role = "Nurse" }
    @{ Username = "tech1"; Password = "Tech@123"; Role = "Technician" }
    @{ Username = "hod"; Password = "HOD@123"; Role = "HOD" }
)

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [string]$Token,
        [string]$Description,
        [object]$Body = $null
    )
    
    try {
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }
        
        $params = @{
            Uri = "$baseUrl$Endpoint"
            Method = $Method
            Headers = $headers
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "  ✓ $Description" -ForegroundColor Green
        return $true
    }
    catch {
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            if ($statusCode -eq 403) {
                Write-Host "  ✗ $Description - 403 Forbidden (Expected restriction)" -ForegroundColor Yellow
            }
            elseif ($statusCode -eq 401) {
                Write-Host "  ✗ $Description - 401 Unauthorized" -ForegroundColor Red
            }
            else {
                Write-Host "  ✗ $Description - Error: $statusCode" -ForegroundColor Red
            }
        }
        else {
            Write-Host "  ✗ $Description - Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    }
}

function Get-Token {
    param(
        [string]$Username,
        [string]$Password
    )
    
    try {
        $loginBody = @{
            username = $Username
            password = $Password
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
        return $response.data.token
    }
    catch {
        Write-Host "Failed to login as $Username" -ForegroundColor Red
        return $null
    }
}

# Test each role
foreach ($user in $testUsers) {
    Write-Host ""
    Write-Host "Testing Role: $($user.Role) (User: $($user.Username))" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    
    # Get authentication token
    $token = Get-Token -Username $user.Username -Password $user.Password
    
    if (-not $token) {
        Write-Host "  ✗ Authentication failed for $($user.Username)" -ForegroundColor Red
        continue
    }
    
    Write-Host "  ✓ Authentication successful" -ForegroundColor Green
    Write-Host ""
    
    # Test HD Schedule endpoints
    Write-Host "  HD Schedule Endpoints:" -ForegroundColor White
    Test-Endpoint -Method "GET" -Endpoint "/hdschedule" -Token $token -Description "GET /hdschedule (View schedules)"
    Test-Endpoint -Method "GET" -Endpoint "/hdschedule/upcoming" -Token $token -Description "GET /hdschedule/upcoming"
    
    # Test HD Log endpoints
    Write-Host ""
    Write-Host "  HD Log Endpoints:" -ForegroundColor White
    Test-Endpoint -Method "GET" -Endpoint "/hdlog" -Token $token -Description "GET /hdlog (View HD logs)"
    
    # Test patient endpoints
    Write-Host ""
    Write-Host "  Patient Endpoints:" -ForegroundColor White
    Test-Endpoint -Method "GET" -Endpoint "/patients" -Token $token -Description "GET /patients (View patients)"
    
    # Role-specific tests
    switch ($user.Role) {
        "Admin" {
            Write-Host ""
            Write-Host "  Admin-Only Endpoints:" -ForegroundColor White
            Test-Endpoint -Method "GET" -Endpoint "/usermanagement" -Token $token -Description "GET /usermanagement (User management)"
            Test-Endpoint -Method "GET" -Endpoint "/audit-logs" -Token $token -Description "GET /audit-logs (Audit logs)"
        }
        
        "Doctor" {
            Write-Host ""
            Write-Host "  Doctor Capabilities:" -ForegroundColor White
            $newSchedule = @{
                patientID = 1
                scheduleDate = (Get-Date).ToString("yyyy-MM-dd")
                slotID = 1
                bedNumber = 5
                dialyserType = "HI"
                duration = 4.0
                bloodFlowRate = 300
                dialysateFlowRate = 500
            }
            Test-Endpoint -Method "POST" -Endpoint "/hdschedule" -Token $token -Description "POST /hdschedule (Create HD schedule)" -Body $newSchedule
        }
        
        "Nurse" {
            Write-Host ""
            Write-Host "  Nurse Capabilities:" -ForegroundColor White
            $newSchedule = @{
                patientID = 1
                scheduleDate = (Get-Date).ToString("yyyy-MM-dd")
                slotID = 1
                bedNumber = 6
                dialyserType = "HI"
                duration = 4.0
                bloodFlowRate = 300
                dialysateFlowRate = 500
            }
            Test-Endpoint -Method "POST" -Endpoint "/hdschedule" -Token $token -Description "POST /hdschedule (Create HD schedule)" -Body $newSchedule
        }
        
        "Technician" {
            Write-Host ""
            Write-Host "  Technician Restrictions:" -ForegroundColor White
            $newSchedule = @{
                patientID = 1
                scheduleDate = (Get-Date).ToString("yyyy-MM-dd")
                slotID = 1
                bedNumber = 7
            }
            Test-Endpoint -Method "POST" -Endpoint "/hdschedule" -Token $token -Description "POST /hdschedule (Should be FORBIDDEN)"
            
            Write-Host ""
            Write-Host "  Technician Allowed Operations:" -ForegroundColor White
            # Assuming we have an HD log with ID 1 for testing
            $monitoringData = @{
                hdLogId = 1
                monitoringTime = (Get-Date).ToString("HH:mm:ss")
                heartRate = 75
                bloodPressure = "120/80"
                actualBFR = 300
                interventions = "Routine check"
                staffInitials = "TECH1"
            }
            Test-Endpoint -Method "POST" -Endpoint "/hdlog/1/monitoring" -Token $token -Description "POST /hdlog/{id}/monitoring (Add monitoring record)"
        }
        
        "HOD" {
            Write-Host ""
            Write-Host "  HOD Read-Only Restrictions:" -ForegroundColor White
            $newSchedule = @{
                patientID = 1
                scheduleDate = (Get-Date).ToString("yyyy-MM-dd")
                slotID = 1
                bedNumber = 8
            }
            Test-Endpoint -Method "POST" -Endpoint "/hdschedule" -Token $token -Description "POST /hdschedule (Should be FORBIDDEN)"
            Test-Endpoint -Method "POST" -Endpoint "/hdlog" -Token $token -Description "POST /hdlog (Should be FORBIDDEN)"
            
            Write-Host ""
            Write-Host "  HOD Allowed Operations:" -ForegroundColor White
            Test-Endpoint -Method "GET" -Endpoint "/staff" -Token $token -Description "GET /staff (View staff)"
            Test-Endpoint -Method "GET" -Endpoint "/reports" -Token $token -Description "GET /reports (View reports)"
        }
    }
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "RBAC Testing Completed" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Admin: Should have full access to all endpoints" -ForegroundColor White
Write-Host "  - Doctor/Nurse: Can create/edit HD schedules and logs" -ForegroundColor White
Write-Host "  - Technician: Read-only except for monitoring records" -ForegroundColor White
Write-Host "  - HOD: Read-only for HD logs, can manage staff" -ForegroundColor White
Write-Host ""
