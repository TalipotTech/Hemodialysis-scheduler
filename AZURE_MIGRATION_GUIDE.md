# Hemodialysis Scheduler - Azure SQL Server Migration Guide

## Overview
This guide explains the migration from SQLite to Azure SQL Server for multi-developer, multi-tenant deployment.

## ✅ Changes Completed

### 1. **Package Updates** (`HDScheduler.API.csproj`)
- ✅ Removed: `System.Data.SQLite`, `System.Data.SQLite.Core`, `Microsoft.Data.Sqlite`
- ✅ Kept: `Microsoft.Data.SqlClient` (already present)
- ✅ Updated: `System.IdentityModel.Tokens.Jwt` to 8.1.2 (security fix)

### 2. **Connection Context** (`Data/DapperContext.cs`)
- ✅ Changed from `SQLiteConnection` to `SqlConnection`
- ✅ Using `Microsoft.Data.SqlClient` namespace

### 3. **Environment Configuration**
Created three environment-specific appsettings files:

#### `appsettings.Development.json` (Dev Environment)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hds-dev-sqlserver.database.windows.net,1433;Initial Catalog=hds-dev-db;..."
  },
  "Environment": "Development"
}
```

#### `appsettings.Staging.json` (Staging Environment)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hds-staging-sqlserver.database.windows.net,1433;Initial Catalog=hds-staging-db;..."
  },
  "Environment": "Staging"
}
```

#### `appsettings.Production.json` (Production Environment)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hds-prod-sqlserver.database.windows.net,1433;Initial Catalog=hds-prod-db;..."
  },
  "Environment": "Production"
}
```

### 4. **SQL Server Migration Scripts**
Created in `Database/SqlServer/`:

- ✅ `01_CreateSchema.sql` - Complete database schema for SQL Server
- ✅ `02_SeedData.sql` - Seed data with default users and test data

### 5. **Azure Deployment Script**
Created `deploy-azure.ps1` - Automated Azure resource provisioning

## 🚀 Deployment Steps

### Step 1: Install Dependencies
```powershell
cd Backend
dotnet restore
```

### Step 2: Run Azure Deployment Script
```powershell
# Navigate to project root
cd E:\DEVELOPMENT\WEBSITE\ENSATE\CLIENTPROJECTS\Hemodialysis-scheduler

# Run deployment (will prompt for SQL password)
.\deploy-azure.ps1 -Environment dev

# Or specify all parameters
.\deploy-azure.ps1 -Environment dev -SqlAdminPassword "YourSecurePassword123!"
```

**What it creates:**
- SQL Server: `hds-dev-sqlserver.database.windows.net`
- Database: `hds-dev-db`
- App Service Plan: `hds-dev-plan` (Linux, B1)
- Web App: `hds-dev-api.azurewebsites.net`

### Step 3: Configure Your IP for Database Access
```powershell
# Get your public IP
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content

# Add firewall rule
az sql server firewall-rule create `
    --resource-group EnsateBlogRG `
    --server hds-dev-sqlserver `
    --name "MyDevMachine" `
    --start-ip-address $myIp `
    --end-ip-address $myIp
```

### Step 4: Initialize Database Schema
```powershell
# Using sqlcmd (install: https://learn.microsoft.com/sql/tools/sqlcmd-utility)
sqlcmd -S hds-dev-sqlserver.database.windows.net `
       -U hdsadmin `
       -P "YourPassword" `
       -d hds-dev-db `
       -i "Database\SqlServer\01_CreateSchema.sql"

# Run seed data
sqlcmd -S hds-dev-sqlserver.database.windows.net `
       -U hdsadmin `
       -P "YourPassword" `
       -d hds-dev-db `
       -i "Database\SqlServer\02_SeedData.sql"
```

### Step 5: Update Local appsettings with Real Password
Edit `Backend/appsettings.Development.json` and replace `{your_password}` with your actual SQL password.

### Step 6: Test Locally
```powershell
cd Backend
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

Visit: `http://localhost:5001/swagger`

### Step 7: Deploy to Azure
```powershell
cd Backend
dotnet publish -c Release -o ./publish

# Create zip
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy
az webapp deploy `
    --resource-group EnsateBlogRG `
    --name hds-dev-api `
    --src-path ./publish.zip `
    --type zip
```

Visit: `https://hds-dev-api.azurewebsites.net/swagger`

## 🔐 Security Considerations

### 1. **Never Commit Passwords**
Add to `.gitignore`:
```
appsettings.*.json
**/appsettings.Development.json
**/appsettings.Staging.json
**/appsettings.Production.json
```

### 2. **Use Azure Key Vault (Recommended)**
```powershell
# Create Key Vault
az keyvault create `
    --name hds-keyvault `
    --resource-group EnsateBlogRG `
    --location southindia

# Store SQL password
az keyvault secret set `
    --vault-name hds-keyvault `
    --name "SqlAdminPassword" `
    --value "YourSecurePassword123!"

# Grant Web App access
az webapp identity assign `
    --name hds-dev-api `
    --resource-group EnsateBlogRG

# Get managed identity
$identity = az webapp identity show --name hds-dev-api --resource-group EnsateBlogRG --query principalId -o tsv

# Grant Key Vault access
az keyvault set-policy `
    --name hds-keyvault `
    --object-id $identity `
    --secret-permissions get list
```

Update `Program.cs` to use Key Vault:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://hds-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 3. **Change Default User Passwords**
After first deployment, change all default passwords:
- Admin: admin / Admin@123
- HOD: hod / Hod@123
- Doctor: doctor1 / Doctor@123
- Nurse: nurse1 / Nurse@123
- Technician: tech1 / Tech@123

## 📊 Multi-Tenant Architecture

### Current Setup (Development)
- Single database: `hds-dev-db`
- All developers share this database
- Consistent test data

### Future Production Setup
Each hospital/clinic gets:
- Separate database: `hds-client1-db`, `hds-client2-db`, etc.
- Separate web app: `hds-client1-api`, `hds-client2-api`, etc.
- Can use custom domains: `client1.hdscheduler.com`

### Tenant Provisioning Script
```powershell
# Example: Create resources for new client
.\deploy-azure.ps1 -Environment "client1" -SqlAdminPassword "SecurePass123!"
```

## 🔧 Troubleshooting

### Cannot Connect to Database
1. Check firewall rules: `az sql server firewall-rule list --resource-group EnsateBlogRG --server hds-dev-sqlserver`
2. Verify password in connection string
3. Test connection: `sqlcmd -S hds-dev-sqlserver.database.windows.net -U hdsadmin -P YourPassword`

### Web App Not Starting
1. Check logs: `az webapp log tail --name hds-dev-api --resource-group EnsateBlogRG`
2. Verify connection string: `az webapp config connection-string list --name hds-dev-api --resource-group EnsateBlogRG`
3. Check environment: `az webapp config appsettings list --name hds-dev-api --resource-group EnsateBlogRG`

### DatabaseInitializer Errors
The current `DatabaseInitializer.cs` still contains SQLite-specific code. For production, use the SQL Server migration scripts directly:
1. Run `01_CreateSchema.sql` once
2. Run `02_SeedData.sql` once
3. Disable DatabaseInitializer in `Program.cs` (comment out the Initialize call)

## 📋 Resource Naming Convention

All resources start with `hds-` (Hemodialysis Scheduler):

| Resource Type | Naming Pattern | Example |
|--------------|----------------|---------|
| SQL Server | `hds-{env}-sqlserver` | `hds-dev-sqlserver` |
| Database | `hds-{env}-db` | `hds-dev-db` |
| App Service Plan | `hds-{env}-plan` | `hds-dev-plan` |
| Web App | `hds-{env}-api` | `hds-dev-api` |
| Key Vault | `hds-keyvault` | `hds-keyvault` |
| Storage Account | `hds{env}storage` | `hdsdevstorage` |

## 📝 Next Steps

1. ✅ Complete migration to SQL Server
2. ⏭️ Test all API endpoints with Azure SQL
3. ⏭️ Set up CI/CD pipeline (GitHub Actions or Azure DevOps)
4. ⏭️ Configure custom domain for production
5. ⏭️ Implement Azure Application Insights for monitoring
6. ⏭️ Set up automated backups and disaster recovery

## 💰 Cost Estimation

**Development Environment:**
- SQL Database (S0 - 10 DTUs): ~$15/month
- App Service Plan (B1): ~$13/month
- **Total: ~$28/month**

**Per Client (Production):**
- SQL Database (S1 - 20 DTUs): ~$30/month
- App Service Plan (B1): ~$13/month
- **Total: ~$43/month per hospital**

Consider reserved capacity for cost savings.

## 🔗 Useful Links

- [Azure SQL Database Documentation](https://learn.microsoft.com/azure/azure-sql/)
- [App Service Linux Documentation](https://learn.microsoft.com/azure/app-service/quickstart-dotnetcore)
- [Azure CLI Reference](https://learn.microsoft.com/cli/azure/)
- [SQL Server Migration Guide](https://learn.microsoft.com/sql/sql-server/migrate/)

---

**Last Updated**: December 6, 2025
**Contact**: ensate365@ensate365.onmicrosoft.com
