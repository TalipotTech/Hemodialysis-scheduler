# HD Scheduler Project - Quick Start Guide

## What Has Been Created

### ✅ Backend API (ASP.NET Core 8.0)
- **Location:** `Backend/`
- **Features:**
  - JWT Authentication with BCrypt password hashing
  - Role-based authorization (Admin, HOD, Doctor, Nurse, Technician)
  - Patient management endpoints
  - Schedule and bed assignment endpoints
  - Dapper for high-performance data access
  - Repository pattern implementation
  - Swagger API documentation
  
**Key Files:**
- `Controllers/` - AuthController, PatientsController, ScheduleController
- `Services/` - AuthService
- `Repositories/` - UserRepository, PatientRepository, ScheduleRepository
- `Models/` - User, Patient, Staff, Schedule
- `Program.cs` - Application configuration with JWT and CORS
- `HDScheduler.API.csproj` - All required NuGet packages included

### ✅ Database (SQL Server)
- **Location:** `Database/`
- **Features:**
  - Complete schema for all tables
  - Seed data with default user accounts
  - Indexes for performance optimization
  - Foreign key relationships
  
**Key Files:**
- `01_CreateSchema.sql` - Creates Users, Patients, Staff, Slots, BedAssignments tables
- `02_SeedData.sql` - Inserts default users and sample data
- `README.md` - Database setup instructions

### ✅ Frontend (Angular 17)
- **Location:** `Frontend/hd-scheduler-app/`
- **Features:**
  - Core services (Auth, Patient, Schedule)
  - JWT interceptor for API calls
  - Route guards for role-based access
  - TypeScript models for type safety
  - Environment configurations
  
**Key Files:**
- `src/app/core/services/` - AuthService, PatientService, ScheduleService
- `src/app/core/models/` - User, Patient, Schedule models
- `src/app/core/guards/` - Auth guard
- `src/app/core/interceptors/` - JWT interceptor
- `src/environments/` - Development and production configurations

### ✅ Documentation
- **Main README.md** - Complete setup guide
- **Backend/README.md** - Backend-specific documentation
- **Frontend/README.md** - Frontend setup instructions
- **Database/README.md** - Database setup guide
- **HD_Scheduler_Technical_Specification.md** - Full technical specification

### ✅ Automation Scripts
- **setup.ps1** - Interactive setup wizard
- **start.ps1** - Start all services at once

---

## Quick Start (3 Simple Steps)

### Option 1: Automated Setup

```powershell
# Run the automated setup script
.\setup.ps1

# Then start all services
.\start.ps1
```

### Option 2: Manual Setup

#### Step 1: Database
```powershell
# Create and setup database
sqlcmd -S localhost -Q "CREATE DATABASE HDScheduler"
sqlcmd -S localhost -d HDScheduler -i Database\01_CreateSchema.sql
sqlcmd -S localhost -d HDScheduler -i Database\02_SeedData.sql
```

#### Step 2: Backend
```powershell
cd Backend
dotnet restore
dotnet build
dotnet run
# API will be at https://localhost:7001
```

#### Step 3: Frontend
```powershell
# In a new terminal
cd Frontend
ng new hd-scheduler-app --routing --style=scss --skip-git
cd hd-scheduler-app
ng add @angular/material --defaults
npm install @auth0/angular-jwt
ng serve
# App will be at http://localhost:4200
```

---

## Default Login Credentials

| Role | Username | Password | Access |
|------|----------|----------|--------|
| Admin | admin | Admin@123 | Full system access |
| HOD | hod | Hod@123 | Management access |
| Doctor | doctor1 | Doctor@123 | Patient management |
| Nurse | nurse1 | Nurse@123 | Patient management |
| Technician | tech1 | Tech@123 | View-only access |

---

## Project Structure

```
HD_Project/
├── Backend/                          # ASP.NET Core 8.0 Web API
│   ├── Controllers/                 # API endpoints (Auth, Patients, Schedule)
│   ├── Services/                    # Business logic (AuthService)
│   ├── Repositories/                # Data access with Dapper
│   ├── Models/                      # Domain models
│   ├── DTOs/                        # API response models
│   ├── Data/                        # Database context (DapperContext)
│   ├── Program.cs                   # App configuration
│   ├── appsettings.json             # Configuration settings
│   └── HDScheduler.API.csproj       # Project file with all packages
│
├── Frontend/                        # Angular 17 Application
│   ├── hd-scheduler-app/
│   │   └── src/
│   │       ├── app/
│   │       │   ├── core/           # Core services, guards, models
│   │       │   ├── features/       # Feature modules (to be generated)
│   │       │   └── shared/         # Shared components
│   │       └── environments/       # Environment configs
│   └── README.md                    # Frontend setup guide
│
├── Database/                        # SQL Server Database
│   ├── 01_CreateSchema.sql         # Create all tables
│   ├── 02_SeedData.sql             # Insert default data
│   ├── PasswordHashGenerator.cs    # Utility to generate BCrypt hashes
│   └── README.md                    # Database documentation
│
├── Documentation/                   # Additional documentation
│
├── setup.ps1                        # Automated setup script
├── start.ps1                        # Start all services script
└── README.md                        # Main setup guide
```

---

## What Works Right Now

### ✅ Backend API
- Authentication and JWT token generation
- User management with role-based access
- Patient CRUD operations
- Schedule management
- Bed assignment and discharge
- Swagger documentation at https://localhost:7001/swagger

### ✅ Database
- All tables created with proper relationships
- 5 default users (one for each role)
- 4 time slots configured
- Sample patient data
- Sample bed assignments

### ✅ Frontend Foundation
- Core services for API communication
- Authentication service with token management
- JWT interceptor for automatic token injection
- Route guards for protected routes
- TypeScript models for type safety
- Environment configurations for dev/prod

---

## What Needs to Be Done Next

### Frontend UI Components (Not Yet Created)
You need to generate and implement these components:

1. **Login Component** - Login form with username/password
2. **Admin Dashboard** - Full system overview
3. **HOD Dashboard** - Staff and schedule management
4. **Staff Entry** - Patient data entry form
5. **Technician View** - Read-only schedule view
6. **Patient List** - Table of all patients
7. **Patient Form** - Create/edit patient form
8. **Schedule Grid** - Daily schedule visualization
9. **Bed Assignment** - Assign patients to beds

### To Generate These:
```powershell
cd Frontend/hd-scheduler-app

# Auth
ng generate component features/auth/login

# Dashboards
ng generate component features/dashboard/admin-dashboard
ng generate component features/dashboard/hod-dashboard
ng generate component features/dashboard/staff-entry
ng generate component features/dashboard/technician-view

# Patients
ng generate component features/patients/patient-list
ng generate component features/patients/patient-form

# Schedule
ng generate component features/schedule/schedule-grid
ng generate component features/schedule/bed-assignment
```

---

## Testing the API

### Test Authentication
```powershell
# Login
$body = @{
    username = "admin"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7001/api/auth/login" `
    -Method Post -Body $body -ContentType "application/json" `
    -SkipCertificateCheck

$token = $response.data.token
Write-Host "Token: $token"
```

### Test Patients Endpoint
```powershell
# Get all patients
Invoke-RestMethod -Uri "https://localhost:7001/api/patients" `
    -Method Get `
    -Headers @{Authorization="Bearer $token"} `
    -SkipCertificateCheck
```

### Test Schedule Endpoint
```powershell
# Get daily schedule
Invoke-RestMethod -Uri "https://localhost:7001/api/schedule/daily" `
    -Method Get `
    -Headers @{Authorization="Bearer $token"} `
    -SkipCertificateCheck
```

---

## Technology Stack

### Backend
- **Framework:** ASP.NET Core 8.0
- **Language:** C# 12
- **ORM:** Dapper 2.1.35
- **Authentication:** JWT Bearer Tokens
- **Password:** BCrypt.Net
- **Database:** SQL Server 2019+

### Frontend
- **Framework:** Angular 17
- **Language:** TypeScript
- **UI:** Angular Material
- **HTTP:** HttpClient with JWT interceptor
- **Styling:** SCSS

### Database
- **DBMS:** SQL Server / Azure SQL
- **Tables:** Users, Patients, Staff, Slots, BedAssignments

---

## Connection Configuration

### Backend to Database
Edit `Backend/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HDScheduler;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Frontend to Backend
Edit `Frontend/hd-scheduler-app/src/environments/environment.development.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api'
};
```

---

## Common Issues and Solutions

### Database Connection Failed
```powershell
# Check SQL Server is running
Get-Service MSSQLSERVER
Start-Service MSSQLSERVER
```

### Port Already in Use
```powershell
# Backend: Change port in launchSettings.json
# Frontend: Run on different port
ng serve --port 4201
```

### Angular CLI Not Found
```powershell
npm install -g @angular/cli@17
```

### CORS Errors
- Verify frontend URL is in `Backend/Program.cs` CORS policy
- Default allows `http://localhost:4200`

---

## Deployment Checklist

### Before Production
- [ ] Change all default passwords
- [ ] Generate new JWT secret key (32+ characters)
- [ ] Update connection strings to use Azure SQL
- [ ] Enable HTTPS only
- [ ] Configure proper CORS origins
- [ ] Set up Azure Key Vault for secrets
- [ ] Enable Application Insights
- [ ] Configure automated backups
- [ ] Test all user roles
- [ ] Perform security audit

---

## Support Files Reference

| File | Purpose |
|------|---------|
| `setup.ps1` | Interactive project setup wizard |
| `start.ps1` | Start all services at once |
| `README.md` | This file - complete guide |
| `Backend/README.md` | Backend-specific documentation |
| `Frontend/README.md` | Frontend setup instructions |
| `Database/README.md` | Database setup guide |
| `HD_Scheduler_Technical_Specification.md` | Full technical specification |

---

## Next Steps

1. **Run Setup** - Use `setup.ps1` for automated setup
2. **Verify Backend** - Test API at https://localhost:7001/swagger
3. **Create Angular App** - Follow Frontend/README.md
4. **Generate Components** - Create UI components as listed above
5. **Implement UI** - Build login and dashboard pages
6. **Test End-to-End** - Verify complete workflow
7. **Deploy** - Follow deployment guide for Azure

---

## Quick Command Reference

```powershell
# Setup everything
.\setup.ps1

# Start all services
.\start.ps1

# Backend only
cd Backend
dotnet run

# Frontend only
cd Frontend/hd-scheduler-app
ng serve

# Database setup
sqlcmd -S localhost -d HDScheduler -i Database\01_CreateSchema.sql
sqlcmd -S localhost -d HDScheduler -i Database\02_SeedData.sql

# Generate Angular components
ng generate component features/auth/login
```

---

**You're ready to start! Run `.\setup.ps1` to begin.**

*For detailed instructions, see README.md in the project root.*
