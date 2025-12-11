#Requires -Modules Az.Sql

<#
.SYNOPSIS
    Quick script to add current IP to Azure SQL Server firewall (for hds-dev-sqlserver-cin).

.DESCRIPTION
    Simple version with hardcoded values for quick execution.
    Just run: .\add-azure-sql-firewall-rule-simple.ps1
#>

# Configuration - Update these values if needed
$ServerName = "hds-dev-sqlserver-cin"
$ResourceGroupName = "hds-dev-rg"

Write-Host "`n🔧 Adding your IP to Azure SQL Server firewall..." -ForegroundColor Cyan

# Get public IP
Write-Host "Getting your public IP..." -ForegroundColor Yellow
$currentIP = (Invoke-RestMethod -Uri "https://api.ipify.org" -TimeoutSec 5).Trim()
Write-Host "Your IP: $currentIP" -ForegroundColor Green

# Check if logged in to Azure
try {
    $context = Get-AzContext -ErrorAction Stop
    if (-not $context) {
        Write-Host "Logging in to Azure..." -ForegroundColor Yellow
        Connect-AzAccount
    }
}
catch {
    Write-Host "Logging in to Azure..." -ForegroundColor Yellow
    Connect-AzAccount
}

# Generate rule name
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$RuleName = "AutoRule-$env:COMPUTERNAME-$timestamp"

# Add firewall rule
Write-Host "Adding firewall rule: $RuleName" -ForegroundColor Yellow
try {
    New-AzSqlServerFirewallRule `
        -ResourceGroupName $ResourceGroupName `
        -ServerName $ServerName `
        -FirewallRuleName $RuleName `
        -StartIpAddress $currentIP `
        -EndIpAddress $currentIP `
        -ErrorAction Stop | Out-Null
    
    Write-Host "✓ Success! Your IP ($currentIP) has been added to the firewall." -ForegroundColor Green
}
catch {
    if ($_.Exception.Message -like "*already exists*") {
        Write-Host "✓ Your IP is already allowed in the firewall." -ForegroundColor Green
    }
    else {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`nYou can now connect to the database!`n" -ForegroundColor Green
