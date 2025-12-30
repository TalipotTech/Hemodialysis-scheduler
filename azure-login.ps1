# Azure Login Script
# Run this after restarting PowerShell

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Azure Login Helper" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`nChecking Azure CLI..." -ForegroundColor Yellow

try {
    $azVersion = az --version 2>&1 | Select-Object -First 1
    Write-Host "  [OK] Azure CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Azure CLI not working. Please restart PowerShell." -ForegroundColor Red
    Write-Host "`n  Steps:" -ForegroundColor Yellow
    Write-Host "    1. Close this PowerShell window" -ForegroundColor White
    Write-Host "    2. Open NEW PowerShell as Administrator" -ForegroundColor White
    Write-Host "    3. Run this script again: .\azure-login.ps1" -ForegroundColor White
    exit 1
}

Write-Host "`nLogging into Azure..." -ForegroundColor Yellow
Write-Host "  Tenant: ensate365.onmicrosoft.com" -ForegroundColor Gray
Write-Host "  A browser window will open for authentication" -ForegroundColor Gray
Write-Host ""

az login --tenant ensate365.onmicrosoft.com

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[OK] Successfully logged into Azure!" -ForegroundColor Green
    
    Write-Host "`nSetting subscription..." -ForegroundColor Yellow
    az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Subscription set" -ForegroundColor Green
        
        Write-Host "`n=====================================" -ForegroundColor Cyan
        Write-Host "Account Information:" -ForegroundColor Yellow
        Write-Host "=====================================" -ForegroundColor Cyan
        az account show --output table
        
        Write-Host "`n=====================================" -ForegroundColor Cyan
        Write-Host "What's Next?" -ForegroundColor Yellow
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host "`nNow you can deploy your application:" -ForegroundColor White
        Write-Host "  Option 1: .\deploy-apps-only.ps1" -ForegroundColor Green
        Write-Host "  Option 2: .\deploy-manual.ps1" -ForegroundColor Green
        Write-Host ""
        Write-Host "Or check your Azure resources:" -ForegroundColor White
        Write-Host "  az webapp list --resource-group EnsateBlogRG --output table" -ForegroundColor Gray
        Write-Host "  az sql db list --resource-group EnsateBlogRG --server hds-dev-sqlserver-cin --output table" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "[ERROR] Failed to set subscription" -ForegroundColor Red
    }
} else {
    Write-Host "`n[ERROR] Azure login failed" -ForegroundColor Red
    Write-Host "Please try again or use Azure Portal" -ForegroundColor Yellow
}

Write-Host "`n=====================================" -ForegroundColor Cyan
