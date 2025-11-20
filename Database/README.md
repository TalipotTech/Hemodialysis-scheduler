# Database Setup Instructions

## Prerequisites
- SQL Server 2019 or later (or Azure SQL Database)
- SQL Server Management Studio (SSMS) or Azure Data Studio
- Proper permissions to create databases

## Local Development Setup

### Step 1: Create the Database
```sql
CREATE DATABASE HDScheduler;
GO

USE HDScheduler;
GO
```

### Step 2: Run Schema Script
Execute the `01_CreateSchema.sql` script to create all tables and indexes.

```powershell
# Using sqlcmd (Windows PowerShell)
sqlcmd -S localhost -d HDScheduler -i "01_CreateSchema.sql"
```

Or open the file in SSMS and execute it.

### Step 3: Run Seed Data Script
Execute the `02_SeedData.sql` script to populate initial data.

```powershell
# Using sqlcmd
sqlcmd -S localhost -d HDScheduler -i "02_SeedData.sql"
```

### Step 4: Verify Installation
```sql
-- Check if all tables are created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check user accounts
SELECT Username, Role, IsActive FROM Users;

-- Check slots
SELECT * FROM Slots;
```

## Azure SQL Database Setup

### Step 1: Create Azure SQL Database
```bash
# Login to Azure
az login

# Create database (using existing resource group)
az sql db create \
  --resource-group EnsateBlogRG \
  --server hd-scheduler-dev \
  --name HDSchedulerDev \
  --service-objective Basic \
  --zone-redundant false
```

### Step 2: Configure Firewall
```bash
# Allow your IP
az sql server firewall-rule create \
  --resource-group EnsateBlogRG \
  --server hd-scheduler-dev \
  --name AllowMyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

### Step 3: Update Connection String
Update `appsettings.json` and `appsettings.Development.json` with Azure connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hd-scheduler-dev.database.windows.net,1433;Database=HDSchedulerDev;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
  }
}
```

### Step 4: Run Scripts
Connect to Azure SQL Database using SSMS or Azure Data Studio and execute:
1. `01_CreateSchema.sql`
2. `02_SeedData.sql`

## Default User Accounts

After running the seed data script, you can login with these accounts:

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | Admin@123 |
| HOD | hod | Hod@123 |
| Doctor | doctor1 | Doctor@123 |
| Nurse | nurse1 | Nurse@123 |
| Technician | tech1 | Tech@123 |

**⚠️ IMPORTANT:** Change these passwords immediately in production environments!

## Password Hash Generation

If you need to create new users or update passwords, use this C# code:

```csharp
using BCrypt.Net;

string password = "YourPassword@123";
string hash = BCrypt.HashPassword(password, 12);
Console.WriteLine(hash);
```

Then insert into Users table:
```sql
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('newuser', 'GENERATED_HASH_HERE', 'Doctor', 1, GETDATE());
```

## Troubleshooting

### Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure Windows Authentication or SQL Authentication is enabled
- For Azure: Verify firewall rules

### Permission Issues
- Ensure your SQL user has db_owner permissions
- For Azure: Use proper admin credentials

### Schema Errors
- Drop and recreate database if needed
- Ensure you're using SQL Server 2016 or later for full feature support

## Backup and Restore

### Backup
```sql
BACKUP DATABASE HDScheduler 
TO DISK = 'C:\Backups\HDScheduler.bak'
WITH FORMAT, MEDIANAME = 'HDSchedulerBackup', NAME = 'Full Backup';
```

### Restore
```sql
RESTORE DATABASE HDScheduler 
FROM DISK = 'C:\Backups\HDScheduler.bak'
WITH REPLACE;
```

## Migration to Production

1. Export schema and data from development
2. Create production database
3. Run schema script
4. **Generate new secure passwords** for all default accounts
5. Update seed script with production password hashes
6. Run seed script
7. Verify all data
8. Configure automated backups
9. Set up monitoring
