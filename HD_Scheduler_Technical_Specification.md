# Hemodialysis Scheduler System - Technical Specification

## Version 1.0

---

## Table of Contents
1. [Executive Summary](#1-executive-summary)
2. [System Overview](#2-system-overview)
3. [Functional Requirements](#3-functional-requirements)
4. [Technical Architecture](#4-technical-architecture)
5. [Implementation Plan](#5-implementation-plan)
6. [Security Considerations](#6-security-considerations)
7. [Project Cost Summary](#7-project-cost-summary)
8. [Conclusion](#8-conclusion)

---

## 1. Executive Summary

The Hemodialysis Scheduler System is a comprehensive web-based solution designed to streamline the management of dialysis patient schedules and staff assignments in hospital settings. This system replaces manual scheduling processes with an automated, role-based platform that ensures efficient bed allocation, patient tracking, and staff coordination across four daily dialysis slots.

The system accommodates 10 dialysis beds per slot with automatic bed release upon patient discharge, ensuring optimal resource utilization and minimal administrative overhead.

---

## 2. System Overview

### 2.1 Core Objectives

- Automate dialysis slot scheduling for improved efficiency
- Provide role-based access control with secure login system
- Enable real-time bed availability tracking
- Maintain comprehensive patient treatment history
- Facilitate staff assignment and workload management

### 2.2 Key Features

- Fixed slot scheduling with 4 daily shifts
- 10-bed capacity per slot
- Automatic bed release on patient discharge
- Role-based access with secure login system
- Default accounts for MVP with planned authentication upgrade
- Comprehensive patient data management

---

## 3. Functional Requirements

### 3.1 Dialysis Slot Scheduling Module

| Slot | Timing | Capacity |
|------|--------|----------|
| Slot 1 | Morning (6:00 AM - 10:00 AM) | 10 beds |
| Slot 2 | Afternoon (11:00 AM - 3:00 PM) | 10 beds |
| Slot 3 | Evening (4:00 PM - 8:00 PM) | 10 beds |
| Slot 4 | Night (9:00 PM - 1:00 AM) | 10 beds |

#### 3.1.1 Schedule Management Features

- Daily schedule grid view with real-time bed occupancy status
- Color-coded bed status indicators (Available/Occupied/Reserved)
- Quick patient assignment interface with drag-and-drop capability
- Weekly and monthly schedule views for planning
- Export functionality for schedule reports

### 3.2 Patient Management Module

#### 3.2.1 Patient Data Fields

| Field Name | Data Type | Description |
|------------|-----------|-------------|
| PatientID | String | Unique patient identifier |
| Name | String | Patient full name |
| Age | Integer | Patient age in years |
| DryWeight | Decimal | Target weight after dialysis (kg) |
| HDStartDate | Date | Date dialysis treatment started |
| HDCycle | String | Dialysis cycle frequency |
| WeightGain | Decimal | Weight gain between sessions |
| DialyserType | Enum | High flux (HI) or Low flux (LO) |
| DialyserReuseCount | Integer | Number of dialyser reuses |
| BloodTubingReuse | Boolean | Blood tubing reuse status |
| HeparinDose | Decimal | Heparin dosage |
| Symptoms | Text | Patient symptoms |
| BloodTestDone | Boolean | Blood test completion status |
| BloodPressure | String | Blood pressure reading |
| SlotID | Integer | Assigned slot (1-4) |
| BedNumber | Integer | Assigned bed (1-10) |
| CreatedByStaffName | String | Staff who created record |
| CreatedByStaffRole | String | Role of creator |
| IsDischarged | Boolean | Flag to release bed assignment |

### 3.3 Staff Management Module

#### 3.3.1 Role-Based Access Control

| Role | Permissions | Route Access |
|------|------------|--------------|
| Admin | Full system access | /admin-dashboard |
| Doctor | Add/edit patients, view schedules | /staff-entry |
| Nurse | Add/edit patients, view schedules | /staff-entry |
| Technician | View-only access | /technician-view |
| HOD | Staff & schedule management | /hod-dashboard |

### 3.4 Authentication & User Management

#### 3.4.1 MVP Login System

For the MVP phase, the system will implement a basic but secure login system with pre-configured default accounts:

| Role | Default Username | Default Password | Access Level |
|------|------------------|------------------|--------------|
| Admin | admin | Admin@123 | Full system |
| HOD | hod | Hod@123 | Management |
| Doctor | doctor1 | Doctor@123 | Patient management |
| Nurse | nurse1 | Nurse@123 | Patient management |
| Technician | tech1 | Tech@123 | View-only |

#### 3.4.2 MVP Authentication Features

- Simple login form with username and password
- JWT token-based session management
- Role-based route guards in Angular
- Session timeout after 30 minutes of inactivity
- Basic password hashing (BCrypt)
- Logout functionality with token invalidation

#### 3.4.3 Post-MVP Authentication Roadmap

After successful MVP deployment and validation, the following authentication enhancements will be implemented:

- **Azure Active Directory Integration:** Enterprise SSO for hospital staff
- **Multi-Factor Authentication (MFA):** Enhanced security with SMS/App-based verification
- **User Management Portal:** Admin interface for creating/managing user accounts
- **Password Policies:** Complexity requirements, expiration, and history
- **Audit Trail:** Comprehensive logging of all authentication events
- **Role Management:** Dynamic role assignment and custom permissions
- **Account Recovery:** Self-service password reset and account unlock

---

## 4. Technical Architecture

### 4.1 Technology Stack

#### 4.1.1 Frontend
- **Framework:** Angular 17+ with TypeScript
- **UI Library:** Angular Material Design Components
- **State Management:** NgRx for complex state management
- **Routing:** Angular Router with role-based route guards
- **Forms:** Reactive Forms with validation

#### 4.1.2 Backend
- **Framework:** ASP.NET Core 8.0 Web API
- **Language:** C# 12
- **ORM:** Dapper (lightweight micro-ORM for high performance)
- **Data Access Pattern:** Repository pattern with Dapper for clean separation of concerns
- **API Documentation:** Swagger/OpenAPI
- **Authentication:** JWT Bearer tokens with Microsoft.AspNetCore.Authentication.JwtBearer
- **Password Hashing:** BCrypt.Net for secure password storage
- **Real-time Updates:** SignalR for live bed status updates

#### 4.1.3 Database
- **DBMS:** Azure SQL Database (PaaS)
- **Region:** South India or Southeast Asia
- **Backup:** Automated daily backups with point-in-time restore
- **High Availability:** Built-in 99.99% SLA with zone redundancy

#### 4.1.4 Why Dapper for This Project

Dapper has been selected as the ORM for this project due to its specific advantages for healthcare scheduling systems:

- **Performance:** Near-raw ADO.NET performance crucial for real-time bed availability checks
- **Control:** Full control over SQL queries for complex scheduling logic and reporting
- **Simplicity:** Lightweight with minimal overhead, reducing application complexity
- **Stored Procedures:** Excellent support for stored procedures used in slot allocation algorithms
- **Team Expertise:** Easier learning curve for teams familiar with SQL

### 4.2 Database Schema

#### 4.2.1 Core Tables

**Users Table**
- Authentication and authorization data for system users with role assignments

**Patients Table**
- Primary storage for patient information with comprehensive medical data fields

**Staff Table**
- Contains staff member profiles with role assignments and shift allocations

**Slots Table**
- Defines the four daily dialysis slots with timing and capacity constraints

**BedAssignments Table**
- Tracks current and historical bed assignments with patient-slot mappings

### 4.3 API Endpoints

#### 4.3.1 Patient Management
- `GET /api/patients` - Retrieve all patients
- `GET /api/patients/{id}` - Get specific patient details
- `POST /api/patients` - Create new patient record
- `PUT /api/patients/{id}` - Update patient information
- `DELETE /api/patients/{id}` - Soft delete patient record

#### 4.3.2 Schedule Management
- `GET /api/schedule/daily` - Get daily schedule grid
- `GET /api/schedule/slot/{slotId}` - Get specific slot details
- `POST /api/schedule/assign` - Assign patient to bed
- `PUT /api/schedule/discharge/{patientId}` - Mark patient as discharged
- `GET /api/schedule/availability` - Check bed availability

#### 4.3.3 Authentication
- `POST /api/auth/login` - User authentication and token generation
- `POST /api/auth/logout` - Session termination
- `POST /api/auth/refresh` - Token refresh for active sessions
- `GET /api/auth/validate` - Validate current session
- `GET /api/auth/user-info` - Get current user details and permissions

---

## 5. Implementation Plan

### 5.1 Development Phases

| Phase | Duration | Key Deliverables | Success Criteria |
|-------|----------|------------------|------------------|
| Phase 1 | 2 weeks | Database design, API setup | Schema deployed, APIs tested |
| Phase 2 | 3 weeks | Core modules development | CRUD operations functional |
| Phase 3 | 2 weeks | UI implementation | All dashboards operational |
| Phase 4 | 1 week | Testing & deployment | System live on Azure |

### 5.2 Deployment Strategy

#### 5.2.1 Azure Infrastructure Configuration

All environments will be deployed within the existing Azure subscription and resource group:

- **Subscription ID:** 27a9f8ec-0b46-445b-80d1-a54d192bbf33
- **Resource Group:** EnsateBlogRG
- **App Service Plan:** EnsateBlogPlan-Linux
- **Region:** South India or Southeast Asia (based on latency requirements)

#### 5.2.2 Environment-Specific Configurations

| Environment | SQL Database Tier | App Service | Monthly Cost (Est.) |
|-------------|------------------|-------------|---------------------|
| **Development** | Basic (5 DTUs) | Free (F1) | ~$5 |
| **Staging** | Basic (5 DTUs) | Free (F1) | ~$5 |
| **Production** | S0 Standard (10 DTUs) | B1 Basic | ~$65 |

#### 5.2.3 Azure SQL Database Specifications

**Development & Staging (Basic Tier - 5 DTUs):**
- 2 GB maximum database size
- Point-in-time restore up to 7 days
- 99.99% availability SLA
- Cost: ~$5/month per database

**Production (Standard S0 - 10 DTUs):**
- 250 GB maximum database size
- Point-in-time restore up to 35 days
- 99.99% availability SLA
- Geo-redundant backups available
- Cost: ~$15/month

#### 5.2.4 Database Connection Strings

Connection strings for each environment (to be updated with actual server names):

- **Development:** `Server=hd-scheduler-dev.database.windows.net; Database=HDSchedulerDev;`
- **Staging:** `Server=hd-scheduler-staging.database.windows.net; Database=HDSchedulerStaging;`
- **Production:** `Server=hd-scheduler-prod.database.windows.net; Database=HDSchedulerProd;`

#### 5.2.5 CI/CD Pipeline Configuration

- **Source Control:** GitHub repository with protected branches
- **Build Pipeline:** GitHub Actions for automated builds
- **Deployment:** Azure Web App deployment slots for zero-downtime releases
- **Database Migrations:** DbUp or FluentMigrator for version-controlled schema changes
- **Monitoring:** Application Insights for performance and error tracking

#### 5.2.6 Cost Optimization Strategies

- **Auto-shutdown:** Configure dev/staging databases to pause during non-business hours
- **Reserved Capacity:** Consider 1-year reserved pricing for production (up to 33% savings)
- **Elastic Pools:** If scaling to multiple hospitals, use elastic pools for cost sharing
- **Azure Hybrid Benefit:** Utilize existing SQL Server licenses if available
- **Monitoring & Alerts:** Set up cost alerts to prevent budget overruns

---

## 6. Security Considerations

Given the critical nature of patient data, the system implements comprehensive security measures from the MVP stage:

- **Authentication:** Mandatory login with role-based access control
- **Password Security:** BCrypt hashing for all passwords, even default accounts
- **Session Management:** JWT tokens with automatic expiration
- **HTTPS encryption:** All data transmission secured with TLS 1.3
- **API rate limiting:** Prevent abuse and ensure fair usage
- **Input validation:** Comprehensive validation at both frontend and backend
- **SQL injection prevention:** Parameterized queries with Dapper's built-in SQL parameter handling
- **Audit logging:** Track all patient data modifications
- **Data encryption:** Sensitive patient data encrypted at rest
- **Backup strategy:** Daily automated backups with 30-day retention

---

## 7. Project Cost Summary

### 7.1 Monthly Operating Costs

| Service | Dev/Staging | Production | Total |
|---------|-------------|------------|-------|
| Azure SQL Database | $10 | $15 | $25 |
| App Service Plan | $0 | $50 | $50 |
| Application Insights | $0 | $0 | $0 |
| **Total Monthly Cost** | **$10** | **$65** | **$75** |

### 7.2 Cost Optimization Notes

- Development and staging environments use free/basic tiers to minimize costs during development
- Production uses minimal viable configuration suitable for a single hospital deployment
- Total monthly operational cost for all environments: approximately $75
- Costs can be further reduced by pausing dev/staging databases during non-business hours
- As usage grows, the system can be scaled up with predictable cost increases

---

## 8. Conclusion

The Hemodialysis Scheduler System represents a significant advancement in hospital dialysis management. By automating scheduling processes, implementing intelligent bed allocation, and providing role-based access through intuitive interfaces, the system will dramatically improve operational efficiency and patient care quality.

The modular architecture ensures scalability for future enhancements, while the cloud-based deployment strategy provides reliability and accessibility. With an estimated 8-week development timeline, this system will deliver immediate value to healthcare providers and establish a foundation for continued digital transformation in dialysis care management.

---

## Appendix A: Database Schema Details

### Users Table
```sql
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);
```

### Patients Table
```sql
CREATE TABLE Patients (
    PatientID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    DryWeight DECIMAL(5,2),
    HDStartDate DATE NOT NULL,
    HDCycle NVARCHAR(50),
    WeightGain DECIMAL(5,2),
    DialyserType NVARCHAR(2) CHECK (DialyserType IN ('HI', 'LO')),
    DialyserReuseCount INT DEFAULT 0,
    BloodTubingReuse BIT DEFAULT 0,
    HeparinDose DECIMAL(5,2),
    Symptoms NVARCHAR(MAX),
    BloodTestDone BIT DEFAULT 0,
    BloodPressure NVARCHAR(20),
    SlotID INT,
    BedNumber INT,
    CreatedByStaffName NVARCHAR(100),
    CreatedByStaffRole NVARCHAR(20),
    IsDischarged BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
```

### Staff Table
```sql
CREATE TABLE Staff (
    StaffID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    AssignedSlot INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
```

### Slots Table
```sql
CREATE TABLE Slots (
    SlotID INT PRIMARY KEY,
    SlotName NVARCHAR(50) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    BedCapacity INT DEFAULT 10
);

-- Insert default slots
INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, BedCapacity) VALUES
(1, 'Morning', '06:00:00', '10:00:00', 10),
(2, 'Afternoon', '11:00:00', '15:00:00', 10),
(3, 'Evening', '16:00:00', '20:00:00', 10),
(4, 'Night', '21:00:00', '01:00:00', 10);
```

### BedAssignments Table
```sql
CREATE TABLE BedAssignments (
    AssignmentID INT PRIMARY KEY IDENTITY(1,1),
    PatientID INT FOREIGN KEY REFERENCES Patients(PatientID),
    SlotID INT FOREIGN KEY REFERENCES Slots(SlotID),
    BedNumber INT NOT NULL,
    AssignmentDate DATE NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    DischargedAt DATETIME NULL
);
```

---

## Appendix B: Sample API Request/Response

### Login Request
```json
POST /api/auth/login
{
    "username": "doctor1",
    "password": "Doctor@123"
}
```

### Login Response
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
        "username": "doctor1",
        "role": "Doctor",
        "name": "Dr. Smith"
    },
    "expiresIn": 1800
}
```

### Create Patient Request
```json
POST /api/patients
Authorization: Bearer {token}
{
    "name": "John Doe",
    "age": 45,
    "dryWeight": 70.5,
    "hdStartDate": "2025-01-01",
    "dialyserType": "HI",
    "slotID": 1,
    "bedNumber": 5
}
```

### Schedule Response
```json
GET /api/schedule/daily
{
    "date": "2025-01-15",
    "slots": [
        {
            "slotID": 1,
            "slotName": "Morning",
            "beds": [
                {
                    "bedNumber": 1,
                    "status": "occupied",
                    "patient": {
                        "id": 101,
                        "name": "John Doe"
                    }
                },
                {
                    "bedNumber": 2,
                    "status": "available",
                    "patient": null
                }
            ]
        }
    ]
}
```

---

## Appendix C: Angular Route Configuration

```typescript
const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { 
    path: 'admin-dashboard', 
    component: AdminDashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'hod-dashboard', 
    component: HODDashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['HOD'] }
  },
  { 
    path: 'staff-entry', 
    component: StaffEntryComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Doctor', 'Nurse'] }
  },
  { 
    path: 'technician-view', 
    component: TechnicianViewComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Technician'] }
  }
];
```

---

## Appendix D: Development Environment Setup

### Prerequisites
- Node.js 18+ and npm
- .NET SDK 8.0
- SQL Server Management Studio
- Visual Studio Code / Visual Studio 2022
- Azure CLI
- Git

### Frontend Setup
```bash
# Clone repository
git clone https://github.com/yourorg/hd-scheduler-frontend.git
cd hd-scheduler-frontend

# Install dependencies
npm install

# Run development server
ng serve --open
```

### Backend Setup
```bash
# Clone repository
git clone https://github.com/yourorg/hd-scheduler-backend.git
cd hd-scheduler-backend

# Restore packages
dotnet restore

# Update database connection string in appsettings.Development.json
# Run database migrations
dotnet ef database update

# Run the API
dotnet run
```

### Azure Deployment Commands
```bash
# Login to Azure
az login

# Create SQL Database (Development)
az sql db create \
  --resource-group EnsateBlogRG \
  --server hd-scheduler-dev \
  --name HDSchedulerDev \
  --service-objective Basic \
  --zone-redundant false

# Deploy to App Service
az webapp deployment source config-zip \
  --resource-group EnsateBlogRG \
  --name hd-scheduler-app \
  --src deploy.zip
```

---

## Contact Information

For technical queries and support during development:
- **Project Manager:** [To be assigned]
- **Technical Lead:** [To be assigned]
- **Azure Administrator:** [To be assigned]

---

*This document serves as the complete technical specification for the Hemodialysis Scheduler System development.*
