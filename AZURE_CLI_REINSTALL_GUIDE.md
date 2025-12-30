# Azure CLI Reinstallation Guide

## Problem
Azure CLI is showing "Failed to load python executable" error, which prevents deployment commands from working.

## Solution: Reinstall Azure CLI

### Method 1: Using Windows Installer (Recommended)

1. **Uninstall current Azure CLI:**
   - Open Windows Settings (Win + I)
   - Go to "Apps" > "Apps & features"
   - Search for "Microsoft Azure CLI"
   - Click "Uninstall" and follow prompts

2. **Download latest version:**
   - Visit: https://aka.ms/installazurecliwindows
   - Or direct link: https://azcliprod.blob.core.windows.net/msi/azure-cli-latest.msi

3. **Install Azure CLI:**
   - Run the downloaded .msi installer
   - Follow the installation wizard
   - Accept the license agreement
   - Choose default installation location
   - Complete the installation

4. **Restart PowerShell/Terminal:**
   - Close all PowerShell windows
   - Open a new PowerShell window as Administrator

5. **Verify installation:**
   ```powershell
   az --version
   ```
   You should see version information without errors.

6. **Login to Azure:**
   ```powershell
   az login
   ```

### Method 2: Using PowerShell (Quick)

Run this command in PowerShell as Administrator:

```powershell
# Uninstall old version
$app = Get-WmiObject -Class Win32_Product | Where-Object { $_.Name -like "*Azure CLI*" }
if ($app) { $app.Uninstall() }

# Download and install latest version
$progressPreference = 'silentlyContinue'
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi
Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'
Remove-Item .\AzureCLI.msi
```

Then restart PowerShell and verify:
```powershell
az --version
az login
```

### Method 3: Using Winget (Windows Package Manager)

If you have winget installed:

```powershell
# Uninstall old version
winget uninstall "Microsoft Azure CLI"

# Install latest version
winget install Microsoft.AzureCLI
```

Restart PowerShell and verify:
```powershell
az --version
az login
```

---

## After Installation

Once Azure CLI is reinstalled and working, you can retry the deployment:

```powershell
# Deploy both backend and frontend
.\deploy-apps-only.ps1

# Or deploy backend only
.\deploy-apps-only.ps1 -SkipFrontend

# Or deploy frontend only
.\deploy-apps-only.ps1 -SkipBackend
```

---

## Alternative: Use Azure Portal for Manual Deployment

If you prefer not to reinstall Azure CLI right now, you can use the manual deployment:

### For Backend:
1. Run: `.\deploy-backend-kudu.ps1` (already completed - package is ready)
2. Go to https://portal.azure.com
3. Navigate to your App Service: **hds-dev-api**
4. Go to **Deployment Center** > **Manual Deployment (Push/FTP)**
5. Upload `Backend\deploy.zip`

### For Frontend:
1. Build locally: `cd Frontend\hd-scheduler-app; npm run build`
2. Go to Azure Portal > Storage Account: **hdsdevfrontend**
3. Go to **Storage browser** > **Blob containers** > **$web**
4. Upload contents from `dist/hd-scheduler-app/browser`

---

## Troubleshooting

**If az --version still fails after reinstall:**
1. Check if Python is installed: `python --version`
2. If Python is missing, install from: https://www.python.org/downloads/
3. Ensure Python is added to PATH during installation
4. Restart PowerShell after Python installation

**If you get permission errors:**
- Run PowerShell as Administrator
- Or use: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`

**Check Azure CLI location:**
```powershell
where.exe az
# Should show: C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd
```
