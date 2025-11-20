# Azure SQL Database - Free/Low-Cost Options
## HD Scheduler Database on a Budget

---

## ⚠️ Important: No Truly "Free" Azure SQL Database

Azure SQL Database is a paid service. However, here are the **most affordable options**:

---

## Option 1: Azure SQL Database (CHEAPEST) - ~$5/month

### Basic Tier - Perfect for Development/Testing

```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name HDSchedulerRG \
  --location southindia

# Create SQL Server
az sql server create \
  --name hd-scheduler-$(date +%s) \
  --resource-group HDSchedulerRG \
  --location southindia \
  --admin-user sqladmin \
  --admin-password 'YourStrongP@ssw0rd!'

# Create Basic Tier Database (CHEAPEST - ~$5/month)
az sql db create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-XXXXX \
  --name HDScheduler \
  --service-objective Basic \
  --max-size 2GB \
  --backup-storage-redundancy Local

# Configure firewall
az sql server firewall-rule create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-XXXXX \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

**Cost:** ~$5/month (₹400/month)
**Limits:**
- 2 GB storage
- 5 DTUs (basic performance)
- ❌ No zone redundancy
- ✅ Automated backups (7 days)
- ✅ 99.9% SLA (not 99.99%)

---

## Option 2: Azure Free Account Credits

### Get $200 Free Credits for 30 Days

1. Sign up for Azure Free Account: https://azure.microsoft.com/free/
2. Get **$200 USD credit** valid for 30 days
3. Use Standard S2 tier during trial period
4. After 30 days, downgrade to Basic tier

**Steps:**
1. Go to https://azure.microsoft.com/free/
2. Click "Start free"
3. Sign in with Microsoft account
4. Verify identity (credit card required but won't be charged during trial)
5. Use the $200 credit for any Azure service

---

## Option 3: Azure for Students - FREE

### If you're a student, get $100 credit annually

**Eligibility:**
- Must have valid student email (.edu or university domain)
- No credit card required

**Sign up:**
1. Visit: https://azure.microsoft.com/free/students/
2. Verify with student email
3. Get **$100 credit per year** (renews annually while student)

```bash
# After signing up, use the same commands as Option 1
# Your credits will be used instead of billing
```

---

## Option 4: Use FREE Alternative Databases

### Since Azure SQL isn't free, consider these FREE alternatives:

### A. PostgreSQL on Supabase (FREE Forever)
- **Cost:** FREE (up to 500MB)
- **Features:** Automated backups, REST API, real-time subscriptions
- **Link:** https://supabase.com

### B. MongoDB Atlas (FREE Forever)
- **Cost:** FREE (512MB)
- **Features:** Automatic backups, high availability
- **Link:** https://www.mongodb.com/cloud/atlas

### C. Azure Cosmos DB (FREE Tier)
- **Cost:** FREE (25GB storage, 1000 RU/s)
- **Features:** Global distribution, automatic backups
- **Link:** Azure Portal → Cosmos DB → Apply Free Tier

```bash
# Create FREE Cosmos DB
az cosmosdb create \
  --name hd-scheduler-cosmos \
  --resource-group HDSchedulerRG \
  --default-consistency-level Session \
  --enable-free-tier true
```

---

## Recommended Approach for Your Project

### For Development & Testing:

```bash
# Use Azure Free Trial + Basic Tier
# Total cost for first month: $0 (using free credits)
# After free credits: ~$5/month

az login

# Create resource group
az group create \
  --name HDSchedulerRG \
  --location southindia

# Create SQL Server (replace XXXXX with unique name)
az sql server create \
  --name hd-scheduler-dev-12345 \
  --resource-group HDSchedulerRG \
  --location southindia \
  --admin-user sqladmin \
  --admin-password 'HD$cheduler2025!'

# Create Basic Database
az sql db create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-dev-12345 \
  --name HDScheduler \
  --service-objective Basic \
  --max-size 2GB \
  --backup-storage-redundancy Local \
  --zone-redundant false

# Allow Azure services
az sql server firewall-rule create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-dev-12345 \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your IP (replace with your IP)
az sql server firewall-rule create \
  --resource-group HDSchedulerRG \
  --server hd-scheduler-dev-12345 \
  --name AllowMyIP \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 255.255.255.255

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --server hd-scheduler-dev-12345 \
  --name HDScheduler
```

---

## Cost Comparison Table

| Option | Monthly Cost | Storage | Performance | Zone Redundancy | Free Tier |
|--------|-------------|---------|-------------|-----------------|-----------|
| **Azure SQL Basic** | ~$5 (₹400) | 2 GB | 5 DTUs | ❌ | ❌ |
| Azure SQL S2 | ~$150 (₹12,000) | 250 GB | 50 DTUs | ✅ | ❌ |
| **Free Trial Credits** | $0 (30 days) | Any | Any | ✅ | ✅ (30 days) |
| **Student Credits** | $0 | Any | Any | ✅ | ✅ ($100/year) |
| Supabase PostgreSQL | **FREE** | 500 MB | Limited | ❌ | ✅ Forever |
| MongoDB Atlas | **FREE** | 512 MB | Limited | ❌ | ✅ Forever |
| Cosmos DB Free Tier | **FREE** | 25 GB | 1000 RU/s | ✅ | ✅ Forever |

---

## Step-by-Step: Deploy to Azure SQL Basic Tier ($5/month)

### 1. Install Azure CLI (if not installed)
```powershell
# Install Azure CLI
winget install -e --id Microsoft.AzureCLI

# Or download from: https://aka.ms/installazurecliwindows
```

### 2. Login and Create Database
```powershell
# Login to Azure
az login

# Set subscription (if you have multiple)
az account list --output table
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create resource group
az group create --name HDSchedulerRG --location southindia

# Generate unique server name
$serverName = "hd-scheduler-$(Get-Random -Maximum 99999)"
Write-Host "Server name: $serverName"

# Create SQL Server
az sql server create `
  --name $serverName `
  --resource-group HDSchedulerRG `
  --location southindia `
  --admin-user sqladmin `
  --admin-password 'HD$cheduler2025!Secure'

# Create Basic Database ($5/month)
az sql db create `
  --resource-group HDSchedulerRG `
  --server $serverName `
  --name HDScheduler `
  --service-objective Basic `
  --max-size 2GB `
  --backup-storage-redundancy Local `
  --zone-redundant false

# Configure firewall (allow all for development)
az sql server firewall-rule create `
  --resource-group HDSchedulerRG `
  --server $serverName `
  --name AllowAll `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 255.255.255.255

Write-Host "`nConnection String:"
Write-Host "Server=tcp:$serverName.database.windows.net,1433;Database=HDScheduler;User ID=sqladmin;Password=HD`$cheduler2025!Secure;Encrypt=True;TrustServerCertificate=False;"
```

### 3. Update Your Application

Create `appsettings.Azure.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR_SERVER_NAME.database.windows.net,1433;Database=HDScheduler;User ID=sqladmin;Password=HD$cheduler2025!Secure;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 4. Deploy Database Schema

```powershell
# Connect and run scripts
$server = "YOUR_SERVER_NAME.database.windows.net"
$database = "HDScheduler"
$user = "sqladmin"
$password = "HD`$cheduler2025!Secure"

# Run schema
sqlcmd -S $server -d $database -U $user -P $password -i "Database\01_CreateSchema.sql"
sqlcmd -S $server -d $database -U $user -P $password -i "Database\02_SeedData.sql"
sqlcmd -S $server -d $database -U $user -P $password -i "Database\03_UpdateSchemaForHDLog.sql"
```

---

## My Recommendation

**For Your HD Scheduler Project:**

1. **Start with:** Azure Free Trial ($200 credit)
2. **Use:** Basic Tier (~$5/month) after credits expire
3. **Upgrade to:** Standard S2 (~$150/month) when you go to production

**Why Basic Tier?**
- ✅ Cheapest Azure SQL option
- ✅ Perfect for development/testing
- ✅ Automated backups included
- ✅ Can handle ~50-100 patients
- ✅ Can upgrade anytime without downtime

**When to Upgrade?**
- More than 100 active patients
- Need better performance
- Need zone redundancy (99.99% SLA)
- Production deployment

---

## Need Help?

Run this script to get your Azure free trial:
```powershell
Start-Process "https://azure.microsoft.com/free/"
```

Or if you're a student:
```powershell
Start-Process "https://azure.microsoft.com/free/students/"
```

---

**Next Steps:**
1. Sign up for Azure Free Trial
2. Run the PowerShell commands above
3. Update your connection string
4. Deploy the database schema
5. Test your application!
