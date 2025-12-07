# ✅ Azure SQL Server Migration - Action Checklist

## 📋 Pre-Deployment Checklist

### Azure Account Setup
- [ ] Verify Azure CLI installed: `az --version`
- [ ] Login to Azure: `az login --tenant ensate365.onmicrosoft.com`
- [ ] Set subscription: `az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"`
- [ ] Verify resource group exists: `az group show --name EnsateBlogRG`

### Local Development Tools
- [ ] SQL Server Command Line Tools installed: `winget install Microsoft.SQLServerCommandLineTools`
- [ ] .NET 8 SDK installed: `dotnet --version` (should show 8.x.x)
- [ ] Git configured and repository cloned
- [ ] Node.js installed for frontend (if testing full stack)

## 🚀 Deployment Steps

### Step 1: Create Azure Resources (5 minutes)
- [ ] Navigate to project root: `cd E:\DEVELOPMENT\WEBSITE\ENSATE\CLIENTPROJECTS\Hemodialysis-scheduler`
- [ ] Run deployment script: `.\deploy-azure.ps1 -Environment dev`
- [ ] Enter secure SQL password when prompted (save this password!)
- [ ] Wait for completion (creates SQL Server, Database, App Service Plan, Web App)
- [ ] Verify resources created in Azure Portal

### Step 2: Configure Firewall (2 minutes)
- [ ] Get your public IP: `$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content`
- [ ] Add firewall rule:
  ```powershell
  az sql server firewall-rule create `
      --resource-group EnsateBlogRG `
      --server hds-dev-sqlserver-eus `
      --name "MyDevMachine" `
      --start-ip-address $myIp `
      --end-ip-address $myIp
  ```
- [ ] Verify firewall rule in Azure Portal: SQL Server → Networking → Firewall rules

### Step 3: Initialize Database (3 minutes)
- [ ] Test SQL connection: `sqlcmd -S hds-dev-sqlserver-eus.database.windows.net -U hdsadmin -P "YourPassword" -Q "SELECT @@VERSION"`
- [ ] Run schema script:
  ```powershell
  sqlcmd -S hds-dev-sqlserver-eus.database.windows.net `
         -U hdsadmin `
         -P "YourPassword" `
         -d hds-dev-db `
         -i "Database\SqlServer\01_CreateSchema.sql"
  ```
- [ ] Verify tables created: `sqlcmd -S hds-dev-sqlserver-eus.database.windows.net -U hdsadmin -P "YourPassword" -d hds-dev-db -Q "SELECT name FROM sys.tables"`
- [ ] Run seed data script:
  ```powershell
  sqlcmd -S hds-dev-sqlserver-eus.database.windows.net `
         -U hdsadmin `
         -P "YourPassword" `
         -d hds-dev-db `
         -i "Database\SqlServer\02_SeedData.sql"
  ```
- [ ] Verify data: `sqlcmd -S hds-dev-sqlserver-eus.database.windows.net -U hdsadmin -P "YourPassword" -d hds-dev-db -Q "SELECT Username, Role FROM Users"`

### Step 4: Configure Local Development (2 minutes)
- [ ] Copy template config: `copy Backend\appsettings.template.json Backend\appsettings.Development.json`
- [ ] Edit `Backend\appsettings.Development.json`:
  - [ ] Replace `{YOUR_SQL_SERVER}` with `hds-dev-sqlserver`
  - [ ] Replace `{YOUR_DATABASE}` with `hds-dev-db`
  - [ ] Replace `{YOUR_USERNAME}` with `hdsadmin`
  - [ ] Replace `{YOUR_PASSWORD}` with your SQL password
  - [ ] Generate strong secret key for JWT (64+ characters)
- [ ] Save and close file

### Step 5: Test Backend Locally (3 minutes)
- [ ] Navigate to backend: `cd Backend`
- [ ] Restore packages: `dotnet restore`
- [ ] Build project: `dotnet build`
- [ ] Set environment: `$env:ASPNETCORE_ENVIRONMENT="Development"`
- [ ] Run application: `dotnet run`
- [ ] Verify startup: Look for "Now listening on: http://localhost:5001"
- [ ] Open Swagger: http://localhost:5001/swagger
- [ ] Test login endpoint: POST `/api/auth/login` with:
  ```json
  {
    "username": "admin",
    "password": "Admin@123"
  }
  ```
- [ ] Verify JWT token received

### Step 6: Test API Endpoints (5 minutes)
- [ ] Test GET `/api/patients` - should return empty list or patients
- [ ] Test GET `/api/slots` - should return 4 time slots
- [ ] Test GET `/api/staff` - should return sample staff
- [ ] Test GET `/api/users` - should return users list
- [ ] Verify no database connection errors

### Step 7: Deploy to Azure (Optional - 5 minutes)
- [ ] Build for release: `dotnet publish -c Release -o ./publish`
- [ ] Create deployment package: `Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force`
- [ ] Deploy to Azure:
  ```powershell
  az webapp deploy `
      --resource-group EnsateBlogRG `
      --name hds-dev-api `
      --src-path ./publish.zip `
      --type zip
  ```
- [ ] Wait for deployment to complete
- [ ] Open Azure URL: https://hds-dev-api.azurewebsites.net/swagger
- [ ] Test login endpoint on Azure
- [ ] Verify all endpoints work

### Step 8: Test Frontend (Optional - 10 minutes)
- [ ] Navigate to frontend: `cd ..\Frontend\hd-scheduler-app`
- [ ] Install dependencies: `npm install --legacy-peer-deps`
- [ ] Update API URL in environment file if needed
- [ ] Start dev server: `npm start`
- [ ] Open browser: http://localhost:4200
- [ ] Test login with admin/Admin@123
- [ ] Verify dashboard loads
- [ ] Test basic navigation

## 🔐 Security Checklist

### Immediate Security Actions
- [ ] Change all default user passwords:
  - [ ] Admin password
  - [ ] HOD password
  - [ ] Doctor password
  - [ ] Nurse password
  - [ ] Technician password
- [ ] Store SQL admin password in secure location (e.g., Azure Key Vault)
- [ ] Generate strong JWT secret key (not using default)
- [ ] Verify `appsettings.*.json` files are in `.gitignore`
- [ ] Do NOT commit passwords to Git

### Production Security (Before Go-Live)
- [ ] Enable Azure AD authentication
- [ ] Set up Azure Key Vault for secrets
- [ ] Configure SSL certificates for custom domain
- [ ] Enable Application Insights
- [ ] Set up automated backups
- [ ] Configure geo-replication for database
- [ ] Enable audit logging
- [ ] Review and restrict firewall rules
- [ ] Enable Azure SQL Advanced Threat Protection

## 📊 Monitoring Setup (Optional)

### Application Insights
- [ ] Create Application Insights resource
- [ ] Add instrumentation key to app settings
- [ ] Test logging and telemetry
- [ ] Create dashboard for monitoring

### Azure SQL Monitoring
- [ ] Enable Query Performance Insights
- [ ] Set up alerts for high DTU usage
- [ ] Configure backup retention policy
- [ ] Test point-in-time restore

## 👥 Team Collaboration Setup

### For Each Developer
- [ ] Clone repository
- [ ] Run `.\deploy-azure.ps1 -Environment dev` (only first time, team lead)
- [ ] Get shared SQL password from team lead
- [ ] Copy `appsettings.template.json` to `appsettings.Development.json`
- [ ] Add their IP to firewall (or use team lead's IP range)
- [ ] Test local connection to shared dev database
- [ ] Verify they can run and debug locally

### Git Configuration
- [ ] Verify `.gitignore` includes `appsettings.*.json`
- [ ] Create Git branch for development work
- [ ] Set up branch protection rules
- [ ] Configure PR review requirements

## 🎯 Success Criteria

Your migration is complete and successful when:

### Backend
- [x] Code builds without SQLite references
- [x] Uses SqlConnection instead of SQLiteConnection
- [x] Environment-specific configs created
- [ ] Connects to Azure SQL successfully
- [ ] All API endpoints return data
- [ ] No database errors in logs

### Database
- [ ] Azure SQL Server created
- [ ] Database initialized with schema
- [ ] Seed data loaded successfully
- [ ] Can query tables via sqlcmd
- [ ] Firewall configured correctly

### Deployment
- [ ] Web App provisioned
- [ ] Backend deployed to Azure
- [ ] Swagger UI accessible publicly
- [ ] Login works on Azure deployment
- [ ] All endpoints functional

### Documentation
- [x] Migration guide created
- [x] Quick start guide created
- [x] Deployment script ready
- [x] SQL Server migration scripts ready
- [x] Security notes documented

## 🐛 Troubleshooting

### Cannot Connect to SQL Server
- [ ] Check firewall rules: `az sql server firewall-rule list --resource-group EnsateBlogRG --server hds-dev-sqlserver-eus`
- [ ] Verify IP address hasn't changed: `(Invoke-WebRequest -Uri "https://api.ipify.org").Content`
- [ ] Test connection: `sqlcmd -S hds-dev-sqlserver-eus.database.windows.net -U hdsadmin -P "YourPassword" -Q "SELECT 1"`
- [ ] Check Azure Portal > SQL Server > Networking

### Backend Won't Start
- [ ] Check connection string format
- [ ] Verify password doesn't have special characters that need escaping
- [ ] Check environment variable: `$env:ASPNETCORE_ENVIRONMENT`
- [ ] Review console output for specific errors
- [ ] Check `dotnet build` completes successfully

### Database Script Errors
- [ ] Verify using correct database: `-d hds-dev-db`
- [ ] Check script paths are correct
- [ ] Try running scripts line by line
- [ ] Check for existing tables: `SELECT name FROM sys.tables`

### Web App Deployment Fails
- [ ] Check Web App exists: `az webapp show --name hds-dev-api --resource-group EnsateBlogRG`
- [ ] Verify publish folder has files: `ls ./publish`
- [ ] Check zip file created: `ls ./publish.zip`
- [ ] Review deployment logs: `az webapp log tail --name hds-dev-api --resource-group EnsateBlogRG`

## 📞 Support Resources

- **Azure Documentation**: https://learn.microsoft.com/azure/
- **SQL Server Docs**: https://learn.microsoft.com/sql/
- **Migration Guide**: `AZURE_MIGRATION_GUIDE.md`
- **Quick Start**: `QUICKSTART_AZURE.md`
- **Summary**: `MIGRATION_SUMMARY.md`

## ✨ Next Steps After Completion

1. Set up staging environment: `.\deploy-azure.ps1 -Environment staging`
2. Configure CI/CD pipeline (GitHub Actions)
3. Set up custom domain and SSL
4. Implement Azure Key Vault integration
5. Create tenant provisioning script for new clients
6. Set up monitoring and alerts
7. Document API for frontend team
8. Plan production rollout strategy

---

**Last Updated**: December 6, 2025
**Estimated Total Time**: 25-35 minutes for first-time setup
