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
$frontendStorageAccount = "hds$($Environment)frontend"

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
    
    # Build and publish
    dotnet publish -c Release -o ./publish
    
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
    Write-Host "  Building Angular application..." -ForegroundColor Gray
    
    Push-Location Frontend/hd-scheduler-app
    
    # Install dependencies if needed
    if (-not (Test-Path "node_modules")) {
        Write-Host "  Installing dependencies..." -ForegroundColor Gray
        npm install
    }
    
    # Build for production
    npm run build -- --configuration production
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Write-Host "  [OK] Build completed" -ForegroundColor Green
    Write-Host "  Deploying to Azure Static Web Apps..." -ForegroundColor Gray
    
    # Deploy to Azure Static Web Apps (or Storage Account with CDN)
    # Note: Adjust this based on your frontend hosting solution
    # Example for Azure Static Web Apps:
    # swa deploy ./dist/hd-scheduler-app --env production
    
    # Example for Azure Storage + CDN:
    az storage blob upload-batch `
        --account-name $frontendStorageAccount `
        --destination '$web' `
        --source ./dist/hd-scheduler-app/browser `
        --overwrite
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [ERROR] Deployment failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Pop-Location
    
    Write-Host "  [OK] Frontend deployed successfully" -ForegroundColor Green
    Write-Host "  URL: https://$frontendStorageAccount.z29.web.core.windows.net" -ForegroundColor White
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
    Write-Host "Frontend:     https://$frontendStorageAccount.z29.web.core.windows.net" -ForegroundColor White
}

Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  Deploy both:           .\deploy-apps-only.ps1" -ForegroundColor Gray
Write-Host "  Backend only:          .\deploy-apps-only.ps1 -SkipFrontend" -ForegroundColor Gray
Write-Host "  Frontend only:         .\deploy-apps-only.ps1 -SkipBackend" -ForegroundColor Gray
Write-Host "  Specific environment:  .\deploy-apps-only.ps1 -Environment prod" -ForegroundColor Gray
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
