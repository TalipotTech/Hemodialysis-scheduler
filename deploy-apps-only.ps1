# ============================================
# Hemodialysis Scheduler - Application Deployment Only
# ============================================
# This script deploys ONLY the applications (Backend API + Frontend)
# Assumes infrastructure (SQL Server, App Service Plan, Web Apps) already exists
# Use deploy-azure.ps1 for initial infrastructure setup

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "EnsateBlogRG",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipFrontend,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackend
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "HD Scheduler - App Deployment Only" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Resource names
$webAppName = "hds-$Environment-api"
$staticWebAppName = "hds-$Environment-frontend"

# Set subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
# az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"
Write-Host "  [OK] Subscription set (skipped - Azure CLI issue)" -ForegroundColor Yellow
Write-Host ""

# ============================================
# Deploy Backend API
# ============================================
if (-not $SkipBackend) {
    Write-Host "[1/2] Deploying Backend API..." -ForegroundColor Yellow
    Write-Host "  Building application..." -ForegroundColor Gray
    
    Push-Location Backend
    
    # Clean previous builds
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path "publish") { Remove-Item -Recurse -Force "publish" }
    
    # Build and publish to a clean directory
    $publishPath = Join-Path $PWD "publish"
    dotnet publish -c Release -o $publishPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Write-Host "  [OK] Build completed" -ForegroundColor Green
    Write-Host "  Deploying to Azure..." -ForegroundColor Gray
    
    # Create zip for deployment
    Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force
    
    # Deploy to Azure
    az webapp deploy `
        --resource-group $ResourceGroup `
        --name $webAppName `
        --src-path ./deploy.zip `
        --type zip
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Deployment failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    # Cleanup
    Remove-Item ./deploy.zip -Force
    Remove-Item -Recurse -Force ./publish
    
    Pop-Location
    
    Write-Host "  [OK] Backend deployed successfully" -ForegroundColor Green
    Write-Host "  URL: https://$webAppName.azurewebsites.net" -ForegroundColor White
} else {
    Write-Host "[1/2] Skipping Backend deployment" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# Deploy Frontend
# ============================================
if (-not $SkipFrontend) {
    Write-Host "[2/2] Deploying Frontend..." -ForegroundColor Yellow
    
    # Get Static Web App deployment token
    Write-Host "  Getting deployment token..." -ForegroundColor Gray
    $staticWebAppName = "hds-$Environment-frontend"
    $deployToken = az staticwebapp secrets list `
        --name $staticWebAppName `
        --resource-group $ResourceGroup `
        --query "properties.apiKey" `
        --output tsv
    
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrEmpty($deployToken)) {
        Write-Host "  [ERROR] Failed to get deployment token!" -ForegroundColor Red
        Write-Host "  Make sure Static Web App '$staticWebAppName' exists" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "  Building Angular application..." -ForegroundColor Gray
    
    Push-Location Frontend/hd-scheduler-app
    
    # Install dependencies if needed
    if (-not (Test-Path "node_modules")) {
        Write-Host "  Installing dependencies..." -ForegroundColor Gray
        npm install
    }
    
    # Build for production (uses environment.ts via Angular file replacements)
    Write-Host "    - Configuration: production" -ForegroundColor DarkGray
    Write-Host "    - Environment: environment.ts" -ForegroundColor DarkGray
    Write-Host "    - API URL: https://hds-$Environment-api.azurewebsites.net" -ForegroundColor DarkGray
    
    npm run build -- --configuration production
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    # Verify build output
    if (Test-Path "./dist/hd-scheduler-app/browser/index.html") {
        Write-Host "  [OK] Build completed (production environment)" -ForegroundColor Green
    } else {
        Write-Host "  [ERROR] Build output not found!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Write-Host "  Deploying to Azure Static Web Apps..." -ForegroundColor Gray
    
    # Deploy to Azure Static Web Apps
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
    
    # Get Static Web App details
    $swaDetails = az staticwebapp show `
        --name $staticWebAppName `
        --resource-group $ResourceGroup `
        --query "{defaultHostname:defaultHostname, customDomains:customDomains}" `
        --output json | ConvertFrom-Json
    
    Write-Host "  [OK] Frontend deployed successfully" -ForegroundColor Green
    Write-Host "  Default URL: https://$($swaDetails.defaultHostname)" -ForegroundColor White
    if ($swaDetails.customDomains -and $swaDetails.customDomains.Count -gt 0) {
        Write-Host "  Custom URL:  https://$($swaDetails.customDomains[0])" -ForegroundColor White
    }
} else {
    Write-Host "[2/2] Skipping Frontend deployment" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

if (-not $SkipBackend) {
    Write-Host "Backend API:  https://$webAppName.azurewebsites.net" -ForegroundColor White
}

if (-not $SkipFrontend) {
    $swaDetails = az staticwebapp show `
        --name $staticWebAppName `
        --resource-group $ResourceGroup `
        --query "{defaultHostname:defaultHostname, customDomains:customDomains}" `
        --output json | ConvertFrom-Json
    
    Write-Host "Frontend:     https://$($swaDetails.defaultHostname)" -ForegroundColor White
    if ($swaDetails.customDomains -and $swaDetails.customDomains.Count -gt 0) {
        Write-Host "Custom:       https://$($swaDetails.customDomains[0])" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  Deploy both:           .\deploy-apps-only.ps1" -ForegroundColor Gray
Write-Host "  Backend only:          .\deploy-apps-only.ps1 -SkipFrontend" -ForegroundColor Gray
Write-Host "  Frontend only:         .\deploy-apps-only.ps1 -SkipBackend" -ForegroundColor Gray
Write-Host "  Specific environment:  .\deploy-apps-only.ps1 -Environment prod" -ForegroundColor Gray
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
