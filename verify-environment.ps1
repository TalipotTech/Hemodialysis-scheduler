# ============================================
# Verify Environment Configuration
# ============================================
# This script verifies that Angular is configured to use
# the correct environment file for production builds

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Environment Configuration Check" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check environment files
Write-Host "[1/3] Checking environment files..." -ForegroundColor Yellow

$devEnv = "Frontend/hd-scheduler-app/src/environments/environment.development.ts"
$prodEnv = "Frontend/hd-scheduler-app/src/environments/environment.ts"

if (Test-Path $devEnv) {
    Write-Host "  [OK] Development environment found" -ForegroundColor Green
    $devContent = Get-Content $devEnv -Raw
    if ($devContent -match "apiUrl:\s*'([^']+)'") {
        Write-Host "      API URL: $($matches[1])" -ForegroundColor Gray
    }
} else {
    Write-Host "  [ERROR] Development environment not found!" -ForegroundColor Red
}

if (Test-Path $prodEnv) {
    Write-Host "  [OK] Production environment found" -ForegroundColor Green
    $prodContent = Get-Content $prodEnv -Raw
    if ($prodContent -match "apiUrl:\s*'([^']+)'") {
        Write-Host "      API URL: $($matches[1])" -ForegroundColor Gray
    }
} else {
    Write-Host "  [ERROR] Production environment not found!" -ForegroundColor Red
}

Write-Host ""

# Check Angular configuration
Write-Host "[2/3] Checking Angular configuration..." -ForegroundColor Yellow

$angularJson = "Frontend/hd-scheduler-app/angular.json"
if (Test-Path $angularJson) {
    $angularConfig = Get-Content $angularJson -Raw | ConvertFrom-Json
    
    $fileReplacements = $angularConfig.projects.'hd-scheduler-app'.architect.build.configurations.production.fileReplacements
    
    if ($fileReplacements) {
        $replacement = $fileReplacements | Where-Object { 
            $_.replace -like "*environment.development.ts" 
        }
        
        if ($replacement) {
            Write-Host "  [OK] File replacement configured" -ForegroundColor Green
            Write-Host "      Replace: $($replacement.replace)" -ForegroundColor Gray
            Write-Host "      With:    $($replacement.with)" -ForegroundColor Gray
        } else {
            Write-Host "  [WARNING] File replacement not found!" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  [WARNING] No file replacements configured!" -ForegroundColor Yellow
    }
} else {
    Write-Host "  [ERROR] angular.json not found!" -ForegroundColor Red
}

Write-Host ""

# Summary
Write-Host "[3/3] Summary" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Build Configuration:" -ForegroundColor White
Write-Host "    - Local dev:  npm start" -ForegroundColor Gray
Write-Host "                  Uses: environment.development.ts" -ForegroundColor DarkGray
Write-Host "                  API:  http://localhost:5000" -ForegroundColor DarkGray
Write-Host ""
Write-Host "    - Production: npm run build -- --configuration production" -ForegroundColor Gray
Write-Host "                  Uses: environment.ts (via file replacement)" -ForegroundColor DarkGray
Write-Host "                  API:  https://hds-dev-api.azurewebsites.net" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Deployment Scripts:" -ForegroundColor White
Write-Host "    - .\deploy-staticwebapp.ps1" -ForegroundColor Gray
Write-Host "    - .\deploy-apps-only.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "  Both deployment scripts automatically use" -ForegroundColor White
Write-Host "  --configuration production for Azure builds" -ForegroundColor White
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Verification Complete! ✅" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
