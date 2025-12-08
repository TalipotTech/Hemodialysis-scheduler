# DialyzeFlow Development Deployment Script
# Deploys both Backend API and Frontend to Azure

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("backend", "frontend", "both")]
    [string]$Target = "both",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  DialyzeFlow Development Deployment" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$config = @{
    Backend = @{
        ResourceGroup = "EnsateBlogRG"
        AppName = "hds-dev-api"
        ProjectPath = "$projectRoot\Backend"
        PublishPath = "$projectRoot\Backend\publish"
        DeployZip = "$projectRoot\Backend\deploy.zip"
    }
    Frontend = @{
        ResourceGroup = "EnsateBlogRG"
        AppName = "hds-dev-frontend"
        ProjectPath = "$projectRoot\Frontend\hd-scheduler-app"
        BuildPath = "$projectRoot\Frontend\hd-scheduler-app\dist\hd-scheduler-app"
        BrowserPath = "$projectRoot\Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser"
    }
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n>> $Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "   ✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "   ✗ $Message" -ForegroundColor Red
}

function Deploy-Backend {
    Write-Step "Deploying Backend API..."
    
    try {
        # Navigate to backend
        Set-Location $config.Backend.ProjectPath
        
        if (-not $SkipBuild) {
            Write-Step "Building Backend..."
            dotnet publish -c Release -o $config.Backend.PublishPath
            if ($LASTEXITCODE -ne 0) { throw "Backend build failed" }
            Write-Success "Backend built successfully"
        } else {
            Write-Host "   Skipping build (using existing build)" -ForegroundColor Gray
        }
        
        # Create deployment package
        Write-Step "Creating deployment package..."
        if (Test-Path $config.Backend.DeployZip) {
            Remove-Item $config.Backend.DeployZip -Force
        }
        Compress-Archive -Path "$($config.Backend.PublishPath)\*" -DestinationPath $config.Backend.DeployZip -Force
        Write-Success "Deployment package created"
        
        # Deploy to Azure
        Write-Step "Deploying to Azure Web App..."
        az webapp deploy `
            --resource-group $config.Backend.ResourceGroup `
            --name $config.Backend.AppName `
            --src-path $config.Backend.DeployZip `
            --type zip `
            --async false
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Backend deployed successfully"
            Write-Host "   URL: https://$($config.Backend.AppName).azurewebsites.net" -ForegroundColor Cyan
        } else {
            throw "Backend deployment failed"
        }
        
    } catch {
        Write-Error "Backend deployment failed: $_"
        return $false
    } finally {
        Set-Location $projectRoot
    }
    
    return $true
}

function Deploy-Frontend {
    Write-Step "Deploying Frontend..."
    
    try {
        # Navigate to frontend
        Set-Location $config.Frontend.ProjectPath
        
        if (-not $SkipBuild) {
            Write-Step "Building Frontend..."
            npm run build -- --output-path=$($config.Frontend.BuildPath)
            if ($LASTEXITCODE -ne 0) { throw "Frontend build failed" }
            Write-Success "Frontend built successfully"
        } else {
            Write-Host "   Skipping build (using existing build)" -ForegroundColor Gray
        }
        
        # Check if build exists
        if (-not (Test-Path $config.Frontend.BrowserPath)) {
            throw "Build output not found at $($config.Frontend.BrowserPath)"
        }
        
        # Deploy to Azure Static Web App
        Write-Step "Deploying to Azure Static Web App..."
        
        # Get deployment token
        $deployToken = az staticwebapp secrets list `
            --name $config.Frontend.AppName `
            --resource-group $config.Frontend.ResourceGroup `
            --query "properties.apiKey" -o tsv
        
        if ([string]::IsNullOrWhiteSpace($deployToken)) {
            throw "Failed to retrieve deployment token"
        }
        
        # Deploy using SWA CLI
        swa deploy $config.Frontend.BrowserPath `
            --deployment-token $deployToken `
            --env production
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Frontend deployed successfully"
            Write-Host "   Azure URL: https://lively-pond-08e4f7c00.3.azurestaticapps.net" -ForegroundColor Cyan
            Write-Host "   Custom URL: https://dev.dialyzeflow.com" -ForegroundColor Cyan
        } else {
            throw "Frontend deployment failed"
        }
        
    } catch {
        Write-Error "Frontend deployment failed: $_"
        return $false
    } finally {
        Set-Location $projectRoot
    }
    
    return $true
}

function Show-Summary {
    param([bool]$BackendSuccess, [bool]$FrontendSuccess)
    
    Write-Host "`n==================================================" -ForegroundColor Cyan
    Write-Host "  Deployment Summary" -ForegroundColor Cyan
    Write-Host "==================================================" -ForegroundColor Cyan
    
    if ($Target -eq "both" -or $Target -eq "backend") {
        if ($BackendSuccess) {
            Write-Host "  Backend:  " -NoNewline -ForegroundColor White
            Write-Host "✓ Deployed" -ForegroundColor Green
        } else {
            Write-Host "  Backend:  " -NoNewline -ForegroundColor White
            Write-Host "✗ Failed" -ForegroundColor Red
        }
    }
    
    if ($Target -eq "both" -or $Target -eq "frontend") {
        if ($FrontendSuccess) {
            Write-Host "  Frontend: " -NoNewline -ForegroundColor White
            Write-Host "✓ Deployed" -ForegroundColor Green
        } else {
            Write-Host "  Frontend: " -NoNewline -ForegroundColor White
            Write-Host "✗ Failed" -ForegroundColor Red
        }
    }
    
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host ""
}

# Main execution
$backendSuccess = $true
$frontendSuccess = $true

try {
    # Check Azure CLI
    Write-Step "Checking prerequisites..."
    $azVersion = az --version 2>$null
    if (-not $azVersion) {
        throw "Azure CLI not found. Please install from https://aka.ms/installazurecli"
    }
    Write-Success "Azure CLI found"
    
    # Check if logged in
    $account = az account show 2>$null
    if (-not $account) {
        throw "Not logged into Azure. Please run 'az login'"
    }
    Write-Success "Azure authentication verified"
    
    if ($Target -eq "both" -or $Target -eq "backend") {
        # Check .NET SDK
        $dotnetVersion = dotnet --version 2>$null
        if (-not $dotnetVersion) {
            throw ".NET SDK not found. Please install from https://dotnet.microsoft.com/download"
        }
        Write-Success ".NET SDK found (v$dotnetVersion)"
    }
    
    if ($Target -eq "both" -or $Target -eq "frontend") {
        # Check Node.js and npm
        Set-Location $config.Frontend.ProjectPath
        $nodeVersion = node --version 2>$null
        if (-not $nodeVersion) {
            throw "Node.js not found. Please install from https://nodejs.org/"
        }
        Write-Success "Node.js found ($nodeVersion)"
        
        # Check SWA CLI
        $swaVersion = swa --version 2>$null
        if (-not $swaVersion) {
            Write-Host "   Static Web Apps CLI not found. Installing..." -ForegroundColor Yellow
            npm install -g @azure/static-web-apps-cli
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to install SWA CLI"
            }
            Write-Success "SWA CLI installed"
        } else {
            Write-Success "SWA CLI found (v$swaVersion)"
        }
        
        Set-Location $projectRoot
    }
    
    Write-Success "All prerequisites met"
    
    # Execute deployments
    if ($Target -eq "both" -or $Target -eq "backend") {
        $backendSuccess = Deploy-Backend
    }
    
    if ($Target -eq "both" -or $Target -eq "frontend") {
        $frontendSuccess = Deploy-Frontend
    }
    
} catch {
    Write-Error "Deployment failed: $_"
    exit 1
} finally {
    Set-Location $projectRoot
    Show-Summary -BackendSuccess $backendSuccess -FrontendSuccess $frontendSuccess
}

# Exit with appropriate code
if (($Target -eq "both" -and $backendSuccess -and $frontendSuccess) -or
    ($Target -eq "backend" -and $backendSuccess) -or
    ($Target -eq "frontend" -and $frontendSuccess)) {
    Write-Host "Deployment completed successfully! 🚀" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Deployment completed with errors." -ForegroundColor Red
    exit 1
}
