# HD Scheduler Backend API

## Overview
ASP.NET Core 8.0 Web API for the Hemodialysis Scheduler System. Uses Dapper for data access, JWT for authentication, and follows repository pattern.

## Technology Stack
- **Framework:** ASP.NET Core 8.0
- **Language:** C# 12
- **ORM:** Dapper 2.1.35
- **Database:** SQL Server / Azure SQL Database
- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt.Net
- **API Documentation:** Swagger/OpenAPI

## Project Structure
```
Backend/
├── Controllers/          # API endpoints
│   ├── AuthController.cs
│   ├── PatientsController.cs
│   └── ScheduleController.cs
├── Data/                # Database context
│   └── DapperContext.cs
├── DTOs/                # Data transfer objects
│   └── ApiResponse.cs
├── Models/              # Domain models
│   ├── Patient.cs
│   ├── Schedule.cs
│   ├── Staff.cs
│   └── User.cs
├── Repositories/        # Data access layer
│   ├── IUserRepository.cs
│   ├── UserRepository.cs
│   ├── IPatientRepository.cs
│   ├── PatientRepository.cs
│   ├── IScheduleRepository.cs
│   └── ScheduleRepository.cs
├── Services/            # Business logic
│   ├── IAuthService.cs
│   └── AuthService.cs
├── appsettings.json     # Configuration
└── Program.cs           # Application startup
```

## Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ or Azure SQL Database
- Visual Studio 2022 or VS Code with C# extension

## Getting Started

### 1. Restore NuGet Packages
```powershell
cd Backend
dotnet restore
```

### 2. Update Connection String
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HDScheduler;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Setup Database
Run the SQL scripts in the Database folder:
```powershell
# Navigate to Database folder
cd ..\Database

# Create schema
sqlcmd -S localhost -d HDScheduler -i 01_CreateSchema.sql

# Insert seed data
sqlcmd -S localhost -d HDScheduler -i 02_SeedData.sql
```

### 4. Run the API
```powershell
cd ..\Backend
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001
- Swagger UI: https://localhost:7001/swagger

## Configuration

### JWT Settings
Configure in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123!@#",
    "Issuer": "HDSchedulerAPI",
    "Audience": "HDSchedulerClient",
    "ExpiryMinutes": 30
  }
}
```

**⚠️ Important:** Generate a strong secret key for production (minimum 32 characters).

### CORS Configuration
Current configuration allows Angular app at `http://localhost:4200`. Update in `Program.cs` for production.

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/validate` - Validate JWT token
- `GET /api/auth/user-info` - Get current user info (requires auth)

### Patients
- `GET /api/patients` - Get all patients (requires auth)
- `GET /api/patients/active` - Get active patients (requires auth)
- `GET /api/patients/{id}` - Get patient by ID (requires auth)
- `POST /api/patients` - Create patient (Admin/Doctor/Nurse)
- `PUT /api/patients/{id}` - Update patient (Admin/Doctor/Nurse)
- `DELETE /api/patients/{id}` - Delete patient (Admin only)

### Schedule
- `GET /api/schedule/daily` - Get daily schedule (requires auth)
- `GET /api/schedule/slot/{slotId}` - Get specific slot schedule (requires auth)
- `POST /api/schedule/assign` - Assign patient to bed (Admin/Doctor/Nurse)
- `PUT /api/schedule/discharge/{patientId}` - Discharge patient (Admin/Doctor/Nurse)
- `GET /api/schedule/availability` - Get bed availability (requires auth)

## Authentication

### Login
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "username": "admin",
      "role": "Admin",
      "name": "admin"
    },
    "expiresIn": 1800
  }
}
```

### Using JWT Token
Include the token in Authorization header:
```bash
curl -X GET https://localhost:7001/api/patients \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Admin | Full access to all endpoints |
| HOD | View all, manage staff and schedules |
| Doctor | Create/update patients, manage schedules |
| Nurse | Create/update patients, manage schedules |
| Technician | View-only access |

## Development

### Build
```powershell
dotnet build
```

### Run Tests (if implemented)
```powershell
dotnet test
```

### Publish
```powershell
dotnet publish -c Release -o ./publish
```

## Deployment

### Azure App Service
1. Create App Service in Azure Portal
2. Configure connection string in Azure App Service settings
3. Deploy using:
```powershell
# Using Azure CLI
az webapp deployment source config-zip \
  --resource-group EnsateBlogRG \
  --name hd-scheduler-api \
  --src ./publish.zip
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HDScheduler.API.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HDScheduler.API.dll"]
```

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure database exists
- For Azure: Check firewall rules

### JWT Token Issues
- Verify SecretKey length (minimum 32 characters)
- Check token expiration
- Ensure Issuer and Audience match

### CORS Errors
- Update allowed origins in Program.cs
- Verify Angular app URL

## Security Best Practices

1. **Production JWT Secret:** Generate a cryptographically secure secret key
2. **HTTPS Only:** Use HTTPS in production
3. **Password Policy:** Enforce strong passwords
4. **Default Passwords:** Change all default passwords immediately
5. **Connection Strings:** Use Azure Key Vault or environment variables for secrets
6. **API Rate Limiting:** Implement rate limiting for production
7. **Input Validation:** Always validate and sanitize input
8. **SQL Injection:** Dapper uses parameterized queries by default

## License
Proprietary - All rights reserved

## Support
For issues or questions, contact the development team.
