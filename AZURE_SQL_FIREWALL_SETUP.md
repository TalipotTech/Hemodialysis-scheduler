# Azure SQL Firewall Management Scripts

## Overview
These scripts help manage Azure SQL Server firewall rules when you encounter the error:
```
Cannot open server 'hds-dev-sqlserver-cin' requested by the login. 
Client with IP address 'xxx.xxx.xxx.xxx' is not allowed to access the server.
```

## Scripts Included

### 1. `add-azure-sql-firewall-rule.ps1` (Full-Featured)
The complete version with extensive error handling, logging, and options.

**Features:**
- Automatically detects your public IP address
- Checks if rule already exists
- Supports custom rule names
- Lists all existing firewall rules
- Multiple fallback IP detection services
- Comprehensive error messages

**Usage:**
```powershell
# Basic usage with required parameters
.\add-azure-sql-firewall-rule.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg"

# With custom rule name
.\add-azure-sql-firewall-rule.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -RuleName "MyOffice"
```

### 2. `add-azure-sql-firewall-rule-simple.ps1` (Quick Version)
Simplified version with hardcoded values for quick execution.

**Features:**
- Hardcoded server name and resource group
- Just run and go
- Minimal prompts
- Perfect for frequent use

**Usage:**
```powershell
# Just run it!
.\add-azure-sql-firewall-rule-simple.ps1
```

### 3. `remove-old-azure-sql-firewall-rules.ps1` (Cleanup)
Removes old auto-generated firewall rules to keep your Azure SQL Server clean.

**Features:**
- Removes rules older than specified days (default 30)
- Only targets auto-generated rules (starting with "AutoRule-")
- WhatIf mode for safe preview
- Confirmation before deletion

**Usage:**
```powershell
# Preview what would be deleted
.\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -WhatIf

# Delete rules older than 30 days
.\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -DaysOld 30

# Delete rules older than 7 days
.\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -DaysOld 7
```

## Prerequisites

### 1. Install Azure PowerShell Module
```powershell
# Install for current user (recommended)
Install-Module -Name Az.Sql -Scope CurrentUser -Force

# Or install for all users (requires admin)
Install-Module -Name Az.Sql -Scope AllUsers -Force
```

### 2. Login to Azure
```powershell
Connect-AzAccount
```

### 3. Verify Your Subscription
```powershell
# List all subscriptions
Get-AzSubscription

# Set the correct subscription if needed
Set-AzContext -SubscriptionId "your-subscription-id"
```

## Quick Start Guide

### When You Get the Firewall Error:

**Option 1: Use the simple script**
```powershell
.\add-azure-sql-firewall-rule-simple.ps1
```

**Option 2: Use the full script**
```powershell
.\add-azure-sql-firewall-rule.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg"
```

### First-Time Setup (One-Time Only):

1. **Install Azure PowerShell** (if not already installed):
   ```powershell
   Install-Module -Name Az.Sql -Scope CurrentUser -Force
   ```

2. **Login to Azure**:
   ```powershell
   Connect-AzAccount
   ```

3. **Run the simple script**:
   ```powershell
   .\add-azure-sql-firewall-rule-simple.ps1
   ```

## Automated Solution

### Create a Scheduled Task (Windows)

To automatically update your IP when it changes, create a scheduled task:

```powershell
# Create a scheduled task that runs the script daily
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File `"E:\DEVELOPMENT\WEBSITE\ENSATE\CLIENTPROJECTS\Hemodialysis-scheduler\add-azure-sql-firewall-rule-simple.ps1`""

$trigger = New-ScheduledTaskTrigger -Daily -At 9am

$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive

Register-ScheduledTask -TaskName "Update Azure SQL Firewall" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Description "Automatically updates Azure SQL firewall with current IP address"
```

### Add to Your Startup Script

Add this to your PowerShell profile or startup script:

```powershell
# Add to: $PROFILE (run `notepad $PROFILE` to edit)
function Update-AzureSQLFirewall {
    & "E:\DEVELOPMENT\WEBSITE\ENSATE\CLIENTPROJECTS\Hemodialysis-scheduler\add-azure-sql-firewall-rule-simple.ps1"
}

# Create an alias for quick access
Set-Alias azfw Update-AzureSQLFirewall
```

Then you can just type `azfw` in PowerShell to update your firewall rule.

## Troubleshooting

### Error: "Az.Sql module not found"
**Solution:**
```powershell
Install-Module -Name Az.Sql -Scope CurrentUser -Force
```

### Error: "You are not logged in to Azure"
**Solution:**
```powershell
Connect-AzAccount
```

### Error: "Cannot detect public IP"
**Possible causes:**
- Firewall blocking outbound requests
- No internet connection
- Corporate proxy blocking IP detection services

**Solution:**
Manually specify your IP:
```powershell
$yourIP = "202.83.56.118"  # Replace with your IP
New-AzSqlServerFirewallRule `
    -ResourceGroupName "hds-dev-rg" `
    -ServerName "hds-dev-sqlserver-cin" `
    -FirewallRuleName "Manual-$(Get-Date -Format 'yyyyMMdd')" `
    -StartIpAddress $yourIP `
    -EndIpAddress $yourIP
```

### Error: "Insufficient permissions"
**Possible causes:**
- Your Azure account doesn't have permissions to modify SQL Server firewall rules
- Wrong subscription selected

**Solution:**
1. Verify you have "Contributor" or "SQL Server Contributor" role on the SQL Server
2. Check you're using the correct subscription:
   ```powershell
   Get-AzContext
   Set-AzContext -SubscriptionId "correct-subscription-id"
   ```

### Your IP keeps changing
**Solution:**
- Set up the scheduled task (see Automated Solution above)
- Or run the cleanup script weekly to remove old IP addresses
- Consider requesting a static IP from your ISP

## Best Practices

1. **Use descriptive rule names** when adding rules manually:
   ```powershell
   -RuleName "Home-Office-John"
   -RuleName "Company-VPN"
   ```

2. **Clean up old rules regularly**:
   ```powershell
   # Every month, remove rules older than 30 days
   .\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -DaysOld 30
   ```

3. **Use IP ranges for offices** instead of individual IPs:
   ```powershell
   New-AzSqlServerFirewallRule `
       -ResourceGroupName "hds-dev-rg" `
       -ServerName "hds-dev-sqlserver-cin" `
       -FirewallRuleName "Office-Network" `
       -StartIpAddress "202.83.56.100" `
       -EndIpAddress "202.83.56.200"
   ```

4. **Never open to all IPs** (0.0.0.0 to 255.255.255.255) in production

5. **Document important rules** so they don't get accidentally deleted

## Manual Alternative (Azure Portal)

If scripts don't work, you can add the rule manually:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your SQL Server: `hds-dev-sqlserver-cin`
3. Click "Networking" or "Firewalls and virtual networks"
4. Click "Add client IP" or "Add firewall rule"
5. Enter your IP address: `202.83.56.118`
6. Click "Save"

## Security Considerations

- ⚠️ Only add your current IP address
- ⚠️ Don't add overly broad IP ranges
- ⚠️ Remove old rules regularly
- ⚠️ Use Azure Virtual Network service endpoints for production
- ⚠️ Consider using Azure Private Link for enhanced security
- ✓ Use strong SQL authentication passwords
- ✓ Enable Azure AD authentication when possible
- ✓ Monitor firewall rule changes with Azure Activity Log

## Configuration for Different Environments

Update the scripts for different environments:

### Development
```powershell
$ServerName = "hds-dev-sqlserver-cin"
$ResourceGroupName = "hds-dev-rg"
```

### Staging
```powershell
$ServerName = "hds-staging-sqlserver"
$ResourceGroupName = "hds-staging-rg"
```

### Production
```powershell
$ServerName = "hds-prod-sqlserver"
$ResourceGroupName = "hds-prod-rg"
```

## Support

If you continue to experience issues:
1. Check Azure SQL Server is running and accessible
2. Verify your Azure subscription is active
3. Confirm you have the correct server name and resource group
4. Check Azure Activity Log for any errors
5. Contact Azure Support if firewall rules aren't being applied

---

**Last Updated:** December 10, 2025  
**Environment:** Azure SQL Database  
**Server:** hds-dev-sqlserver-cin  
**Resource Group:** hds-dev-rg
