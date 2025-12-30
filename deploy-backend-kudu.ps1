# Deploy Backend using Kudu REST API (no Azure CLI needed)
param(
    [string]$WebAppName = "hds-dev-api",
    [string]$ResourceGroup = "EnsateBlogRG"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Backend Deployment via Kudu API" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Build the backend
Write-Host "`nBuilding Backend..." -ForegroundColor Yellow
Push-Location Backend

# Clean previous builds
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "publish") { Remove-Item -Recurse -Force "publish" }

# Build and publish
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "Build completed successfully" -ForegroundColor Green

# Create deployment package
Write-Host "`nCreating deployment package..." -ForegroundColor Yellow
if (Test-Path "deploy.zip") { Remove-Item "deploy.zip" -Force }
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

Write-Host "Deployment package created" -ForegroundColor Green

Pop-Location

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Deployment package ready!" -ForegroundColor Green
Write-Host "Location: Backend\deploy.zip" -ForegroundColor White
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "`nTo deploy manually:" -ForegroundColor Yellow
Write-Host "1. Go to Azure Portal" -ForegroundColor White
Write-Host "2. Navigate to your App Service: $WebAppName" -ForegroundColor White
Write-Host "3. Go to Development Tools > Advanced Tools > Go" -ForegroundColor White
Write-Host "4. Click 'Debug console' > 'CMD'" -ForegroundColor White
Write-Host "5. Navigate to site/wwwroot" -ForegroundColor White
Write-Host "6. Drag and drop Backend\deploy.zip to upload" -ForegroundColor White
Write-Host "7. Wait for automatic extraction" -ForegroundColor White
Write-Host "`nOR use the Azure portal deployment center to upload the zip file" -ForegroundColor Yellow
