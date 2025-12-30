# Hemodialysis Scheduler - Deployment Guide

## Table of Contents
- [Prerequisites](#prerequisites)
- [Local Development Setup](#local-development-setup)
- [Database Setup](#database-setup)
- [Backend Configuration](#backend-configuration)
- [Frontend Configuration](#frontend-configuration)
- [Running the Application](#running-the-application)
- [Production Deployment](#production-deployment)
- [Docker Preparation](#docker-preparation)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

#### 1. .NET 8.0 SDK
**Download**: https://dotnet.microsoft.com/download/dotnet/8.0

**Verify Installation**:
```powershell
dotnet --version
# Expected output: 8.0.x
```

#### 2. Node.js 18+ and npm
**Download**: https://nodejs.org/

**Verify Installation**:
```powershell
node --version
# Expected output: v18.x or higher

npm --version
# Expected output: 9.x or higher
```

#### 3. SQL Server 2019+
**Options**:
- SQL Server Express (Free): https://www.microsoft.com/sql-server/sql-server-downloads
- Azure SQL Database (Cloud)
- SQL Server Developer Edition (Free)

**Verify Installation**:
```powershell
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

#### 4. SQL Server Management Studio (SSMS)
**Download**: https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

#### 5. Visual Studio 2022 or VS Code
**Visual Studio 2022**: https://visualstudio.microsoft.com/
**VS Code**: https://code.visualstudio.com/

**VS Code Extensions**:
- C# Dev Kit
- Angular Language Service
- ESLint
- Prettier

#### 6. Git
**Download**: https://git-scm.com/

**Verify Installation**:
```powershell
git --version
```

---

## Local Development Setup

### 1. Clone Repository

```powershell
cd G:\ENSATE\HdScheduler_Cloud
git clone https://github.com/TalipotTech/Hemodialysis-scheduler.git
cd Hemodialysis-scheduler
```

### 2. Project Structure
```
Hemodialysis-scheduler/
├── Backend/                    # ASP.NET Core API
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   └── HDScheduler.API.csproj
├── Frontend/                   # Angular Application
│   └── hd-scheduler-app/
│       ├── src/
│       ├── angular.json
│       └── package.json
├── Database/                   # SQL Scripts
│   ├── 01_CreateSchema.sql
│   ├── 02_SeedData.sql
│   └── SqlServer/
└── README.md
```

---

## Database Setup

### Option 1: Local SQL Server

#### Step 1: Create Database
```powershell
sqlcmd -S localhost -Q "CREATE DATABASE HDScheduler"
```

#### Step 2: Run Schema Script
```powershell
cd Database
sqlcmd -S localhost -d HDScheduler -i 01_CreateSchema.sql
```

**Expected Output**:
```
Database schema created successfully!
```

#### Step 3: Seed Initial Data
```powershell
sqlcmd -S localhost -d HDScheduler -i 02_SeedData.sql
```

**Default Credentials Created**:
```
Username: admin
Password: Admin@123
Role: Admin

Username: doctor1
Password: Doctor@123
Role: Doctor

Username: nurse1
Password: Nurse@123
Role: Nurse
```

#### Step 4: Verify Database
```powershell
sqlcmd -S localhost -d HDScheduler -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
```

**Expected Tables**:
- Users
- Patients
- HDSchedule
- HDLogs
- Staff
- Slots
- BedAssignments
- AuditLogs
- IntraDialyticRecords
- PostDialysisMedications
- AISettings
- AIUsageLogs
- SavedPrompts

### Option 2: Azure SQL Database

#### Step 1: Create Azure SQL Database
```powershell
# Install Azure CLI
winget install Microsoft.AzureCLI

# Login to Azure
az login

# Create resource group
az group create --name hd-scheduler-rg --location eastus

# Create SQL Server
az sql server create `
  --name hds-dev-sqlserver `
  --resource-group hd-scheduler-rg `
  --location eastus `
  --admin-user hdsadmin `
  --admin-password "YourStrongPassword123!"

# Create database
az sql db create `
  --resource-group hd-scheduler-rg `
  --server hds-dev-sqlserver `
  --name hds-dev-db `
  --service-objective S0
```

#### Step 2: Configure Firewall
```powershell
# Get your public IP
$myIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content

# Add firewall rule
az sql server firewall-rule create `
  --resource-group hd-scheduler-rg `
  --server hds-dev-sqlserver `
  --name AllowMyIP `
  --start-ip-address $myIP `
  --end-ip-address $myIP
```

#### Step 3: Get Connection String
```powershell
az sql db show-connection-string `
  --client ado.net `
  --server hds-dev-sqlserver `
  --name hds-dev-db
```

**Connection String Format**:
```
Server=tcp:hds-dev-sqlserver.database.windows.net,1433;
Initial Catalog=hds-dev-db;
Persist Security Info=False;
User ID=hdsadmin;
Password={your_password};
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

#### Step 4: Run Migrations
```powershell
# Connect using sqlcmd
sqlcmd -S hds-dev-sqlserver.database.windows.net `
       -d hds-dev-db `
       -U hdsadmin `
       -P "YourStrongPassword123!" `
       -i Database\01_CreateSchema.sql
```

---

## Backend Configuration

### Step 1: Navigate to Backend
```powershell
cd Backend
```

### Step 2: Restore NuGet Packages
```powershell
dotnet restore
```

### Step 3: Configure Connection String

**File**: `Backend/appsettings.Development.json`

**For Local SQL Server**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HDScheduler;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123!@#",
    "Issuer": "HDSchedulerAPI",
    "Audience": "HDSchedulerClient",
    "ExpiryMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**For Azure SQL Database**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:hds-dev-sqlserver.database.windows.net,1433;Initial Catalog=hds-dev-db;User ID=hdsadmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123!@#",
    "Issuer": "HDSchedulerAPI",
    "Audience": "HDSchedulerClient",
    "ExpiryMinutes": 30
  }
}
```

### Step 4: Build Backend
```powershell
dotnet build
```

**Expected Output**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 5: Test Backend
```powershell
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Test API**:
Open browser: http://localhost:5000/swagger

---

## Frontend Configuration

### Step 1: Navigate to Frontend
```powershell
cd ..\Frontend\hd-scheduler-app
```

### Step 2: Install Dependencies
```powershell
npm install
```

**This will install**:
- Angular 20.x
- Syncfusion components
- Angular Material
- RxJS
- Other dependencies (~5-10 minutes)

### Step 3: Configure API URL

**File**: `Frontend/hd-scheduler-app/src/environments/environment.development.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

**File**: `Frontend/hd-scheduler-app/src/environments/environment.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-url.azurewebsites.net/api'
};
```

### Step 4: Build Frontend
```powershell
npm run build
```

**Expected Output**:
```
✔ Browser application bundle generation complete.
Build at: 2025-12-24T14:30:00.000Z
```

### Step 5: Test Frontend Development Server
```powershell
npm start
```

**Expected Output**:
```
** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

**Test Application**:
Open browser: http://localhost:4200

---

## Running the Application

### Full Stack Development

**Terminal 1 - Backend**:
```powershell
cd Backend
dotnet run
```

**Terminal 2 - Frontend**:
```powershell
cd Frontend\hd-scheduler-app
npm start
```

**Access Application**:
- Frontend: http://localhost:4200
- Backend API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

### Testing Login

**Navigate to**: http://localhost:4200/login

**Admin Credentials**:
```
Username: admin
Password: Admin@123
```

**Test Workflow**:
1. Login → Admin Dashboard
2. Navigate to Patients → View patient list
3. Navigate to Schedule → View today's schedule
4. Create new patient
5. Assign bed for dialysis session
6. Document treatment (HD Log)

---

## Production Deployment

### Azure Deployment

#### 1. Backend Deployment (Azure App Service)

**Step 1: Create App Service**:
```powershell
# Create App Service Plan
az appservice plan create `
  --name hd-scheduler-plan `
  --resource-group hd-scheduler-rg `
  --sku B1

# Create Web App
az webapp create `
  --resource-group hd-scheduler-rg `
  --plan hd-scheduler-plan `
  --name hd-scheduler-api `
  --runtime "DOTNET:8.0"
```

**Step 2: Configure Connection String**:
```powershell
az webapp config connection-string set `
  --resource-group hd-scheduler-rg `
  --name hd-scheduler-api `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="Server=tcp:..."
```

**Step 3: Publish Backend**:
```powershell
cd Backend
dotnet publish -c Release -o ./publish

# Create deployment package
Compress-Archive -Path ./publish/* -DestinationPath deploy.zip

# Deploy to Azure
az webapp deployment source config-zip `
  --resource-group hd-scheduler-rg `
  --name hd-scheduler-api `
  --src deploy.zip
```

**Verify**: https://hd-scheduler-api.azurewebsites.net/swagger

#### 2. Frontend Deployment (Azure Static Web Apps)

**Step 1: Build Production**:
```powershell
cd Frontend\hd-scheduler-app
npm run build:prod
```

**Step 2: Create Static Web App**:
```powershell
az staticwebapp create `
  --name hd-scheduler-frontend `
  --resource-group hd-scheduler-rg `
  --location eastus
```

**Step 3: Deploy**:
```powershell
# Install SWA CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy ./dist/hd-scheduler-app/browser `
  --app-name hd-scheduler-frontend `
  --resource-group hd-scheduler-rg
```

**Verify**: https://hd-scheduler-frontend.azurestaticapps.net

---

## Docker Preparation

### Backend Dockerfile

**Create**: `Backend/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["HDScheduler.API.csproj", "./"]
RUN dotnet restore "HDScheduler.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "HDScheduler.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "HDScheduler.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HDScheduler.API.dll"]
```

### Frontend Dockerfile

**Create**: `Frontend/hd-scheduler-app/Dockerfile`

```dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source code
COPY . .
RUN npm run build:prod

# Runtime stage
FROM nginx:alpine
COPY --from=build /app/dist/hd-scheduler-app/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Docker Compose

**Create**: `docker-compose.yml` (root directory)

```yaml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: hd-scheduler-db
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - hd-scheduler-network

  # Backend API
  backend:
    build:
      context: ./Backend
      dockerfile: Dockerfile
    container_name: hd-scheduler-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=HDScheduler;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;
      - JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration123!@#
    ports:
      - "5000:80"
    depends_on:
      - sqlserver
    networks:
      - hd-scheduler-network

  # Frontend
  frontend:
    build:
      context: ./Frontend/hd-scheduler-app
      dockerfile: Dockerfile
    container_name: hd-scheduler-frontend
    ports:
      - "4200:80"
    depends_on:
      - backend
    networks:
      - hd-scheduler-network

volumes:
  sqlserver_data:

networks:
  hd-scheduler-network:
    driver: bridge
```

### Docker Commands

**Build and Run**:
```powershell
# Build images
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Remove volumes (WARNING: deletes data)
docker-compose down -v
```

**Individual Service Management**:
```powershell
# Restart backend only
docker-compose restart backend

# View backend logs
docker-compose logs -f backend

# Execute command in container
docker-compose exec backend bash
```

### Database Initialization in Docker

**Option 1: Manual**:
```powershell
# Copy SQL script to container
docker cp Database/01_CreateSchema.sql hd-scheduler-db:/tmp/

# Execute script
docker exec -it hd-scheduler-db /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "YourStrong@Password123" `
  -i /tmp/01_CreateSchema.sql
```

**Option 2: Automated** (Add to docker-compose.yml):
```yaml
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    volumes:
      - ./Database:/docker-entrypoint-initdb.d
      - sqlserver_data:/var/opt/mssql
```

---

## Troubleshooting

### Common Issues

#### 1. Backend Won't Start

**Error**: "Failed to connect to database"

**Solution**:
```powershell
# Test connection string
sqlcmd -S localhost -d HDScheduler -Q "SELECT 1"

# Check SQL Server service
Get-Service MSSQLSERVER

# Start if stopped
Start-Service MSSQLSERVER
```

#### 2. Frontend Build Errors

**Error**: "Module not found"

**Solution**:
```powershell
# Clear cache and reinstall
rm -r node_modules
rm package-lock.json
npm cache clean --force
npm install
```

#### 3. CORS Errors

**Error**: "Access-Control-Allow-Origin blocked"

**Solution**: Check `Backend/Program.cs` CORS configuration:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

#### 4. JWT Token Issues

**Error**: "Unauthorized 401"

**Solution**: Verify JWT settings match in both frontend and backend:
- Check token expiration (30 minutes default)
- Verify SecretKey is same in appsettings.json
- Check token is being sent in Authorization header

#### 5. Database Migration Errors

**Error**: "Object already exists"

**Solution**:
```powershell
# Drop and recreate database
sqlcmd -S localhost -Q "DROP DATABASE HDScheduler; CREATE DATABASE HDScheduler"

# Re-run migrations
sqlcmd -S localhost -d HDScheduler -i Database\01_CreateSchema.sql
```

#### 6. Port Already in Use

**Error**: "Port 5000 is already in use"

**Solution**:
```powershell
# Find process using port
netstat -ano | findstr :5000

# Kill process (replace PID)
taskkill /PID <PID> /F

# Or change port in Backend/Properties/launchSettings.json
```

### Logging

**Backend Logs**:
```csharp
// Add to appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**View Logs**:
```powershell
# Console output
dotnet run

# Or check Application Insights (Azure)
```

### Performance Optimization

**Backend**:
```csharp
// Add response caching
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Add compression
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

**Frontend**:
```powershell
# Build with optimization
ng build --configuration production --optimization
```

---

## Environment Variables

### Backend (.env or appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HDScheduler;..."
  },
  "JwtSettings": {
    "SecretKey": "SECRET_KEY_HERE",
    "Issuer": "HDSchedulerAPI",
    "Audience": "HDSchedulerClient",
    "ExpiryMinutes": 30
  },
  "AISettings": {
    "GeminiApiKey": "YOUR_GEMINI_API_KEY",
    "DailyCostLimit": 5.00
  }
}
```

### Frontend (environment.ts)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  appVersion: '1.0.0'
};
```

---

## Health Checks

### Backend Health Endpoint

**Add to Program.cs**:
```csharp
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy",
    timestamp = DateTime.UtcNow 
}));
```

**Test**:
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/health"
```

---

## Backup & Recovery

### Database Backup
```powershell
# Backup
sqlcmd -S localhost -Q "BACKUP DATABASE HDScheduler TO DISK='C:\Backups\HDScheduler.bak'"

# Restore
sqlcmd -S localhost -Q "RESTORE DATABASE HDScheduler FROM DISK='C:\Backups\HDScheduler.bak'"
```

### Azure SQL Automated Backups
```powershell
# List backups
az sql db list-deleted --resource-group hd-scheduler-rg --server hds-dev-sqlserver

# Restore
az sql db restore `
  --resource-group hd-scheduler-rg `
  --server hds-dev-sqlserver `
  --name hds-dev-db `
  --time "2025-12-24T00:00:00Z"
```

---

## Next Steps

After completing this setup:

1. **Test all features**: Login, patient management, scheduling, treatment documentation
2. **Configure monitoring**: Application Insights, logging
3. **Set up CI/CD**: GitHub Actions or Azure DevOps
4. **Security hardening**: SSL certificates, firewall rules
5. **Performance testing**: Load testing with k6 or JMeter
6. **User training**: Create user documentation
7. **Docker deployment**: Follow Docker Preparation section above

---

## Support & Resources

- **Documentation**: See other documentation files
  - `SYSTEM_ARCHITECTURE.md`
  - `APPLICATION_WORKFLOW.md`
  - `API_DOCUMENTATION.md`
- **Repository**: https://github.com/TalipotTech/Hemodialysis-scheduler
- **Issues**: Report bugs via GitHub Issues

---

## Documentation Version
**Version**: 1.0  
**Last Updated**: December 24, 2025  
**Author**: TalipotTech Development Team  
**Repository**: https://github.com/TalipotTech/Hemodialysis-scheduler
