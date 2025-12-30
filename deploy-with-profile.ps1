# Deploy without Azure CLI using Publish Profile
# Download publish profile from Azure Portal first

$zipFile = ".\Backend\deploy.zip"
$appName = "hds-dev-api"

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Deploy using Publish Profile" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

# Check if zip exists
if (-not (Test-Path $zipFile)) {
    Write-Host "[ERROR] deploy.zip not found!" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 1: Download Publish Profile" -ForegroundColor Yellow
Write-Host "Opening Azure Portal..." -ForegroundColor Gray
Start-Process "https://portal.azure.com/#@ensate365.onmicrosoft.com/resource/subscriptions/74dc21b3-629c-40c3-aa0b-935da454b3e4/resourceGroups/EnsateBlogRG/providers/Microsoft.Web/sites/$appName/vstscd"

Write-Host "`nIn Azure Portal:" -ForegroundColor White
Write-Host "1. Click 'Download publish profile' button at the top" -ForegroundColor Gray
Write-Host "2. Save it to your Downloads folder" -ForegroundColor Gray
Write-Host "3. Come back here" -ForegroundColor Gray

Write-Host "`nPress any key after downloading publish profile..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Look for publish profile in Downloads
$downloadsPath = [Environment]::GetFolderPath("UserProfile") + "\Downloads"
$publishProfile = Get-ChildItem "$downloadsPath\$appName*.PublishSettings" -ErrorAction SilentlyContinue | 
                  Sort-Object LastWriteTime -Descending | 
                  Select-Object -First 1

if (-not $publishProfile) {
    Write-Host "`n[ERROR] Publish profile not found in Downloads folder" -ForegroundColor Red
    Write-Host "Please download it and run this script again" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n[OK] Found publish profile: $($publishProfile.Name)" -ForegroundColor Green

# Parse publish profile XML
[xml]$profile = Get-Content $publishProfile.FullName
$publishData = $profile.publishData.publishProfile | Where-Object { $_.publishMethod -eq "MSDeploy" }

if (-not $publishData) {
    Write-Host "[ERROR] Could not parse publish profile" -ForegroundColor Red
    exit 1
}

$username = $publishData.userName
$password = $publishData.userPWD
$publishUrl = $publishData.publishUrl

Write-Host "`nStep 2: Deploying to Azure..." -ForegroundColor Yellow
Write-Host "Target: $publishUrl" -ForegroundColor Gray

# Create basic auth header
$pair = "$($username):$($password)"
$bytes = [System.Text.Encoding]::ASCII.GetBytes($pair)
$base64 = [System.Convert]::ToBase64String($bytes)
$headers = @{
    Authorization = "Basic $base64"
}

# Deploy using Kudu API
$kuduUrl = "https://$appName.scm.azurewebsites.net/api/zipdeploy"

try {
    Write-Host "Uploading deployment package..." -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri $kuduUrl `
        -Method Post `
        -InFile $zipFile `
        -Headers $headers `
        -ContentType "application/zip" `
        -TimeoutSec 600
    
    Write-Host "`n=====================================" -ForegroundColor Cyan
    Write-Host "[SUCCESS] Deployment Completed!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    
    Write-Host "`nYour backend is now live at:" -ForegroundColor White
    Write-Host "https://$appName.azurewebsites.net" -ForegroundColor Cyan
    
    Write-Host "`nTest your API:" -ForegroundColor White
    Write-Host "https://$appName.azurewebsites.net/swagger" -ForegroundColor Cyan
    
    Write-Host "`nOpening Swagger UI..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    Start-Process "https://$appName.azurewebsites.net/swagger"
    
} catch {
    Write-Host "`n[ERROR] Deployment failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=====================================" -ForegroundColor Cyan
