#Requires -Modules Az.Sql

<#
.SYNOPSIS
    Ultra-simple script to add your IP to Azure SQL Server firewall - No Azure login required if already authenticated.

.DESCRIPTION
    This version checks if you're already logged in and skips the login prompt if possible.
    If not logged in, it will guide you through the process.
#>

# Configuration
$ServerName = "hds-dev-sqlserver-cin"
$ResourceGroupName = "hds-dev-rg"

Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Azure SQL Firewall Quick Update" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Step 1: Check if Az.Sql module is installed
Write-Host "Step 1: Checking Azure PowerShell module..." -ForegroundColor Yellow
if (-not (Get-Module -ListAvailable -Name Az.Sql)) {
    Write-Host "❌ Az.Sql module not found. Installing..." -ForegroundColor Red
    Write-Host "This may take a few minutes..." -ForegroundColor Yellow
    try {
        Install-Module -Name Az.Sql -Scope CurrentUser -Force -AllowClobber -ErrorAction Stop
        Write-Host "✓ Module installed successfully!`n" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to install module. Please run as administrator or install manually:" -ForegroundColor Red
        Write-Host "   Install-Module -Name Az.Sql -Scope CurrentUser -Force" -ForegroundColor Yellow
        exit 1
    }
}
else {
    Write-Host "✓ Az.Sql module is installed`n" -ForegroundColor Green
}

# Step 2: Check Azure login status
Write-Host "Step 2: Checking Azure login status..." -ForegroundColor Yellow
$needsLogin = $false
try {
    $context = Get-AzContext -ErrorAction Stop
    if ($null -eq $context -or $null -eq $context.Account) {
        $needsLogin = $true
    }
    else {
        Write-Host "✓ Already logged in as: $($context.Account.Id)" -ForegroundColor Green
        Write-Host "✓ Subscription: $($context.Subscription.Name)`n" -ForegroundColor Green
    }
}
catch {
    $needsLogin = $true
}

if ($needsLogin) {
    Write-Host "`n⚠️  You need to log in to Azure first." -ForegroundColor Yellow
    Write-Host "`nPlease follow these steps:" -ForegroundColor Cyan
    Write-Host "  1. A browser window will open (or check your browser)" -ForegroundColor White
    Write-Host "  2. Select your Microsoft account" -ForegroundColor White
    Write-Host "  3. Sign in with your credentials" -ForegroundColor White
    Write-Host "  4. Close the browser tab when it says 'Authentication complete'" -ForegroundColor White
    Write-Host "  5. Come back to this PowerShell window`n" -ForegroundColor White
    
    $response = Read-Host "Press ENTER to open browser and login, or type 'cancel' to exit"
    if ($response -eq "cancel") {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "`nOpening browser for authentication..." -ForegroundColor Yellow
    Write-Host "⚠️  If browser doesn't open, manually go to: https://microsoft.com/devicelogin" -ForegroundColor Yellow
    
    try {
        Connect-AzAccount -ErrorAction Stop | Out-Null
        Write-Host "✓ Login successful!`n" -ForegroundColor Green
    }
    catch {
        Write-Host "`n❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
        Write-Host "  - Check if your browser has pop-up blockers enabled" -ForegroundColor White
        Write-Host "  - Try closing and reopening PowerShell" -ForegroundColor White
        Write-Host "  - Make sure you're using the correct Microsoft account" -ForegroundColor White
        exit 1
    }
}

# Step 3: Get current public IP
Write-Host "Step 3: Detecting your public IP address..." -ForegroundColor Yellow
$currentIP = $null
$services = @(
    "https://api.ipify.org",
    "https://ifconfig.me/ip",
    "https://icanhazip.com"
)

foreach ($service in $services) {
    try {
        $currentIP = (Invoke-RestMethod -Uri $service -TimeoutSec 5 -ErrorAction Stop).Trim()
        if ($currentIP -match '^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$') {
            break
        }
    }
    catch {
        continue
    }
}

if (-not $currentIP) {
    Write-Host "❌ Could not detect IP automatically." -ForegroundColor Red
    $currentIP = Read-Host "Please enter your public IP address manually"
    if ([string]::IsNullOrWhiteSpace($currentIP)) {
        Write-Host "No IP address provided. Exiting." -ForegroundColor Red
        exit 1
    }
}

Write-Host "✓ Your IP address: $currentIP`n" -ForegroundColor Green

# Step 4: Check if IP already exists in firewall
Write-Host "Step 4: Checking existing firewall rules..." -ForegroundColor Yellow
try {
    $existingRules = Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $ServerName -ErrorAction Stop
    $matchingRule = $existingRules | Where-Object { $_.StartIpAddress -eq $currentIP -and $_.EndIpAddress -eq $currentIP }
    
    if ($matchingRule) {
        Write-Host "✓ Your IP is already allowed!" -ForegroundColor Green
        Write-Host "  Rule name: $($matchingRule.FirewallRuleName)" -ForegroundColor White
        Write-Host "`n✓ No action needed. You should be able to connect now.`n" -ForegroundColor Green
        exit 0
    }
    else {
        Write-Host "✓ IP not found in firewall rules. Adding now...`n" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️  Could not check existing rules: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Continuing to add rule anyway...`n" -ForegroundColor Yellow
}

# Step 5: Add firewall rule
Write-Host "Step 5: Adding firewall rule..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$hostname = $env:COMPUTERNAME
$ruleName = "AutoRule-$hostname-$timestamp"

try {
    $result = New-AzSqlServerFirewallRule `
        -ResourceGroupName $ResourceGroupName `
        -ServerName $ServerName `
        -FirewallRuleName $ruleName `
        -StartIpAddress $currentIP `
        -EndIpAddress $currentIP `
        -ErrorAction Stop
    
    Write-Host "✓ Firewall rule added successfully!" -ForegroundColor Green
    Write-Host "  Rule name: $ruleName" -ForegroundColor White
    Write-Host "  IP address: $currentIP" -ForegroundColor White
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  SUCCESS! You can now connect to the database." -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════`n" -ForegroundColor Cyan
}
catch {
    if ($_.Exception.Message -like "*already exists*" -or $_.Exception.Message -like "*conflict*") {
        Write-Host "✓ Your IP is already in the firewall (rule exists with different name)" -ForegroundColor Green
        Write-Host "`n✓ You should be able to connect now.`n" -ForegroundColor Green
    }
    else {
        Write-Host "`n❌ Error adding firewall rule:" -ForegroundColor Red
        Write-Host "$($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`nPossible solutions:" -ForegroundColor Yellow
        Write-Host "  1. Check if server name is correct: $ServerName" -ForegroundColor White
        Write-Host "  2. Check if resource group is correct: $ResourceGroupName" -ForegroundColor White
        Write-Host "  3. Verify you have permissions to modify SQL Server firewall" -ForegroundColor White
        Write-Host "  4. Try adding the rule manually in Azure Portal" -ForegroundColor White
        exit 1
    }
}
