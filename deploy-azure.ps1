# ============================================
# Hemodialysis Scheduler - Azure Deployment Script
# ============================================
# This script creates all necessary Azure resources for the HD Scheduler application
# Environment: Development
# Location: South India
# Resource Group: EnsateBlogRG

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "southindia",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlLocation = "centralindia",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "EnsateBlogRG",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlAdminPassword
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "HD Scheduler Azure Deployment" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "App Services Location: $Location" -ForegroundColor Cyan
Write-Host "SQL Server Location: $SqlLocation" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Login to Azure (uncomment if needed)
# Write-Host "Logging into Azure..." -ForegroundColor Yellow
# az login --tenant ensate365.onmicrosoft.com

# Set subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"

# Resource names - SQL Server includes region suffix due to existing South India server
$sqlServerName = "hds-$Environment-sqlserver-cin"
$databaseName = "hds-$Environment-db"
$appServicePlanName = "hds-$Environment-plan"
$webAppName = "hds-$Environment-api"
$sqlAdminUser = "hdsadmin"

# Prompt for SQL password if not provided
if ([string]::IsNullOrEmpty($SqlAdminPassword)) {
    $securePassword = Read-Host "Enter SQL Server admin password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $SqlAdminPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

Write-Host ""
Write-Host "Creating Azure Resources..." -ForegroundColor Green
Write-Host "  SQL Server: $sqlServerName (in $SqlLocation)" -ForegroundColor Gray
Write-Host "  Database: $databaseName" -ForegroundColor Gray
Write-Host "  App Service Plan: $appServicePlanName (in $Location)" -ForegroundColor Gray
Write-Host "  Web App: $webAppName (in $Location)" -ForegroundColor Gray
Write-Host ""
Write-Host "  Note: Using '-eus' suffix for SQL Server to avoid conflict with existing South India server" -ForegroundColor Yellow
Write-Host ""

# 1. Create SQL Server (in East US due to South India restrictions)
Write-Host "[1/4] Creating SQL Server in $SqlLocation..." -ForegroundColor Yellow
Write-Host "  Note: SQL Server must be in East US (South India not accepting new SQL Servers)" -ForegroundColor Gray
$sqlServerExists = az sql server show --name $sqlServerName --resource-group $ResourceGroup 2>$null
if ($null -eq $sqlServerExists) {
    az sql server create `
        --name $sqlServerName `
        --resource-group $ResourceGroup `
        --location $SqlLocation `
        --admin-user $sqlAdminUser `
        --admin-password $SqlAdminPassword
    
    Write-Host "  ✓ SQL Server created successfully" -ForegroundColor Green
    
    # Configure firewall rule to allow Azure services
    Write-Host "  Configuring firewall rules..." -ForegroundColor Gray
    az sql server firewall-rule create `
        --resource-group $ResourceGroup `
        --server $sqlServerName `
        --name "AllowAzureServices" `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0
    
    Write-Host "  ✓ Firewall rules configured" -ForegroundColor Green
} else {
    Write-Host "  ✓ SQL Server already exists" -ForegroundColor Green
}

# 2. Create SQL Database
Write-Host ""
Write-Host "[2/4] Creating SQL Database..." -ForegroundColor Yellow
$databaseExists = az sql db show --name $databaseName --server $sqlServerName --resource-group $ResourceGroup 2>$null
if ($null -eq $databaseExists) {
    az sql db create `
        --resource-group $ResourceGroup `
        --server $sqlServerName `
        --name $databaseName `
        --service-objective S0 `
        --backup-storage-redundancy Local
    
    Write-Host "  ✓ Database created successfully" -ForegroundColor Green
} else {
    Write-Host "  ✓ Database already exists" -ForegroundColor Green
}

# 3. Create App Service Plan (Linux Basic B1) - in same region as other resources
Write-Host ""
Write-Host "[3/4] Creating App Service Plan in $Location..." -ForegroundColor Yellow
$planExists = az appservice plan show --name $appServicePlanName --resource-group $ResourceGroup 2>$null
if ($null -eq $planExists) {
    az appservice plan create `
        --name $appServicePlanName `
        --resource-group $ResourceGroup `
        --location $Location `
        --is-linux `
        --sku B1
    
    Write-Host "  ✓ App Service Plan created successfully" -ForegroundColor Green
} else {
    Write-Host "  ✓ App Service Plan already exists" -ForegroundColor Green
}

# 4. Create Web App
Write-Host ""
Write-Host "[4/4] Creating Web App..." -ForegroundColor Yellow
$webAppExists = az webapp show --name $webAppName --resource-group $ResourceGroup 2>$null
if ($null -eq $webAppExists) {
    az webapp create `
        --name $webAppName `
        --resource-group $ResourceGroup `
        --plan $appServicePlanName `
        --runtime "DOTNETCORE:8.0"
    
    Write-Host "  ✓ Web App created successfully" -ForegroundColor Green
    
    # Configure connection string
    Write-Host "  Configuring connection string..." -ForegroundColor Gray
    $connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=$sqlAdminUser;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    az webapp config connection-string set `
        --name $webAppName `
        --resource-group $ResourceGroup `
        --connection-string-type SQLAzure `
        --settings DefaultConnection="$connectionString"
    
    # Configure app settings
    az webapp config appsettings set `
        --name $webAppName `
        --resource-group $ResourceGroup `
        --settings ASPNETCORE_ENVIRONMENT=$Environment
    
    Write-Host "  ✓ Configuration completed" -ForegroundColor Green
} else {
    Write-Host "  ✓ Web App already exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SQL Server:        $sqlServerName.database.windows.net (Location: $SqlLocation)" -ForegroundColor White
Write-Host "Database:          $databaseName" -ForegroundColor White
Write-Host "Web App URL:       https://$webAppName.azurewebsites.net (Location: $Location)" -ForegroundColor White
Write-Host "Admin User:        $sqlAdminUser" -ForegroundColor White
Write-Host ""
Write-Host "Note: SQL Server is in $SqlLocation while App Services are in $Location" -ForegroundColor Yellow
Write-Host "      This is normal and won't affect performance significantly." -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Add your IP to SQL Server firewall:" -ForegroundColor Gray
Write-Host "   az sql server firewall-rule create --resource-group $ResourceGroup --server $sqlServerName --name 'MyIP' --start-ip-address <YOUR_IP> --end-ip-address <YOUR_IP>" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run database schema script:" -ForegroundColor Gray
Write-Host "   sqlcmd -S $sqlServerName.database.windows.net -U $sqlAdminUser -P <password> -d $databaseName -i Database/SqlServer/01_CreateSchema.sql" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Run seed data script:" -ForegroundColor Gray
Write-Host "   sqlcmd -S $sqlServerName.database.windows.net -U $sqlAdminUser -P <password> -d $databaseName -i Database/SqlServer/02_SeedData.sql" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Deploy application:" -ForegroundColor Gray
Write-Host "   cd Backend && dotnet publish -c Release && az webapp deploy --resource-group $ResourceGroup --name $webAppName --src-path ./bin/Release/net8.0/publish.zip" -ForegroundColor Gray
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
