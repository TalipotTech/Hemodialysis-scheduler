# 🚀 DEPLOY NOW - Quick Reference

## ✅ Build Status: READY FOR DEPLOYMENT

Both backend and frontend have been successfully built and packaged!

---

## 📦 Deployment Packages

### Backend API
- **File**: `Backend\deploy.zip` (5.24 MB)
- **Target**: Azure App Service `hds-dev-api`
- **Status**: ✅ Ready

### Frontend Application
- **Folder**: `Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser\`
- **Size**: 49.92 MB
- **Target**: Azure Static Web App or Storage Account
- **Status**: ✅ Ready

---

## 🎯 Fastest Deployment Method (5 Minutes)

### Step 1: Deploy Backend (2 minutes)

1. Open **Azure Portal**: https://portal.azure.com
2. Navigate to **Resource Group** → `EnsateBlogRG`
3. Click on **App Service** → `hds-dev-api`
4. In left menu: **Deployment** → **Deployment Center**
5. Click **ZIP Deploy** or **Browse**
6. Select file: `Backend\deploy.zip`
7. Click **Upload** and wait for completion
8. ✅ Done! Backend is live at: https://hds-dev-api.azurewebsites.net

**Verify**: Open https://hds-dev-api.azurewebsites.net/swagger

---

### Step 2: Deploy Frontend (3 minutes)

#### Option A: Azure Static Web App

1. In Azure Portal → **Static Web Apps** → Your frontend app
2. Click **Browse** or **GitHub Actions** → **Manage**
3. Upload folder: `Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser\`
4. ✅ Done!

#### Option B: Azure Storage Account (Static Website)

1. In Azure Portal → **Storage Account** → `hdsdevfrontend`
2. Left menu → **Data storage** → **Static website**
3. Enable if not already enabled
4. Click **$web** container
5. Click **Upload** → Upload all files from `Frontend\hd-scheduler-app\dist\hd-scheduler-app\browser\`
6. ✅ Done! Frontend is live at the primary endpoint URL

---

## 🔄 Alternative: Command Line Deployment

### Prerequisites
1. Close this PowerShell window
2. Open **NEW** PowerShell window as Administrator
3. Verify Azure CLI: `az --version`
4. Login: `az login`

### Deploy Both Applications
```powershell
cd G:\ENSATE\HdScheduler_Cloud\Hemodialysis-scheduler
.\deploy-apps-only.ps1
```

### Deploy Backend Only
```powershell
.\deploy-apps-only.ps1 -SkipFrontend
```

### Deploy Frontend Only
```powershell
.\deploy-apps-only.ps1 -SkipBackend
```

---

## 🔐 Default Login Credentials

After deployment, test with:
- **Username**: `admin`
- **Password**: `Admin@123`

Or:
- **Username**: `doctor1`
- **Password**: `Doctor@123`

---

## 🧪 Post-Deployment Testing

### 1. Test Backend API
```
URL: https://hds-dev-api.azurewebsites.net/swagger
Try: POST /api/Auth/login
Body: {"username": "admin", "password": "Admin@123"}
Expected: JWT token returned
```

### 2. Test Frontend
```
URL: [Your frontend URL]
Action: Login with admin credentials
Expected: Dashboard loads successfully
```

### 3. Test Full Flow
1. Login to frontend
2. Navigate to Patients → Should show patient list
3. Navigate to Schedule → Should show schedule grid
4. Try creating a new patient
5. Try scheduling a session

---

## ⚙️ Configuration Check

### Backend Configuration (Azure App Service)

Go to **App Service** → **Configuration** → **Application Settings**:

Required settings:
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = [Your Azure SQL connection string]
JwtSettings__SecretKey = YourSuperSecretKeyForJWTTokenGeneration123!@#ProductionKeyChangeThis!
JwtSettings__Issuer = HDSchedulerAPI
JwtSettings__Audience = HDSchedulerClient
JwtSettings__ExpiryMinutes = 60
```

### CORS Configuration

Go to **App Service** → **CORS**:
- Add your frontend URL (e.g., `https://yourapp.azurestaticapps.net`)
- Or temporarily add `*` for testing (change later!)

### Database Connection

Verify in **SQL Server** → **Firewalls and virtual networks**:
- ✅ "Allow Azure services and resources to access this server" = **ON**
- ✅ Your IP address is whitelisted (if connecting from local SSMS)

---

## 🐛 Troubleshooting

### Backend Not Starting
- Check App Service **Logs** in Azure Portal
- Go to **Log stream** to see real-time logs
- Verify connection string is correct
- Check if database exists and has tables

### Frontend Not Loading
- Check browser console (F12) for errors
- Verify API URL in `environment.ts` is correct
- Check CORS settings in backend
- Clear browser cache

### Database Connection Failed
- Verify firewall rules in Azure SQL
- Test connection with SSMS or Azure Data Studio
- Check connection string format
- Ensure database has tables (run migrations if needed)

### Login Not Working
- Verify user exists in database
- Check JWT settings match between frontend and backend
- Check browser network tab for API response
- Verify password is correct

---

## 📚 Additional Resources

- **Full Guide**: See `CLOUD_DEPLOYMENT_STEPS.md`
- **Deployment Guide**: See `DEPLOYMENT_GUIDE.md`
- **Architecture**: See `SYSTEM_ARCHITECTURE.md`
- **API Docs**: See `API_DOCUMENTATION.md`

---

## 🎉 Next Steps After Deployment

1. ✅ Test all features
2. ✅ Change default passwords
3. ✅ Configure custom domain (optional)
4. ✅ Set up Application Insights for monitoring
5. ✅ Configure automated backups
6. ✅ Set up CI/CD pipeline (optional)
7. ✅ Review and update security settings

---

## 💡 Pro Tips

1. **Use Deployment Slots** for zero-downtime deployments
2. **Enable Application Insights** for monitoring and diagnostics
3. **Set up automated backups** for SQL database
4. **Use Key Vault** for storing secrets (connection strings, API keys)
5. **Configure auto-scaling** based on load
6. **Set up alerts** for critical errors

---

## 🆘 Need Help?

1. Check **Azure Portal** → **Resource Health** for your resources
2. Review **Application Insights** → **Failures** for errors
3. Check **App Service** → **Log stream** for real-time logs
4. See documentation files in the project root

---

**Status**: ✅ Ready to deploy!  
**Time Required**: 5-10 minutes  
**Difficulty**: Easy (Azure Portal) | Medium (Command Line)

👉 **Recommended**: Start with Azure Portal method (Step 1 & 2 above)
