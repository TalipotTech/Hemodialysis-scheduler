# ============================================
# Hemodialysis Scheduler - Static Web App Deployment
# ============================================
# This script deploys the Frontend to Azure Static Web Apps
# Properly configured with custom domain support

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "EnsateBlogRG",
    
    [Parameter(Mandatory=$false)]
    [string]$StaticWebAppName = "hds-$Environment-frontend"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "HD Scheduler - Static Web App Deployment" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "[1/5] Checking prerequisites..." -ForegroundColor Yellow

# Check Azure CLI
$azVersion = az --version 2>$null
if (-not $azVersion) {
    Write-Host "  [ERROR] Azure CLI not found!" -ForegroundColor Red
    Write-Host "  Install from: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    exit 1
}
Write-Host "  [OK] Azure CLI found" -ForegroundColor Green

# Check Node.js
$nodeVersion = node --version 2>$null
if (-not $nodeVersion) {
    Write-Host "  [ERROR] Node.js not found!" -ForegroundColor Red
    exit 1
}
Write-Host "  [OK] Node.js $nodeVersion" -ForegroundColor Green

# Check/Install SWA CLI
Write-Host "  Checking Static Web Apps CLI..." -ForegroundColor Gray
$swaVersion = swa --version 2>$null
if (-not $swaVersion) {
    Write-Host "  Installing SWA CLI..." -ForegroundColor Yellow
    npm install -g @azure/static-web-apps-cli
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Failed to install SWA CLI!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  [OK] SWA CLI installed" -ForegroundColor Green
} else {
    Write-Host "  [OK] SWA CLI v$swaVersion" -ForegroundColor Green
}

Write-Host ""

# Get Static Web App deployment token
Write-Host "[2/5] Getting deployment token..." -ForegroundColor Yellow
$deployToken = az staticwebapp secrets list `
    --name $StaticWebAppName `
    --resource-group $ResourceGroup `
    --query "properties.apiKey" `
    --output tsv

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrEmpty($deployToken)) {
    Write-Host "  [ERROR] Failed to get deployment token!" -ForegroundColor Red
    Write-Host "  Make sure the Static Web App '$StaticWebAppName' exists in resource group '$ResourceGroup'" -ForegroundColor Yellow
    exit 1
}
Write-Host "  [OK] Deployment token retrieved" -ForegroundColor Green
Write-Host ""

# Build the Angular app
Write-Host "[3/5] Building Angular application..." -ForegroundColor Yellow
Push-Location Frontend/hd-scheduler-app

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "  Installing dependencies..." -ForegroundColor Gray
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] npm install failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

# Build for production (uses environment.ts via Angular file replacements)
Write-Host "  Building for production..." -ForegroundColor Gray
Write-Host "    - Configuration: production" -ForegroundColor DarkGray
Write-Host "    - Environment: environment.ts (production)" -ForegroundColor DarkGray
Write-Host "    - API URL: https://hds-$Environment-api.azurewebsites.net" -ForegroundColor DarkGray

npm run build -- --configuration production

if ($LASTEXITCODE -ne 0) {
    Write-Host "  [ERROR] Build failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}

# Verify the build used production environment
$indexPath = "./dist/hd-scheduler-app/browser/index.html"
if (Test-Path $indexPath) {
    Write-Host "  [OK] Build completed" -ForegroundColor Green
    Write-Host "    - Output: dist/hd-scheduler-app/browser/" -ForegroundColor DarkGray
} else {
    Write-Host "  [ERROR] Build output not found!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Write-Host ""

# Deploy to Static Web App
Write-Host "[4/5] Deploying to Azure Static Web App..." -ForegroundColor Yellow
Write-Host "  This may take a few minutes..." -ForegroundColor Gray

swa deploy ./dist/hd-scheduler-app/browser `
    --deployment-token $deployToken `
    --env production `
    --no-use-keychain

if ($LASTEXITCODE -ne 0) {
    Write-Host "  [ERROR] Deployment failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location
Write-Host "  [OK] Deployment completed" -ForegroundColor Green
Write-Host ""

# Get Static Web App details
Write-Host "[5/5] Getting deployment details..." -ForegroundColor Yellow
$swaDetails = az staticwebapp show `
    --name $StaticWebAppName `
    --resource-group $ResourceGroup `
    --query "{defaultHostname:defaultHostname, customDomains:customDomains}" `
    --output json | ConvertFrom-Json

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deployment Complete! ✅" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Frontend URLs:" -ForegroundColor Yellow
Write-Host "  Default:  https://$($swaDetails.defaultHostname)" -ForegroundColor White

if ($swaDetails.customDomains -and $swaDetails.customDomains.Count -gt 0) {
    Write-Host "  Custom:   https://$($swaDetails.customDomains[0])" -ForegroundColor White
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Test the application at the URLs above" -ForegroundColor Gray
Write-Host "  2. Verify custom domain is working: https://$($swaDetails.customDomains[0])" -ForegroundColor Gray
Write-Host "  3. Check that no CORS errors occur" -ForegroundColor Gray
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
