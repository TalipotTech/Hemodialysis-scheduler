# Deploy using Kudu API
# This works without Azure CLI!

$appName = "hds-dev-api"
$zipFile = ".\Backend\deploy.zip"
$kuduUrl = "https://$appName.scm.azurewebsites.net/api/zipdeploy"

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Kudu API Deployment" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

# Check if file exists
if (-not (Test-Path $zipFile)) {
    Write-Host "[ERROR] deploy.zip not found!" -ForegroundColor Red
    exit 1
}

Write-Host "`nFound deployment package:" -ForegroundColor Green
$size = (Get-Item $zipFile).Length / 1MB
Write-Host "Size: $([math]::Round($size, 2)) MB" -ForegroundColor White

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Getting Deployment Credentials" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`nYou need deployment credentials from Azure Portal:" -ForegroundColor White
Write-Host "1. Go to: https://portal.azure.com" -ForegroundColor Gray
Write-Host "2. Navigate to App Service: hds-dev-api" -ForegroundColor Gray
Write-Host "3. Go to: Deployment → Deployment Center" -ForegroundColor Gray
Write-Host "4. Click 'FTPS credentials' tab" -ForegroundColor Gray
Write-Host "5. Copy the Username and Password" -ForegroundColor Gray

Write-Host "`nEnter deployment credentials:" -ForegroundColor Yellow
$username = Read-Host "Username (e.g., hds-dev-api\`$hds-dev-api)"
$password = Read-Host "Password" -AsSecureString
$passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

Write-Host "`nDeploying..." -ForegroundColor Yellow

try {
    $base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$username`:$passwordPlain"))
    
    $headers = @{
        Authorization = "Basic $base64Auth"
    }
    
    Write-Host "Uploading deployment package..." -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri $kuduUrl `
        -Method Post `
        -InFile $zipFile `
        -Headers $headers `
        -ContentType "application/zip" `
        -TimeoutSec 600
    
    Write-Host "`n[SUCCESS] Deployment completed!" -ForegroundColor Green
    Write-Host "`nYour backend is now live at:" -ForegroundColor White
    Write-Host "https://$appName.azurewebsites.net" -ForegroundColor Cyan
    Write-Host "`nTest Swagger:" -ForegroundColor White
    Write-Host "https://$appName.azurewebsites.net/swagger" -ForegroundColor Cyan
    
} catch {
    Write-Host "`n[ERROR] Deployment failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Verify credentials are correct" -ForegroundColor Gray
    Write-Host "2. Username format: appname\`$appname or `$appname" -ForegroundColor Gray
    Write-Host "3. Try getting new credentials from Azure Portal" -ForegroundColor Gray
}

Write-Host "`n=====================================" -ForegroundColor Cyan
