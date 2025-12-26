# Azure Static Web Apps Deployment Guide

## Overview

Your frontend is now properly configured to deploy to **Azure Static Web Apps** instead of the Storage Account. This resolves CORS issues and provides better integration with your custom domain.

## Current Setup

- **Static Web App Name**: `hds-dev-frontend`
- **Default URL**: https://lively-pond-08e4f7c00.3.azurestaticapps.net
- **Custom Domain**: https://dev.dialyzeflow.com ✅
- **Backend CORS**: Already configured for both URLs ✅

## Deployment Options

### Option 1: Use deploy-apps-only.ps1 (Recommended)

This is the main deployment script that now properly uses Static Web Apps:

```powershell
# Deploy both backend and frontend
.\deploy-apps-only.ps1

# Deploy frontend only
.\deploy-apps-only.ps1 -SkipBackend

# Deploy to production environment
.\deploy-apps-only.ps1 -Environment prod
```

### Option 2: Use deploy-staticwebapp.ps1 (Frontend Only)

For frontend-only deployments with more detailed output:

```powershell
# Deploy to dev environment
.\deploy-staticwebapp.ps1

# Deploy to production
.\deploy-staticwebapp.ps1 -Environment prod
```

## Files Created/Updated

1. **[staticwebapp.config.json](Frontend/hd-scheduler-app/staticwebapp.config.json)** - Static Web App configuration
   - Handles SPA routing (redirects to index.html)
   - Configures MIME types
   - Sets cache headers

2. **[deploy-staticwebapp.ps1](deploy-staticwebapp.ps1)** - Dedicated Static Web App deployment script
   - Checks prerequisites
   - Gets deployment token
   - Builds and deploys the app

3. **[deploy-apps-only.ps1](deploy-apps-only.ps1)** - Updated to use Static Web Apps instead of Storage Account

4. **[cleanup-storage-account.ps1](cleanup-storage-account.ps1)** - Removes the old storage account

## Environment Configuration

The deployment scripts **automatically use the production environment** - no manual switching required!

### How It Works

1. **Angular File Replacement** ([angular.json](Frontend/hd-scheduler-app/angular.json#L49-L53)):
   ```json
   "production": {
     "fileReplacements": [
       {
         "replace": "src/environments/environment.development.ts",
         "with": "src/environments/environment.ts"
       }
     ]
   }
   ```

2. **When you build with** `--configuration production`:
   - Angular automatically replaces `environment.development.ts` with `environment.ts`
   - Your app uses the production API URL: `https://hds-dev-api.azurewebsites.net`

3. **Deployment scripts** use production configuration:
   ```powershell
   npm run build -- --configuration production
   ```

### Environment Files

| File | Usage | API URL |
|------|-------|---------|
| `environment.development.ts` | Local development (`npm start`) | `http://localhost:5000` |
| `environment.ts` | Azure deployment (automatic) | `https://hds-dev-api.azurewebsites.net` |

### Verification

Run this command to verify your configuration:
```powershell
.\verify-environment.ps1
```

This will show:
- ✅ Both environment files exist
- ✅ Angular file replacement is configured
- ✅ Correct API URLs for each environment

## Prerequisites

The deployment script will automatically check and install:
- Azure CLI ✅
- Node.js ✅
- Static Web Apps CLI (will auto-install if missing)

## Next Steps

### 1. Test the New Deployment

Deploy your frontend using the updated script:

```powershell
.\deploy-staticwebapp.ps1
```

This will:
- Install SWA CLI if needed
- Build your Angular app
- Deploy to the existing Static Web App
- Show both default and custom domain URLs

### 2. Verify Everything Works

After deployment, test both URLs:
- Default: https://lively-pond-08e4f7c00.3.azurestaticapps.net
- Custom: https://dev.dialyzeflow.com

Both should work **WITHOUT CORS errors** because they're already configured in your backend.

### 3. Clean Up Old Storage Account

Once you've verified everything works, remove the old storage account:

```powershell
.\cleanup-storage-account.ps1
```

This will:
- Check if the storage account exists
- Ask for confirmation
- Delete `hdsdevfrontend` storage account
- Free up resources

## Understanding the Setup

### Why Two URLs?

- **Default URL** (`lively-pond-08e4f7c00.3.azurestaticapps.net`): 
  - Azure-generated URL
  - Always available
  - Cannot be changed

- **Custom Domain** (`dev.dialyzeflow.com`):
  - Your branded URL
  - Configured in Azure Static Web Apps
  - This is your production URL

### CORS Configuration

Your backend [CORS policy](Backend/Program.cs#L89-L92) already includes:
```csharp
policy.WithOrigins(
    "http://localhost:4200",
    "https://lively-pond-08e4f7c00.3.azurestaticapps.net",
    "https://dev.dialyzeflow.com")
```

Both Static Web App URLs are allowed, so no CORS issues! ✅

### What Happened Before?

The old deployment created a **Storage Account** (`hdsdevfrontend.z29.web.core.windows.net`) that:
- ❌ Was NOT in the CORS whitelist
- ❌ Created confusion with multiple URLs
- ❌ Required manual domain configuration

Now you have **one proper hosting solution** with your custom domain already configured.

## Troubleshooting

### SWA CLI Installation Issues

If the automatic installation fails:
```powershell
npm install -g @azure/static-web-apps-cli
```

### Deployment Token Issues

If deployment fails with token errors:
```powershell
az staticwebapp secrets list --name hds-dev-frontend --resource-group EnsateBlogRG
```

### Build Issues

Ensure you're in the correct directory:
```powershell
cd Frontend/hd-scheduler-app
npm install
npm run build -- --configuration production
```

## Production Deployment

For production environment:

1. Create production Static Web App (if not exists):
```powershell
az staticwebapp create \
  --name hds-prod-frontend \
  --resource-group EnsateBlogRG \
  --location "East US"
```

2. Deploy to production:
```powershell
.\deploy-staticwebapp.ps1 -Environment prod
```

3. Update backend CORS to include production URLs

## Benefits of Static Web Apps

✅ **No CORS Issues**: Proper origin handling  
✅ **Custom Domain**: Already configured (dev.dialyzeflow.com)  
✅ **Auto HTTPS**: Free SSL certificates  
✅ **Global CDN**: Fast content delivery  
✅ **SPA Support**: Built-in routing for Angular  
✅ **CI/CD Ready**: Can integrate with GitHub Actions  

## Summary

- ✅ Old problem: Storage Account not in CORS whitelist
- ✅ Solution: Use existing Static Web App (already has custom domain)
- ✅ Scripts updated: `deploy-apps-only.ps1` now uses Static Web Apps
- ✅ New script: `deploy-staticwebapp.ps1` for dedicated deployments
- ✅ Cleanup: `cleanup-storage-account.ps1` to remove old resources
- ✅ No configuration needed: Custom domain already set up!
