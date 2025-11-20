# Hemodialysis Scheduler System - Complete Setup Guide

## Overview
Complete step-by-step instructions to set up and run the HD Scheduler project from scratch.

## Prerequisites

### Required Software
1. **Visual Studio 2022** or **VS Code** with C# extension
2. **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download/dotnet/8.0
3. **Node.js 18+** - Download from https://nodejs.org/
4. **SQL Server 2019+** - Download SQL Server Express or use Azure SQL Database
5. **SQL Server Management Studio (SSMS)** - Download from Microsoft

### Verify Installations
```powershell
# Check .NET version
dotnet --version  # Should output 8.0.x

# Check Node.js and npm
node --version    # Should output v18.x or higher
npm --version     # Should output 9.x or higher

# Check SQL Server
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

---

## Part 1: Database Setup

### Step 1: Create Database
```powershell
# Open SQL Server Management Studio or run:
sqlcmd -S localhost -Q "CREATE DATABASE HDScheduler"
```

### Step 2: Run Schema Script
```powershell
cd Database
sqlcmd -S localhost -d HDScheduler -i 01_CreateSchema.sql
```

Expected output: "Database schema created successfully!"

### Step 3: Insert Seed Data
```powershell
sqlcmd -S localhost -d HDScheduler -i 02_SeedData.sql
```

Expected output: Default user credentials displayed

### Step 4: Verify Database
```powershell
sqlcmd -S localhost -d HDScheduler -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
```

You should see: Users, Patients, Staff, Slots, BedAssignments

---

## Part 2: Backend API Setup

### Step 1: Navigate to Backend
```powershell
cd ..\Backend
```

### Step 2: Restore NuGet Packages
```powershell
dotnet restore
```

### Step 3: Update Connection String (if needed)
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HDScheduler;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For Azure SQL Database:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=HDScheduler;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;"
  }
}
```

### Step 4: Build the API
```powershell
dotnet build
```

Expected output: "Build succeeded. 0 Warning(s), 0 Error(s)"

### Step 5: Run the API
```powershell
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

### Step 6: Test the API
Open browser and navigate to: https://localhost:7001/swagger

You should see the Swagger UI with all API endpoints.

### Step 7: Test Login API
```powershell
# In a new PowerShell window
$body = @{
    username = "admin"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7001/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$response.data.token
```

If you see a JWT token, the API is working correctly!

---

## Part 3: Frontend Setup

### Step 1: Navigate to Frontend
```powershell
cd ..\Frontend
```

### Step 2: Create Angular Project
```powershell
# Create new Angular 17 application
ng new hd-scheduler-app --routing --style=scss --skip-git

# When prompted:
# - Would you like to add Angular routing? Yes
# - Which stylesheet format would you like to use? SCSS
```

### Step 3: Navigate to Project
```powershell
cd hd-scheduler-app
```

### Step 4: Install Angular Material
```powershell
ng add @angular/material

# When prompted:
# - Choose a prebuilt theme: Indigo/Pink
# - Set up global typography styles: Yes
# - Include Angular animations: Yes
```

### Step 5: Install Additional Dependencies
```powershell
npm install @auth0/angular-jwt
```

### Step 6: Copy Project Files
The project structure has been created in the Frontend folder. Now copy the pre-created files:

```powershell
# Core files already exist in:
# - Frontend/hd-scheduler-app/src/app/core/
# - Frontend/hd-scheduler-app/src/environments/

# You can now generate components:
```

### Step 7: Generate Components
```powershell
# Auth components
ng generate component features/auth/login --skip-tests

# Dashboard components
ng generate component features/dashboard/admin-dashboard --skip-tests
ng generate component features/dashboard/hod-dashboard --skip-tests
ng generate component features/dashboard/staff-entry --skip-tests
ng generate component features/dashboard/technician-view --skip-tests

# Patient components
ng generate component features/patients/patient-list --skip-tests
ng generate component features/patients/patient-form --skip-tests

# Schedule components
ng generate component features/schedule/schedule-grid --skip-tests
ng generate component features/schedule/bed-assignment --skip-tests
```

### Step 8: Update Environment Files
The environment files are already created with correct API URLs.

### Step 9: Configure App
Create/update `src/app/app.config.ts`:
```typescript
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient()
  ]
};
```

### Step 10: Run Development Server
```powershell
ng serve --open
```

Expected output:
```
✔ Browser application bundle generation complete.
Initial Chunk Files | Names         |  Raw Size
main.js             | main          | 200.50 kB |

** Angular Live Development Server is listening on localhost:4200 **
```

The application should open at http://localhost:4200

---

## Part 4: Verification & Testing

### Backend Verification

1. **API Health Check**
   - Open: https://localhost:7001/swagger
   - You should see all API endpoints

2. **Test Authentication**
   ```powershell
   # Test login
   curl -X POST https://localhost:7001/api/auth/login `
     -H "Content-Type: application/json" `
     -d '{"username":"admin","password":"Admin@123"}'
   ```

3. **Test Patients Endpoint**
   ```powershell
   # Get all patients (requires token from login)
   curl -X GET https://localhost:7001/api/patients `
     -H "Authorization: Bearer YOUR_TOKEN_HERE"
   ```

### Frontend Verification

1. **Login Page**
   - Navigate to http://localhost:4200
   - Should redirect to login page

2. **Test Login**
   - Username: `admin`
   - Password: `Admin@123`
   - Should successfully login and redirect to dashboard

3. **Test Navigation**
   - Verify role-based routing works
   - Check that different roles see different dashboards

### Database Verification

```sql
-- Check users
SELECT Username, Role FROM Users WHERE IsActive = 1;

-- Check patients
SELECT PatientID, Name, SlotID, BedNumber FROM Patients WHERE IsDischarged = 0;

-- Check slots
SELECT * FROM Slots;

-- Check bed assignments
SELECT * FROM BedAssignments WHERE IsActive = 1;
```

---

## Part 5: Deployment (Optional)

### Deploy to Azure

#### Backend Deployment
```powershell
# Login to Azure
az login

# Create App Service
az webapp create --resource-group EnsateBlogRG --plan EnsateBlogPlan-Linux --name hd-scheduler-api --runtime "DOTNETCORE:8.0"

# Configure connection string
az webapp config connection-string set --resource-group EnsateBlogRG --name hd-scheduler-api --settings DefaultConnection="YOUR_AZURE_SQL_CONNECTION_STRING" --connection-string-type SQLAzure

# Deploy
cd Backend
dotnet publish -c Release
cd bin/Release/net8.0/publish
Compress-Archive -Path * -DestinationPath ../deploy.zip
az webapp deployment source config-zip --resource-group EnsateBlogRG --name hd-scheduler-api --src ../deploy.zip
```

#### Frontend Deployment
```powershell
# Build for production
cd Frontend/hd-scheduler-app
ng build --configuration production

# Deploy to Azure Static Web Apps or Azure App Service
# (Instructions depend on chosen hosting method)
```

---

## Troubleshooting

### Database Connection Issues

**Problem:** Cannot connect to SQL Server
```powershell
# Check SQL Server is running
Get-Service MSSQLSERVER

# Start SQL Server if stopped
Start-Service MSSQLSERVER
```

### Backend Build Errors

**Problem:** NuGet restore fails
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

### Frontend Issues

**Problem:** Angular CLI not found
```powershell
# Install globally
npm install -g @angular/cli@17
```

**Problem:** Port 4200 already in use
```powershell
# Use different port
ng serve --port 4201
```

### API Connection Issues

**Problem:** CORS errors in browser console
- Verify `AllowAngularApp` policy in Backend/Program.cs includes your frontend URL

**Problem:** 401 Unauthorized
- Check JWT token is being sent in Authorization header
- Verify token hasn't expired
- Check user has required role for endpoint

---

## Default Credentials

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | Admin@123 |
| HOD | hod | Hod@123 |
| Doctor | doctor1 | Doctor@123 |
| Nurse | nurse1 | Nurse@123 |
| Technician | tech1 | Tech@123 |

⚠️ **IMPORTANT:** Change these passwords immediately in production!

---

## Project Structure Summary

```
HD_Project/
├── Backend/                    # ASP.NET Core Web API
│   ├── Controllers/           # API endpoints
│   ├── Services/              # Business logic
│   ├── Repositories/          # Data access with Dapper
│   ├── Models/                # Domain models
│   ├── DTOs/                  # Data transfer objects
│   └── appsettings.json       # Configuration
│
├── Frontend/                  # Angular application
│   └── hd-scheduler-app/
│       └── src/
│           ├── app/
│           │   ├── core/      # Services, guards, interceptors
│           │   ├── features/  # Feature modules
│           │   └── shared/    # Shared components
│           └── environments/  # Environment configs
│
├── Database/                  # SQL scripts
│   ├── 01_CreateSchema.sql   # Database schema
│   ├── 02_SeedData.sql        # Initial data
│   └── README.md              # Database setup guide
│
└── Documentation/             # Project documentation
```

---

## Next Steps

1. ✅ Verify all three tiers are running (Database, Backend, Frontend)
2. ✅ Test login with default credentials
3. ✅ Create a test patient
4. ✅ Assign patient to a bed
5. ✅ View daily schedule
6. 📝 Implement remaining dashboard features
7. 📝 Add form validations
8. 📝 Implement real-time updates with SignalR
9. 📝 Add comprehensive error handling
10. 📝 Deploy to Azure

---

## Support & Documentation

- **Technical Specification:** See `HD_Scheduler_Technical_Specification.md`
- **Backend Documentation:** See `Backend/README.md`
- **Frontend Guide:** See `Frontend/README.md`
- **Database Guide:** See `Database/README.md`

---

## License

Proprietary - All rights reserved

---

*Last Updated: January 2025*
