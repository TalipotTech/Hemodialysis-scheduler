# ============================================
# Cleanup Old Storage Account Deployment
# ============================================
# This script removes the old Storage Account-based frontend deployment
# Run this AFTER successfully deploying to Static Web Apps

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "EnsateBlogRG"
)

$ErrorActionPreference = "Stop"

$storageAccountName = "hds$($Environment)frontend"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Cleanup Old Storage Account Deployment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will remove the storage account: $storageAccountName" -ForegroundColor Yellow
Write-Host ""

# Check if storage account exists
Write-Host "Checking if storage account exists..." -ForegroundColor Gray
$storageExists = az storage account show `
    --name $storageAccountName `
    --resource-group $ResourceGroup `
    --query "name" `
    --output tsv 2>$null

if ([string]::IsNullOrEmpty($storageExists)) {
    Write-Host "  Storage account '$storageAccountName' not found." -ForegroundColor Green
    Write-Host "  Nothing to clean up." -ForegroundColor Green
    exit 0
}

Write-Host "  Found storage account: $storageAccountName" -ForegroundColor Yellow
Write-Host ""

# Confirm deletion
$confirmation = Read-Host "Are you sure you want to delete this storage account? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Cleanup cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Deleting storage account..." -ForegroundColor Yellow

az storage account delete `
    --name $storageAccountName `
    --resource-group $ResourceGroup `
    --yes

if ($LASTEXITCODE -ne 0) {
    Write-Host "  [ERROR] Failed to delete storage account!" -ForegroundColor Red
    exit 1
}

Write-Host "  [OK] Storage account deleted successfully" -ForegroundColor Green
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Cleanup Complete! ✅" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The old storage account has been removed." -ForegroundColor White
Write-Host "Your frontend is now hosted on Azure Static Web Apps:" -ForegroundColor White
Write-Host "  Default URL: https://lively-pond-08e4f7c00.3.azurestaticapps.net" -ForegroundColor Gray
Write-Host "  Custom URL:  https://dev.dialyzeflow.com" -ForegroundColor Gray
Write-Host ""
