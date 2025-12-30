# Hemodialysis Scheduler - Cloud Deployment Steps

## Status: Backend Built Successfully ✓

Your backend has been compiled and packaged. The deployment package is ready at:
**Location**: `Backend\deploy.zip` (5.24 MB)

---

## Deployment Options

Since Azure CLI needs a PowerShell restart to work properly, here are your deployment options:

### Option 1: Deploy via Azure Portal (Easiest - No CLI Required)

#### Backend Deployment:

1. **Open Azure Portal**: https://portal.azure.com
2. **Navigate to your App Service**:
   - Resource Group: `EnsateBlogRG`
   - App Service: `hds-dev-api` (or your environment name)
3. **Deploy using Deployment Center**:
   - In the left menu, go to **Deployment** → **Deployment Center**
   - Choose **ZIP Deploy** or **Local Git**
   - Click **Browse** and select `Backend\deploy.zip`
   - Click **Upload**
   - Wait for deployment to complete (2-3 minutes)

#### Alternative - Using Kudu Console:

1. Go to your App Service in Azure Portal
2. Navigate to **Development Tools** → **Advanced Tools** → **Go**
3. This opens Kudu dashboard
4. Click **Tools** → **Zip Push Deploy**
5. Drag and drop `Backend\deploy.zip`
6. Wait for automatic extraction and deployment

#### Verify Backend:
- URL: https://hds-dev-api.azurewebsites.net
- Swagger: https://hds-dev-api.azurewebsites.net/swagger

---

### Option 2: Deploy using Visual Studio (If Available)

1. Open `Backend\HDScheduler.API.csproj` in Visual Studio 2022
2. Right-click on the project → **Publish**
3. Select **Azure** → **Azure App Service (Windows)**
4. Sign in with your Azure account
5. Select your subscription and App Service `hds-dev-api`
6. Click **Publish**

---

### Option 3: Deploy After Restarting PowerShell (CLI Method)

After closing and reopening PowerShell (to load the new Azure CLI):

```powershell
# Login to Azure
az login

# Set subscription
az account set --subscription "74dc21b3-629c-40c3-aa0b-935da454b3e4"

# Deploy backend
az webapp deploy `
  --resource-group EnsateBlogRG `
  --name hds-dev-api `
  --src-path .\Backend\deploy.zip `
  --type zip

# OR use the deployment script
.\deploy-apps-only.ps1
```

---

## Frontend Deployment

### Step 1: Build Frontend

Run this in PowerShell:

```powershell
cd Frontend\hd-scheduler-app

# Install dependencies (if not already installed)
npm install

# Build for production
npm run build -- --configuration production
```

### Step 2: Deploy Frontend

#### Option A: Azure Static Web Apps (via Portal)

1. Go to Azure Portal
2. Create or navigate to Static Web App
3. Go to **Deployment** → **Upload**
4. Upload the `dist\hd-scheduler-app\browser` folder

#### Option B: Azure Storage Account (Static Website)

1. Go to your storage account in Azure Portal
2. Enable **Static website** in Settings
3. Upload contents of `dist\hd-scheduler-app\browser` to `$web` container
4. Note the primary endpoint URL

#### Option C: Using Azure CLI (after restart)

```powershell
# Deploy to storage account
az storage blob upload-batch `
  --account-name hdsdevfrontend `
  --destination '$web' `
  --source ./dist/hd-scheduler-app/browser `
  --overwrite
```

---

## Configuration Checklist

### Backend Configuration

Ensure `appsettings.Production.json` has:
- ✓ Correct Azure SQL connection string
- ✓ JWT secret key
- ✓ CORS settings for frontend URL

### Frontend Configuration

Update `src/environments/environment.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://hds-dev-api.azurewebsites.net'
};
```

---

## Post-Deployment Steps

### 1. Database Setup (If Not Done)

Connect to Azure SQL and run:
```powershell
# Using SQL Server Management Studio or Azure Data Studio
# Server: hds-dev-sqlserver-cin.database.windows.net
# Database: hds-dev-db
# Auth: SQL Authentication
# User: hdsadmin
# Password: [Your password]

# Run these scripts in order:
# 1. Database\01_CreateSchema.sql
# 2. Database\02_SeedData.sql
```

### 2. Configure App Service Settings

In Azure Portal → App Service → Configuration:

**Connection Strings**:
- Name: `DefaultConnection`
- Type: `SQLAzure`
- Value: Your Azure SQL connection string

**Application Settings**:
- `ASPNETCORE_ENVIRONMENT` = `Production`
- `JwtSettings__SecretKey` = Your secret key
- `JwtSettings__Issuer` = `HDSchedulerAPI`
- `JwtSettings__Audience` = `HDSchedulerClient`

### 3. Enable CORS

In Azure Portal → App Service → CORS:
- Add your frontend URL
- Or add `*` for testing (not recommended for production)

### 4. Test the Deployment

1. **Test Backend**:
   - Open: https://hds-dev-api.azurewebsites.net/swagger
   - Try the `/api/Auth/login` endpoint
   - Use default credentials:
     ```json
     {
       "username": "admin",
       "password": "Admin@123"
     }
     ```

2. **Test Frontend**:
   - Open your frontend URL
   - Try logging in
   - Navigate through the application

---

## Quick Deployment Summary

### What's Ready:
✓ Backend built and packaged → `Backend\deploy.zip`
✓ Azure infrastructure exists (SQL Server, App Service)
✓ Configuration files are set

### What You Need To Do:

**Immediate (No PowerShell restart needed):**
1. Go to Azure Portal
2. Upload `Backend\deploy.zip` to App Service `hds-dev-api`
3. Build frontend: `cd Frontend\hd-scheduler-app; npm run build`
4. Upload frontend to Static Web App or Storage Account

**OR (After PowerShell restart):**
1. Close this PowerShell window
2. Open new PowerShell window
3. Run: `az login`
4. Run: `.\deploy-apps-only.ps1`

---

## Troubleshooting

### Backend Issues:
- Check App Service logs in Azure Portal
- Verify connection string is correct
- Check Application Insights for errors

### Frontend Issues:
- Verify API URL in `environment.ts`
- Check browser console for CORS errors
- Ensure CORS is configured in backend

### Database Connection Issues:
- Verify firewall rules allow Azure services
- Test connection string using SSMS
- Check if database exists and has tables

---

## Support Resources

- Azure Portal: https://portal.azure.com
- Azure Status: https://status.azure.com
- Documentation: See `DEPLOYMENT_GUIDE.md`

---

**Next Step**: Choose Option 1 (Azure Portal) for the easiest deployment without needing to restart PowerShell.
