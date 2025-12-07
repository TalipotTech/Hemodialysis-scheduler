# 🚀 Quick Start - Azure SQL Server Setup

## Prerequisites
- Azure CLI installed: `winget install Microsoft.AzureCLI`
- SQL Server Command Line Tools: `winget install Microsoft.SQLServerCommandLineTools`
- .NET 8 SDK installed

## Step-by-Step Setup (10 minutes)

### 1. Login to Azure
```powershell
az login --tenant ensate365.onmicrosoft.com
az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"
```

### 2. Deploy Azure Resources
```powershell
cd E:\DEVELOPMENT\WEBSITE\ENSATE\CLIENTPROJECTS\Hemodialysis-scheduler
.\deploy-azure.ps1 -Environment dev
```
**Enter SQL password when prompted** (e.g., `HDScheduler@2025`)

### 3. Add Your IP to Firewall
```powershell
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az sql server firewall-rule create `
    --resource-group EnsateBlogRG `
    --server hds-dev-sqlserver `
    --name "DevMachine" `
    --start-ip-address $myIp `
    --end-ip-address $myIp
```

### 4. Initialize Database
```powershell
# Set your password
$sqlPassword = "HDScheduler@2025"

# Run schema script
sqlcmd -S hds-dev-sqlserver.database.windows.net `
       -U hdsadmin `
       -P $sqlPassword `
       -d hds-dev-db `
       -i "Database\SqlServer\01_CreateSchema.sql"

# Run seed data script
sqlcmd -S hds-dev-sqlserver.database.windows.net `
       -U hdsadmin `
       -P $sqlPassword `
       -d hds-dev-db `
       -i "Database\SqlServer\02_SeedData.sql"
```

### 5. Update Local Configuration
Edit `Backend/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hds-dev-sqlserver.database.windows.net,1433;Initial Catalog=hds-dev-db;Persist Security Info=False;User ID=hdsadmin;Password=HDScheduler@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 6. Run Backend Locally
```powershell
cd Backend
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet restore
dotnet run
```

Visit: http://localhost:5001/swagger

### 7. Test Login
Use Swagger UI to test `/api/auth/login`:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

## ✅ Success Indicators
- Backend starts without errors
- Swagger UI loads at http://localhost:5001/swagger
- Login API returns JWT token
- No database connection errors in console

## 🔧 Common Issues

### "Cannot connect to SQL Server"
- Check firewall rule: Your IP might have changed
- Re-run step 3 to add current IP
- Verify password in connection string

### "Login failed for user 'hdsadmin'"
- Password might be incorrect
- Check Azure Portal > SQL Server > Reset Password

### "Database 'hds-dev-db' does not exist"
- Re-run step 2 (deploy-azure.ps1)
- Verify database exists: `az sql db list --server hds-dev-sqlserver --resource-group EnsateBlogRG`

## 📋 Default User Accounts

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | Admin@123 |
| HOD | hod | Hod@123 |
| Doctor | doctor1 | Doctor@123 |
| Nurse | nurse1 | Nurse@123 |
| Technician | tech1 | Tech@123 |

**⚠️ Change these passwords after first deployment!**

## 🌐 Azure Resources Created

| Resource | Name | URL/Connection |
|----------|------|----------------|
| SQL Server | hds-dev-sqlserver | hds-dev-sqlserver.database.windows.net |
| Database | hds-dev-db | - |
| App Service Plan | hds-dev-plan | Linux, B1 |
| Web App | hds-dev-api | https://hds-dev-api.azurewebsites.net |

## 📊 Cost
- SQL Database (S0): ~$15/month
- App Service (B1): ~$13/month
- **Total: ~$28/month**

**Note**: Using East US region (South India not available for SQL Server)

## 🎯 Next Steps
1. ✅ Backend running locally with Azure SQL
2. ⏭️ Run frontend: `cd Frontend/hd-scheduler-app && npm start`
3. ⏭️ Deploy to Azure Web App (see AZURE_MIGRATION_GUIDE.md)
4. ⏭️ Set up staging environment
5. ⏭️ Configure CI/CD pipeline

---
**Need Help?** Check `AZURE_MIGRATION_GUIDE.md` for detailed documentation.
