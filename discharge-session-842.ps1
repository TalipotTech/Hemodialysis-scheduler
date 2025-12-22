# Discharge Session 842 via API
# This will mark the session as completed and make it appear in Patient History

$scheduleId = 842
$apiUrl = "http://localhost:5000/api/HDSchedule/$scheduleId/discharge"

# Get the JWT token from local storage (you'll need to replace this with your actual token)
# For now, we'll try without auth for testing
$token = "YOUR_JWT_TOKEN_HERE"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Discharging session $scheduleId..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Method Put -Headers $headers
    
    if ($response.success) {
        Write-Host "✓ Session $scheduleId discharged successfully!" -ForegroundColor Green
        Write-Host "The session will now appear in Patient History" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Failed to discharge session: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error calling API: $_" -ForegroundColor Red
    Write-Host "`nAlternatively, run the SQL script: discharge-session-842.sql" -ForegroundColor Yellow
}
