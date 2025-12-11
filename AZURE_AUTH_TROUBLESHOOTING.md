# Quick reference for Azure SQL Firewall scripts

## If Connect-AzAccount is stuck or not working:

### Option 1: Use Device Code Authentication (Recommended)
This method doesn't require browser pop-ups:

```powershell
# Login using device code (no browser pop-up needed)
Connect-AzAccount -UseDeviceAuthentication

# You'll see a code like "ABC123DEF"
# 1. Go to: https://microsoft.com/devicelogin
# 2. Enter the code shown
# 3. Sign in with your Microsoft account
# 4. Come back to PowerShell - it will continue automatically
```

### Option 2: Use Interactive Script
```powershell
.\add-azure-sql-firewall-rule-interactive.ps1
```
This script provides step-by-step guidance and waits for your input.

### Option 3: Manual Steps (If scripts don't work)

**Get your IP address:**
```powershell
# Get your public IP
$ip = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
Write-Host "Your IP: $ip"
```

**Add to Azure Portal manually:**
1. Go to: https://portal.azure.com
2. Search for "hds-dev-sqlserver-cin"
3. Click on "Networking" (left menu)
4. Click "Add client IP" or manually add your IP
5. Click "Save"

### Option 4: Use Azure CLI (Alternative to PowerShell)
If PowerShell authentication is problematic, try Azure CLI:

```bash
# Install Azure CLI first (if not installed)
# Download from: https://aka.ms/installazurecliwindows

# Login via browser
az login

# Add firewall rule
az sql server firewall-rule create ^
  --resource-group hds-dev-rg ^
  --server hds-dev-sqlserver-cin ^
  --name "MyIP-%date:~-4,4%%date:~-10,2%%date:~-7,2%" ^
  --start-ip-address YOUR_IP_HERE ^
  --end-ip-address YOUR_IP_HERE
```

## Common Issues & Solutions

### Issue: "Please select the account you want to login with" - then stuck

**Causes:**
- Browser pop-up blocked
- Multiple Azure accounts
- Cached credentials expired
- Browser security settings

**Solutions:**

1. **Clear Azure cache:**
```powershell
# Clear all cached Azure credentials
Clear-AzContext -Force

# Then try logging in again
Connect-AzAccount
```

2. **Specify tenant explicitly:**
```powershell
# If you know your tenant ID
Connect-AzAccount -TenantId "your-tenant-id"

# Or use tenant domain
Connect-AzAccount -Tenant "yourcompany.onmicrosoft.com"
```

3. **Use device code authentication (no browser needed):**
```powershell
Connect-AzAccount -UseDeviceAuthentication
```

4. **Check for browser issues:**
```powershell
# Try with a specific browser
$env:BROWSER = "chrome"  # or "firefox", "edge"
Connect-AzAccount
```

5. **Close all PowerShell windows and start fresh:**
```powershell
# Start NEW PowerShell window
# Run only:
Connect-AzAccount -UseDeviceAuthentication
```

### Issue: "Az.Sql module not found"

**Solution:**
```powershell
# Install with verbose to see progress
Install-Module -Name Az.Sql -Scope CurrentUser -Force -Verbose
```

### Issue: "Access denied" or "Insufficient permissions"

**Solutions:**
1. Make sure you're logging in with the correct account
2. Check you have "Contributor" role on the SQL Server
3. Ask your Azure administrator to grant you permissions

### Issue: Script runs but still can't connect to database

**Check:**
1. Wait 1-2 minutes for firewall rule to propagate
2. Verify rule was added:
```powershell
Get-AzSqlServerFirewallRule -ResourceGroupName "hds-dev-rg" -ServerName "hds-dev-sqlserver-cin" | Format-Table
```
3. Restart your application
4. Check if VPN is interfering

## Quick Commands Reference

```powershell
# === Authentication ===
Connect-AzAccount                           # Standard login
Connect-AzAccount -UseDeviceAuthentication  # Device code (recommended if stuck)
Clear-AzContext -Force                      # Clear cached credentials
Get-AzContext                               # Check current login

# === Check IP ===
(Invoke-RestMethod -Uri "https://api.ipify.org").Trim()

# === List firewall rules ===
Get-AzSqlServerFirewallRule -ResourceGroupName "hds-dev-rg" -ServerName "hds-dev-sqlserver-cin"

# === Add rule manually ===
$ip = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
New-AzSqlServerFirewallRule `
  -ResourceGroupName "hds-dev-rg" `
  -ServerName "hds-dev-sqlserver-cin" `
  -FirewallRuleName "Manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" `
  -StartIpAddress $ip `
  -EndIpAddress $ip

# === Remove a specific rule ===
Remove-AzSqlServerFirewallRule `
  -ResourceGroupName "hds-dev-rg" `
  -ServerName "hds-dev-sqlserver-cin" `
  -FirewallRuleName "RuleNameHere"
```

## Recommended Workflow

**First Time Setup:**
```powershell
# 1. Install module
Install-Module -Name Az.Sql -Scope CurrentUser -Force

# 2. Login with device code (avoids browser issues)
Connect-AzAccount -UseDeviceAuthentication

# 3. Verify login
Get-AzContext

# 4. Get and add your IP
$ip = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
Write-Host "Your IP: $ip"

New-AzSqlServerFirewallRule `
  -ResourceGroupName "hds-dev-rg" `
  -ServerName "hds-dev-sqlserver-cin" `
  -FirewallRuleName "MyComputer-$(Get-Date -Format 'yyyyMMdd')" `
  -StartIpAddress $ip `
  -EndIpAddress $ip
```

**Every Time Your IP Changes:**
```powershell
# Quick way (if already logged in)
.\add-azure-sql-firewall-rule-simple.ps1

# Or with device auth if needed
.\add-azure-sql-firewall-rule-interactive.ps1
```

## Alternative: Use Connection String with Azure AD Auth

If firewall management is too complex, consider using Azure AD authentication:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:hds-dev-sqlserver-cin.database.windows.net,1433;Database=YourDB;Authentication=Active Directory Interactive;"
}
```

This uses your Azure login instead of SQL authentication and can sometimes bypass IP restrictions.

---

**Need more help?**
- Azure Portal: https://portal.azure.com
- Azure PowerShell Docs: https://docs.microsoft.com/powershell/azure/
- Open a support ticket in Azure Portal
