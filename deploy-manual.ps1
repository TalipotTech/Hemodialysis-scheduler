# ============================================
# Quick Azure Deployment - Manual Upload Method
# ============================================
# Use this if Azure CLI has issues
# Opens Azure Portal pages for manual upload

param(
    [string]$ResourceGroup = "EnsateBlogRG",
    [string]$WebAppName = "hds-dev-api",
    [string]$FrontendStorage = "hdsdevfrontend"
)

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Azure Manual Deployment Helper" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Check if deployment packages exist
$backendZip = ".\Backend\deploy.zip"
$frontendDist = ".\Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser"

Write-Host "`nChecking deployment packages..." -ForegroundColor Yellow

if (Test-Path $backendZip) {
    $size = (Get-Item $backendZip).Length / 1MB
    Write-Host "  [OK] Backend: deploy.zip ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] Backend package not found!" -ForegroundColor Red
    Write-Host "    Run: cd Backend; dotnet publish -c Release -o ./publish; Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip" -ForegroundColor Yellow
    exit 1
}

if (Test-Path $frontendDist) {
    Write-Host "  [OK] Frontend: dist folder ready" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] Frontend build not found!" -ForegroundColor Red
    Write-Host "    Run: cd Frontend\hd-scheduler-app; npm run build" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Opening Azure Portal..." -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

# Open backend deployment page
$backendUrl = "https://portal.azure.com/#@ensate365.onmicrosoft.com/resource/subscriptions/74dc21b3-629c-40c3-aa0b-935da454b3e4/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$WebAppName/vstscd"

Write-Host "`n[1] Backend Deployment:" -ForegroundColor Yellow
Write-Host "  Opening: App Service Deployment Center" -ForegroundColor Gray
Write-Host "  Resource: $WebAppName" -ForegroundColor White
Write-Host "  File to upload: Backend\deploy.zip" -ForegroundColor White

Start-Process $backendUrl

Write-Host "`n  Instructions:" -ForegroundColor Cyan
Write-Host "    1. Wait for Azure Portal to load" -ForegroundColor Gray
Write-Host "    2. Click 'ZIP Deploy' or 'Local Git'" -ForegroundColor Gray
Write-Host "    3. Upload: Backend\deploy.zip" -ForegroundColor Gray
Write-Host "    4. Wait for deployment (1-2 minutes)" -ForegroundColor Gray
Write-Host "    5. Verify: https://$WebAppName.azurewebsites.net/swagger" -ForegroundColor Gray

Start-Sleep -Seconds 3

# Open frontend storage account
$storageUrl = "https://portal.azure.com/#@ensate365.onmicrosoft.com/resource/subscriptions/74dc21b3-629c-40c3-aa0b-935da454b3e4/resourceGroups/$ResourceGroup/providers/Microsoft.Storage/storageAccounts/$FrontendStorage/overview"

Write-Host "`n[2] Frontend Deployment:" -ForegroundColor Yellow
Write-Host "  Opening: Storage Account" -ForegroundColor Gray
Write-Host "  Resource: $FrontendStorage" -ForegroundColor White
Write-Host "  Folder to upload: Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser\" -ForegroundColor White

Start-Process $storageUrl

Write-Host "`n  Instructions:" -ForegroundColor Cyan
Write-Host "    1. Wait for Azure Portal to load" -ForegroundColor Gray
Write-Host "    2. Navigate to: Data storage → Containers → `$web" -ForegroundColor Gray
Write-Host "    3. Click 'Upload'" -ForegroundColor Gray
Write-Host "    4. Select all files from: Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser\" -ForegroundColor Gray
Write-Host "    5. Enable 'Overwrite if files already exist'" -ForegroundColor Gray
Write-Host "    6. Click 'Upload' and wait" -ForegroundColor Gray
Write-Host "    7. Get frontend URL from Storage Account → Static website" -ForegroundColor Gray

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "File Paths for Upload:" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`nBackend:" -ForegroundColor White
$backendFullPath = Resolve-Path $backendZip
Write-Host "  $backendFullPath" -ForegroundColor Gray

Write-Host "`nFrontend:" -ForegroundColor White
$frontendFullPath = Resolve-Path $frontendDist
Write-Host "  $frontendFullPath" -ForegroundColor Gray

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Quick Access Links:" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "`nAzure Portal Home:" -ForegroundColor White
Write-Host "  https://portal.azure.com" -ForegroundColor Gray
Write-Host "`nResource Group:" -ForegroundColor White
Write-Host "  https://portal.azure.com/#@ensate365.onmicrosoft.com/resource/subscriptions/74dc21b3-629c-40c3-aa0b-935da454b3e4/resourceGroups/$ResourceGroup/overview" -ForegroundColor Gray

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "After deployment, test here:" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Backend API: https://$WebAppName.azurewebsites.net" -ForegroundColor White
Write-Host "  Swagger UI:  https://$WebAppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host "  Frontend:    [Get URL from Storage Account → Static website]" -ForegroundColor White

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Test Login Credentials:" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Admin:" -ForegroundColor White
Write-Host "    Username: admin" -ForegroundColor Gray
Write-Host "    Password: Admin@123" -ForegroundColor Gray
Write-Host "  Doctor:" -ForegroundColor White
Write-Host "    Username: doctor1" -ForegroundColor Gray
Write-Host "    Password: Doctor@123" -ForegroundColor Gray

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Press any key to open file explorer to deployment packages..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Open file explorer to backend package
explorer.exe "/select,$backendFullPath"

Start-Sleep -Seconds 1

# Open file explorer to frontend folder
explorer.exe "$frontendFullPath"

Write-Host "`n[OK] File explorers opened!" -ForegroundColor Green
Write-Host "  You can now drag and drop files to Azure Portal" -ForegroundColor White
Write-Host "`n=====================================" -ForegroundColor Cyan
