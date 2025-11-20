# Azure SQL Database Deployment Guide
## HD Scheduler Production Database Setup

### Current Status
- ✅ Local Development: Working with LocalDB
- ❌ Azure SQL Database: Not yet configured
- ❌ Zone Redundancy: Not enabled
- ❌ Automated Backups: Not configured
- ❌ High Availability: Not implemented

---

## Implementation Checklist

### 1. Create Azure SQL Server & Database

#### Option A: Using Azure Portal
1. Navigate to [Azure Portal](https://portal.azure.com)
2. Create Resource → Azure SQL → SQL Database
3. Configuration:
   - **Resource Group:** `HDSchedulerRG` (new or existing)
   - **Database Name:** `HDSchedulerProd`
   - **Server:** Create new
     - **Server Name:** `hd-scheduler-prod` (must be globally unique)
     - **Location:** `South India` or `Southeast Asia`
     - **Authentication:** SQL Authentication
     - **Admin Login:** `sqladmin`
     - **Password:** (Strong password - save securely!)
   - **Compute + Storage:**
     - **Service Tier:** Standard S2 (50 DTUs) or higher
     - **Storage:** 250 GB minimum
     - **Zone Redundancy:** ✅ **Enable** (Critical for 99.99% SLA)
   - **Backup Storage Redundancy:** Geo-redundant (GRS)

#### Option B: Using Azure CLI

```bash
# Login to Azure
az login

# Create resource group (if not exists)
az group create \
  --name HDSchedulerRG \
  --location southindia

# Create SQL Server
az sql server create \
  --name hd-scheduler-prod \
  --resource-group HDSchedulerRG \
  --location southindia \
  --admin-user sqladmin \
  --admin-password 'YourStrongP@ssw0rd!'

# Create SQL Database with Zone Redundancy
az sql db create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --name HDSchedulerProd \
  --service-objective S2 \
  --zone-redundant true \
  --backup-storage-redundancy Geo \
  --max-size 250GB

# Configure firewall (allow Azure services)
az sql server firewall-rule create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Add your development IP
az sql server firewall-rule create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --name AllowMyIP \
  --start-ip-address YOUR_IP_ADDRESS \
  --end-ip-address YOUR_IP_ADDRESS
```

---

### 2. Configure Automated Backups

Azure SQL Database provides **automatic backups** by default:

- ✅ **Full backups:** Weekly
- ✅ **Differential backups:** Every 12-24 hours
- ✅ **Transaction log backups:** Every 5-10 minutes
- ✅ **Retention:** 7-35 days (configurable)
- ✅ **Point-in-time restore:** To any second within retention period

#### Configure Long-term Retention (Optional)

```bash
# Set long-term retention policy (keep weekly backups for 1 year)
az sql db ltr-policy set \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --database HDSchedulerProd \
  --weekly-retention P12W \
  --monthly-retention P12M \
  --yearly-retention P5Y \
  --week-of-year 1
```

---

### 3. High Availability Configuration

#### Zone Redundancy (99.99% SLA)
- Automatically distributes database replicas across availability zones
- Protects against datacenter-level failures
- Already enabled with `--zone-redundant true` flag above

#### Verify High Availability

```bash
# Check database properties
az sql db show \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --name HDSchedulerProd \
  --query "{Name:name, ZoneRedundant:zoneRedundant, SKU:currentSku.name}"
```

---

### 4. Update Connection Strings

#### Backend Configuration

**appsettings.Production.json** (Create this file):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hd-scheduler-prod.database.windows.net,1433;Database=HDSchedulerProd;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=True;"
  },
  "JwtSettings": {
    "SecretKey": "GENERATE_NEW_SECURE_KEY_FOR_PRODUCTION",
    "Issuer": "HDSchedulerAPI",
    "Audience": "HDSchedulerClient",
    "ExpiryMinutes": 60
  }
}
```

#### Using Azure Key Vault (Recommended)

```bash
# Create Key Vault
az keyvault create \
  --name hd-scheduler-kv \
  --resource-group HDSchedulerRG \
  --location southindia

# Store connection string
az keyvault secret set \
  --vault-name hd-scheduler-kv \
  --name ConnectionString \
  --value "Server=tcp:hd-scheduler-prod.database.windows.net,1433;Database=HDSchedulerProd;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;"
```

---

### 5. Deploy Database Schema

#### Using Azure Data Studio or SSMS

1. Connect to Azure SQL Database:
   - **Server:** `hd-scheduler-prod.database.windows.net`
   - **Database:** `HDSchedulerProd`
   - **Authentication:** SQL Authentication
   - **Login:** `sqladmin`
   - **Password:** (your password)

2. Execute scripts in order:
   ```
   01_CreateSchema.sql
   02_SeedData.sql (with PRODUCTION passwords!)
   03_UpdateSchemaForHDLog.sql
   ```

#### Using sqlcmd

```powershell
# Set variables
$server = "hd-scheduler-prod.database.windows.net"
$database = "HDSchedulerProd"
$user = "sqladmin"
$password = "YOUR_PASSWORD"

# Run schema
sqlcmd -S $server -d $database -U $user -P $password -i "01_CreateSchema.sql"

# Run seed data (MODIFY PASSWORDS FIRST!)
sqlcmd -S $server -d $database -U $user -P $password -i "02_SeedData.sql"

# Run HD Log updates
sqlcmd -S $server -d $database -U $user -P $password -i "03_UpdateSchemaForHDLog.sql"
```

---

### 6. Monitoring & Alerts

#### Enable Diagnostic Logs

```bash
# Create Log Analytics workspace
az monitor log-analytics workspace create \
  --resource-group HDSchedulerRG \
  --workspace-name hd-scheduler-logs

# Get workspace ID
$workspaceId = az monitor log-analytics workspace show \
  --resource-group HDSchedulerRG \
  --workspace-name hd-scheduler-logs \
  --query id -o tsv

# Enable diagnostics
az monitor diagnostic-settings create \
  --name DBDiagnostics \
  --resource /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/HDSchedulerRG/providers/Microsoft.Sql/servers/hd-scheduler-prod/databases/HDSchedulerProd \
  --workspace $workspaceId \
  --logs '[{"category":"SQLInsights","enabled":true},{"category":"QueryStoreRuntimeStatistics","enabled":true}]' \
  --metrics '[{"category":"Basic","enabled":true}]'
```

#### Create Alerts

```bash
# Alert for high DTU usage
az monitor metrics alert create \
  --name HighDTU \
  --resource-group HDSchedulerRG \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/HDSchedulerRG/providers/Microsoft.Sql/servers/hd-scheduler-prod/databases/HDSchedulerProd \
  --condition "avg dtu_consumption_percent > 80" \
  --window-size 5m \
  --evaluation-frequency 1m
```

---

### 7. Security Best Practices

#### Enable Advanced Data Security

```bash
az sql db threat-policy update \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-prod \
  --database HDSchedulerProd \
  --state Enabled \
  --storage-account hdschedulersecurity
```

#### Enable Transparent Data Encryption (TDE)
TDE is **enabled by default** on Azure SQL Database.

#### Use Managed Identity (for App Service)

```bash
# Enable system-assigned identity on App Service
az webapp identity assign \
  --resource-group HDSchedulerRG \
  --name hd-scheduler-api

# Grant SQL access to managed identity
# (Run this SQL command in Azure SQL)
```

```sql
CREATE USER [hd-scheduler-api] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [hd-scheduler-api];
ALTER ROLE db_datawriter ADD MEMBER [hd-scheduler-api];
```

---

### 8. Cost Optimization

#### Recommendations:
- **Development:** S2 (50 DTUs) - ~$150/month
- **Production:** S3 (100 DTUs) or P2 (250 DTUs) - $300-600/month
- **Scale:** Use auto-scaling for variable workloads
- **Backup Storage:** Use LRS instead of GRS if not needed

#### Monitor Costs

```bash
az consumption usage list \
  --start-date 2025-01-01 \
  --end-date 2025-01-31 \
  --query "[?contains(instanceName, 'hd-scheduler-prod')]"
```

---

## Verification Checklist

- [ ] Azure SQL Server created in South India/Southeast Asia
- [ ] Database created with zone redundancy enabled
- [ ] Automated backups configured (7-35 days retention)
- [ ] Long-term retention policy set (optional)
- [ ] Firewall rules configured
- [ ] Connection string updated in application
- [ ] Database schema deployed
- [ ] Production user passwords changed
- [ ] Diagnostic logging enabled
- [ ] Performance alerts configured
- [ ] Advanced Data Security enabled
- [ ] Backup restoration tested
- [ ] Failover tested (zone redundancy)

---

## Cost Estimate

| Component | Configuration | Monthly Cost (USD) |
|-----------|--------------|-------------------|
| Azure SQL Database | S2 (50 DTUs) | ~$150 |
| Zone Redundancy | Enabled | +$75 |
| Backup Storage | 250 GB GRS | ~$20 |
| **Total** | | **~$245/month** |

*Prices as of 2025, may vary by region*

---

## Support & Documentation

- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/sql-database/)
- [High Availability](https://docs.microsoft.com/azure/azure-sql/database/high-availability-sla)
- [Automated Backups](https://docs.microsoft.com/azure/azure-sql/database/automated-backups-overview)
- [Zone Redundancy](https://docs.microsoft.com/azure/azure-sql/database/high-availability-sla#zone-redundant-configuration)
