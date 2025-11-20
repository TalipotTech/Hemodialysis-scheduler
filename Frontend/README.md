# HD Scheduler Angular Frontend

## Quick Setup Commands

### Prerequisites
```powershell
# Verify Node.js and npm are installed
node --version  # Should be 18.x or higher
npm --version   # Should be 9.x or higher

# Install Angular CLI globally
npm install -g @angular/cli@17
```

### Create Angular Project
```powershell
# Navigate to Frontend directory
cd Frontend

# Create new Angular application
ng new hd-scheduler-app --routing --style=scss --skip-git

# Navigate into the project
cd hd-scheduler-app

# Install Angular Material
ng add @angular/material

# When prompted, choose:
# - Choose a prebuilt theme: Indigo/Pink
# - Set up global Angular Material typography styles: Yes
# - Include Angular Material animations: Yes

# Install additional dependencies
npm install @auth0/angular-jwt
npm install @ngrx/store @ngrx/effects @ngrx/store-devtools
```

### Project Structure
After running the setup commands above, you'll need to create this structure:

```
src/
├── app/
│   ├── core/                    # Core module (singleton services)
│   │   ├── guards/
│   │   │   └── auth.guard.ts
│   │   ├── interceptors/
│   │   │   └── jwt.interceptor.ts
│   │   ├── models/
│   │   │   ├── user.model.ts
│   │   │   ├── patient.model.ts
│   │   │   └── schedule.model.ts
│   │   └── services/
│   │       ├── auth.service.ts
│   │       ├── patient.service.ts
│   │       └── schedule.service.ts
│   ├── shared/                  # Shared module (reusable components)
│   │   ├── components/
│   │   └── pipes/
│   ├── features/                # Feature modules
│   │   ├── auth/
│   │   │   ├── login/
│   │   │   └── auth.module.ts
│   │   ├── dashboard/
│   │   │   ├── admin-dashboard/
│   │   │   ├── hod-dashboard/
│   │   │   ├── staff-entry/
│   │   │   ├── technician-view/
│   │   │   └── dashboard.module.ts
│   │   ├── patients/
│   │   │   ├── patient-list/
│   │   │   ├── patient-form/
│   │   │   └── patients.module.ts
│   │   └── schedule/
│   │       ├── schedule-grid/
│   │       ├── bed-assignment/
│   │       └── schedule.module.ts
│   ├── app.component.ts
│   ├── app.component.html
│   ├── app.component.scss
│   ├── app.config.ts
│   └── app.routes.ts
├── environments/
│   ├── environment.ts
│   └── environment.development.ts
└── index.html
```

### Generate Components (Run after project creation)
```powershell
# Core services and guards
ng generate service core/services/auth --skip-tests
ng generate service core/services/patient --skip-tests
ng generate service core/services/schedule --skip-tests
ng generate guard core/guards/auth --skip-tests

# Auth feature
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

### Run Development Server
```powershell
ng serve

# Or with specific port
ng serve --port 4200 --open
```

### Build for Production
```powershell
ng build --configuration production
```

## Manual Setup (If you prefer manual file creation)

If you want to create files manually, the essential files are provided in the repository. Copy them to the appropriate locations after running `ng new`.

## Configuration

### Update environment files

**src/environments/environment.development.ts:**
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api'
};
```

**src/environments/environment.ts:**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-url.azurewebsites.net/api'
};
```

## Features

- ✅ JWT Authentication
- ✅ Role-based routing and guards
- ✅ Responsive Material Design UI
- ✅ Patient management
- ✅ Schedule visualization
- ✅ Bed assignment
- ✅ Real-time status updates

## Default Login Credentials

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | Admin@123 |
| HOD | hod | Hod@123 |
| Doctor | doctor1 | Doctor@123 |
| Nurse | nurse1 | Nurse@123 |
| Technician | tech1 | Tech@123 |

## Development Server

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any source files.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Troubleshooting

### Port Already in Use
```powershell
ng serve --port 4201
```

### Angular CLI Not Found
```powershell
npm install -g @angular/cli@17
```

### Module Not Found Errors
```powershell
npm install
```

## Next Steps

1. Run the setup commands above
2. Copy the provided source files to appropriate locations
3. Update environment files with your API URL
4. Run `ng serve`
5. Navigate to http://localhost:4200
6. Login with default credentials

For detailed implementation, see the source files in the repository.
