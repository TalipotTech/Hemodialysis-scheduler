# Simple Deployment Script
# This deploys your backend to Azure

$zipPath = ".\Backend\deploy.zip"
$appName = "hds-dev-api"

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Simple Azure Deployment" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

# Check if file exists
if (-not (Test-Path $zipPath)) {
    Write-Host "[ERROR] deploy.zip not found at: $zipPath" -ForegroundColor Red
    exit 1
}

Write-Host "`n[OK] Found deployment package" -ForegroundColor Green
$size = (Get-Item $zipPath).Length / 1MB
Write-Host "Size: $([math]::Round($size, 2)) MB" -ForegroundColor White

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "MANUAL DEPLOYMENT STEPS" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`nSince Azure CLI needs a restart, here's what to do:" -ForegroundColor White

Write-Host "`nOPTION 1: Use FTP Client (Recommended)" -ForegroundColor Green
Write-Host "1. Download FileZilla or WinSCP" -ForegroundColor Gray
Write-Host "2. Get FTP credentials from Azure Portal:" -ForegroundColor Gray
Write-Host "   App Service → Deployment Center → FTPS credentials" -ForegroundColor Gray
Write-Host "3. Connect and upload deploy.zip to /site/wwwroot" -ForegroundColor Gray
Write-Host "4. Extract it there" -ForegroundColor Gray

Write-Host "`nOPTION 2: Use Visual Studio Code" -ForegroundColor Green
Write-Host "1. Install 'Azure App Service' extension in VS Code" -ForegroundColor Gray
Write-Host "2. Right-click on Backend folder" -ForegroundColor Gray
Write-Host "3. Select 'Deploy to Web App'" -ForegroundColor Gray
Write-Host "4. Choose hds-dev-api" -ForegroundColor Gray

Write-Host "`nOPTION 3: Restart PowerShell and use Azure CLI" -ForegroundColor Green
Write-Host "1. Close this PowerShell window" -ForegroundColor Gray
Write-Host "2. Open NEW PowerShell as Administrator" -ForegroundColor Gray
Write-Host "3. Run: az login" -ForegroundColor Gray
Write-Host "4. Run: az webapp deploy --resource-group EnsateBlogRG --name $appName --src-path $zipPath --type zip" -ForegroundColor Gray

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "FASTEST: Copy/Paste this command in Kudu SSH" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "`nIn the Kudu terminal (browser), type:" -ForegroundColor White
Write-Host ""
Write-Host "cd /home/site/wwwroot && rm -rf * && curl -L -o deploy.zip 'https://www.dropbox.com/...' && unzip -o deploy.zip && rm deploy.zip" -ForegroundColor Cyan
Write-Host ""
Write-Host "But first, upload deploy.zip to a file sharing service like:" -ForegroundColor Gray
Write-Host "- https://transfer.sh" -ForegroundColor Gray
Write-Host "- Dropbox" -ForegroundColor Gray
Write-Host "- OneDrive" -ForegroundColor Gray
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Recommended: Option 3 (Restart PowerShell)" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
