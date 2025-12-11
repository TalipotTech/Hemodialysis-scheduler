#Requires -Modules Az.Sql

<#
.SYNOPSIS
    Removes old auto-generated firewall rules from Azure SQL Server.

.DESCRIPTION
    This script removes firewall rules that start with "AutoRule-" and are older than a specified number of days.
    Useful for cleaning up old IP addresses that are no longer needed.

.PARAMETER ServerName
    The name of the Azure SQL Server (without .database.windows.net)

.PARAMETER ResourceGroupName
    The name of the Azure Resource Group containing the SQL Server

.PARAMETER DaysOld
    Remove rules older than this many days. Default is 30 days.

.PARAMETER WhatIf
    Preview what would be deleted without actually deleting.

.EXAMPLE
    .\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -DaysOld 30

.EXAMPLE
    .\remove-old-azure-sql-firewall-rules.ps1 -ServerName "hds-dev-sqlserver-cin" -ResourceGroupName "hds-dev-rg" -WhatIf
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName = "hds-dev-sqlserver-cin",
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName = "hds-dev-rg",
    
    [Parameter(Mandatory=$false)]
    [int]$DaysOld = 30,
    
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf
)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Azure SQL Firewall Rule Cleanup" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Ensure user is logged in
try {
    $context = Get-AzContext -ErrorAction Stop
    if (-not $context) {
        Connect-AzAccount
    }
}
catch {
    Connect-AzAccount
}

Write-Host "Server: $ServerName" -ForegroundColor White
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "Removing rules older than: $DaysOld days" -ForegroundColor White

if ($WhatIf) {
    Write-Host "Mode: Preview (WhatIf)`n" -ForegroundColor Yellow
}
else {
    Write-Host "Mode: Delete`n" -ForegroundColor Red
}

# Get all firewall rules
$allRules = Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $ServerName

# Filter auto-generated rules
$autoRules = $allRules | Where-Object { $_.FirewallRuleName -like "AutoRule-*" }

if ($autoRules.Count -eq 0) {
    Write-Host "No auto-generated rules found." -ForegroundColor Green
    exit 0
}

Write-Host "Found $($autoRules.Count) auto-generated rules." -ForegroundColor Cyan

$cutoffDate = (Get-Date).AddDays(-$DaysOld)
$rulesToDelete = @()

foreach ($rule in $autoRules) {
    # Try to extract date from rule name (format: AutoRule-HOSTNAME-yyyyMMdd-HHmmss)
    if ($rule.FirewallRuleName -match "AutoRule-.*-(\d{8})-\d{6}$") {
        $dateStr = $matches[1]
        try {
            $ruleDate = [DateTime]::ParseExact($dateStr, "yyyyMMdd", $null)
            
            if ($ruleDate -lt $cutoffDate) {
                $rulesToDelete += [PSCustomObject]@{
                    Name = $rule.FirewallRuleName
                    IP = $rule.StartIpAddress
                    Date = $ruleDate
                    Age = ((Get-Date) - $ruleDate).Days
                }
            }
        }
        catch {
            # Could not parse date, skip
        }
    }
}

if ($rulesToDelete.Count -eq 0) {
    Write-Host "`nNo rules older than $DaysOld days found." -ForegroundColor Green
    exit 0
}

Write-Host "`nRules to be removed:" -ForegroundColor Yellow
$rulesToDelete | Format-Table Name, IP, Date, @{Name="Days Old";Expression={$_.Age}} -AutoSize

if ($WhatIf) {
    Write-Host "`nWhatIf mode: No rules were deleted." -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to actually delete these rules." -ForegroundColor Yellow
    exit 0
}

# Confirm deletion
$confirmation = Read-Host "`nAre you sure you want to delete these $($rulesToDelete.Count) rules? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host "`nDeleting rules..." -ForegroundColor Red
$deleted = 0
$failed = 0

foreach ($rule in $rulesToDelete) {
    try {
        Remove-AzSqlServerFirewallRule `
            -ResourceGroupName $ResourceGroupName `
            -ServerName $ServerName `
            -FirewallRuleName $rule.Name `
            -Force `
            -ErrorAction Stop
        
        Write-Host "✓ Deleted: $($rule.Name)" -ForegroundColor Green
        $deleted++
    }
    catch {
        Write-Host "✗ Failed to delete: $($rule.Name) - $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Cleanup complete!" -ForegroundColor Green
Write-Host "  Rules deleted: $deleted" -ForegroundColor Green
Write-Host "  Rules failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host "========================================`n" -ForegroundColor Cyan
