#Requires -Modules Az.Sql

<#
.SYNOPSIS
    Adds current public IP address to Azure SQL Server firewall rules.

.DESCRIPTION
    This script automatically detects your current public IP address and adds it as a firewall rule
    to the specified Azure SQL Server. Useful when you get "Client with IP address is not allowed" errors.

.PARAMETER ServerName
    The name of the Azure SQL Server (without .database.windows.net)

.PARAMETER ResourceGroupName
    The name of the Azure Resource Group containing the SQL Server

.PARAMETER RuleName
    Optional. The name for the firewall rule. Defaults to hostname with timestamp.

.EXAMPLE
    .\add-azure-sql-firewall-rule.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg"

.EXAMPLE
    .\add-azure-sql-firewall-rule.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -RuleName "MyOffice"

.NOTES
    Requires Azure PowerShell module (Az.Sql) to be installed.
    Run: Install-Module -Name Az.Sql -Scope CurrentUser -Force
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName = "hds-dev-sqlserver-cin",
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName = "hds-dev-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$RuleName = ""
)

# Function to get public IP address
function Get-PublicIPAddress {
    Write-Host "Detecting your public IP address..." -ForegroundColor Cyan
    
    $services = @(
        "https://api.ipify.org",
        "https://ifconfig.me/ip",
        "https://icanhazip.com",
        "https://checkip.amazonaws.com"
    )
    
    foreach ($service in $services) {
        try {
            $ip = (Invoke-RestMethod -Uri $service -TimeoutSec 5).Trim()
            if ($ip -match '^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$') {
                Write-Host "✓ Your public IP address: $ip" -ForegroundColor Green
                return $ip
            }
        }
        catch {
            continue
        }
    }
    
    throw "Failed to detect public IP address. Please check your internet connection."
}

# Function to check if Az module is installed
function Test-AzModuleInstalled {
    $module = Get-Module -ListAvailable -Name Az.Sql
    if (-not $module) {
        Write-Host "❌ Az.Sql module is not installed." -ForegroundColor Red
        Write-Host "Installing Az.Sql module..." -ForegroundColor Yellow
        try {
            Install-Module -Name Az.Sql -Scope CurrentUser -Force -AllowClobber
            Write-Host "✓ Az.Sql module installed successfully." -ForegroundColor Green
        }
        catch {
            throw "Failed to install Az.Sql module. Please run: Install-Module -Name Az.Sql -Scope CurrentUser -Force"
        }
    }
}

# Function to ensure user is logged in to Azure
function Test-AzureLogin {
    try {
        $context = Get-AzContext
        if (-not $context) {
            Write-Host "Not logged in to Azure. Initiating login..." -ForegroundColor Yellow
            Connect-AzAccount
        }
        else {
            Write-Host "✓ Logged in as: $($context.Account.Id)" -ForegroundColor Green
            Write-Host "✓ Subscription: $($context.Subscription.Name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "Logging in to Azure..." -ForegroundColor Yellow
        Connect-AzAccount
    }
}

# Main script execution
try {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  Azure SQL Firewall Rule Manager" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    # Check if Az.Sql module is installed
    Test-AzModuleInstalled
    
    # Ensure user is logged in to Azure
    Test-AzureLogin
    
    # Get current public IP
    $currentIP = Get-PublicIPAddress
    
    # Generate rule name if not provided
    if ([string]::IsNullOrEmpty($RuleName)) {
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $hostname = $env:COMPUTERNAME
        $RuleName = "AutoRule-$hostname-$timestamp"
    }
    
    Write-Host "`nAdding firewall rule..." -ForegroundColor Cyan
    Write-Host "  Server: $ServerName" -ForegroundColor White
    Write-Host "  Resource Group: $ResourceGroupName" -ForegroundColor White
    Write-Host "  Rule Name: $RuleName" -ForegroundColor White
    Write-Host "  IP Address: $currentIP" -ForegroundColor White
    
    # Check if rule with same IP already exists
    Write-Host "`nChecking existing firewall rules..." -ForegroundColor Cyan
    $existingRules = Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $ServerName
    $existingRule = $existingRules | Where-Object { $_.StartIpAddress -eq $currentIP -and $_.EndIpAddress -eq $currentIP }
    
    if ($existingRule) {
        Write-Host "✓ A firewall rule for IP $currentIP already exists: $($existingRule.FirewallRuleName)" -ForegroundColor Green
        Write-Host "`nNo action needed. Your IP is already allowed." -ForegroundColor Green
    }
    else {
        # Add the firewall rule
        $result = New-AzSqlServerFirewallRule `
            -ResourceGroupName $ResourceGroupName `
            -ServerName $ServerName `
            -FirewallRuleName $RuleName `
            -StartIpAddress $currentIP `
            -EndIpAddress $currentIP
        
        Write-Host "`n✓ Firewall rule added successfully!" -ForegroundColor Green
        Write-Host "  Rule Name: $($result.FirewallRuleName)" -ForegroundColor White
        Write-Host "  Start IP: $($result.StartIpAddress)" -ForegroundColor White
        Write-Host "  End IP: $($result.EndIpAddress)" -ForegroundColor White
    }
    
    Write-Host "`n✓ You can now connect to the SQL Server from this IP address." -ForegroundColor Green
    
    # List all current firewall rules
    Write-Host "`nCurrent firewall rules for $ServerName :" -ForegroundColor Cyan
    $allRules = Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $ServerName
    $allRules | Format-Table FirewallRuleName, StartIpAddress, EndIpAddress -AutoSize
    
}
catch {
    Write-Host "`n❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Ensure you have Azure PowerShell installed: Install-Module -Name Az -Scope CurrentUser" -ForegroundColor Yellow
    Write-Host "  2. Verify you're logged in: Connect-AzAccount" -ForegroundColor Yellow
    Write-Host "  3. Check you have permissions to modify SQL Server firewall rules" -ForegroundColor Yellow
    Write-Host "  4. Verify the server name and resource group are correct" -ForegroundColor Yellow
    exit 1
}
