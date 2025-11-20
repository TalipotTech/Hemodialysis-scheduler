# HD Scheduler Project - Documentation Index

## 🚀 START HERE

**New to the project?** → Read [BUILD_GUIDE.md](BUILD_GUIDE.md)

**Want a quick overview?** → Read [QUICKSTART.md](QUICKSTART.md)

**Need detailed setup?** → Read [README.md](README.md)

---

## 📚 Documentation Structure

### 1. Quick Start Guides

| File | Purpose | Read This If... |
|------|---------|-----------------|
| [BUILD_GUIDE.md](BUILD_GUIDE.md) | Step-by-step build instructions | You're setting up for the first time |
| [QUICKSTART.md](QUICKSTART.md) | Quick reference guide | You need a fast overview |
| [PROJECT_STATUS.md](PROJECT_STATUS.md) | What's done and what's pending | You want to know project status |

### 2. Detailed Documentation

| File | Purpose | Read This If... |
|------|---------|-----------------|
| [README.md](README.md) | Complete setup and deployment guide | You need comprehensive instructions |
| [HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md) | Full technical specification | You need architecture details |

### 3. Component-Specific Guides

| File | Purpose | Read This If... |
|------|---------|-----------------|
| [Backend/README.md](Backend/README.md) | Backend API documentation | You're working on the API |
| [Frontend/README.md](Frontend/README.md) | Frontend setup guide | You're building the UI |
| [Database/README.md](Database/README.md) | Database setup instructions | You're managing the database |

### 4. Automation Scripts

| File | Purpose | When to Use |
|------|---------|-------------|
| [setup.ps1](setup.ps1) | Interactive setup wizard | First-time project setup |
| [start.ps1](start.ps1) | Start all services | Daily development startup |

---

## 🎯 Recommended Reading Order

### For Developers Starting Fresh

1. **[BUILD_GUIDE.md](BUILD_GUIDE.md)** - Start here for step-by-step setup
2. **[Backend/README.md](Backend/README.md)** - Understand the API
3. **[Database/README.md](Database/README.md)** - Set up the database
4. **[Frontend/README.md](Frontend/README.md)** - Build the UI

### For Project Managers

1. **[PROJECT_STATUS.md](PROJECT_STATUS.md)** - Current project status
2. **[HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md)** - Full technical spec
3. **[QUICKSTART.md](QUICKSTART.md)** - Overview and features

### For Quick Setup

1. Run `.\setup.ps1`
2. Read **[QUICKSTART.md](QUICKSTART.md)**
3. Run `.\start.ps1`

---

## 📂 Project Structure Overview

```
HD_Project/
│
├── 📄 BUILD_GUIDE.md              ⭐ START HERE - Step-by-step build instructions
├── 📄 QUICKSTART.md                Quick reference and overview
├── 📄 README.md                    Complete setup and deployment guide
├── 📄 PROJECT_STATUS.md            Current status and completion tracking
├── 📄 DOCUMENTATION_INDEX.md       This file - navigation guide
├── 📄 HD_Scheduler_Technical_Specification.md  Full technical specification
│
├── ⚙️ setup.ps1                   Interactive setup script
├── ⚙️ start.ps1                   Start all services script
│
├── 📁 Backend/                     ASP.NET Core 8.0 Web API
│   ├── 📄 README.md               Backend documentation
│   ├── Controllers/                API endpoints
│   ├── Services/                   Business logic
│   ├── Repositories/               Data access (Dapper)
│   ├── Models/                     Domain models
│   ├── DTOs/                       Data transfer objects
│   ├── Data/                       Database context
│   └── *.csproj                    Project file with packages
│
├── 📁 Database/                    SQL Server database
│   ├── 📄 README.md               Database documentation
│   ├── 01_CreateSchema.sql        Create all tables
│   ├── 02_SeedData.sql            Insert default data
│   └── PasswordHashGenerator.cs   BCrypt utility
│
├── 📁 Frontend/                    Angular 17 application
│   ├── 📄 README.md               Frontend documentation
│   └── hd-scheduler-app/
│       └── src/
│           ├── app/
│           │   ├── core/          Services, guards, models
│           │   ├── features/      Feature components
│           │   └── shared/        Shared components
│           └── environments/      Environment configs
│
└── 📁 Documentation/               Additional docs (if any)
```

---

## 🔍 Find Information Quickly

### Setup & Installation

**Q: How do I set up the project from scratch?**  
→ [BUILD_GUIDE.md](BUILD_GUIDE.md) - Sections "Build & Run"

**Q: What are the prerequisites?**  
→ [README.md](README.md) - Section "Prerequisites"

**Q: How do I create the database?**  
→ [Database/README.md](Database/README.md) - Section "Setup Instructions"

### Development

**Q: How do I add a new API endpoint?**  
→ [Backend/README.md](Backend/README.md) - Section "API Endpoints"

**Q: How do I create a new Angular component?**  
→ [Frontend/README.md](Frontend/README.md) - Section "Generate Components"

**Q: Where are the TypeScript models?**  
→ `Frontend/hd-scheduler-app/src/app/core/models/`

### Configuration

**Q: How do I change the database connection string?**  
→ [Backend/README.md](Backend/README.md) - Section "Configuration"

**Q: How do I update the API URL for production?**  
→ [Frontend/README.md](Frontend/README.md) - Section "Configuration"

**Q: What are the default login credentials?**  
→ [QUICKSTART.md](QUICKSTART.md) - Section "Default Credentials"

### Troubleshooting

**Q: Database connection errors**  
→ [README.md](README.md) - Section "Troubleshooting"

**Q: CORS errors in the browser**  
→ [BUILD_GUIDE.md](BUILD_GUIDE.md) - Section "Troubleshooting"

**Q: Port already in use**  
→ [BUILD_GUIDE.md](BUILD_GUIDE.md) - Section "Troubleshooting"

### Architecture

**Q: What technology stack is used?**  
→ [HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md) - Section 4.1

**Q: What is the database schema?**  
→ [HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md) - Appendix A

**Q: How does authentication work?**  
→ [Backend/README.md](Backend/README.md) - Section "Authentication"

### Deployment

**Q: How do I deploy to Azure?**  
→ [README.md](README.md) - Section "Deployment"

**Q: What are the Azure costs?**  
→ [HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md) - Section 7

---

## ✅ Quick Checklists

### First-Time Setup Checklist
- [ ] Read [BUILD_GUIDE.md](BUILD_GUIDE.md)
- [ ] Install prerequisites (.NET 8, Node.js, SQL Server)
- [ ] Run `.\setup.ps1`
- [ ] Verify database creation
- [ ] Test backend API at https://localhost:7001/swagger
- [ ] Create Angular app
- [ ] Run `.\start.ps1`
- [ ] Test login at http://localhost:4200

### Daily Development Checklist
- [ ] Start SQL Server
- [ ] Run `.\start.ps1` or start services manually
- [ ] Verify backend at https://localhost:7001
- [ ] Verify frontend at http://localhost:4200
- [ ] Make changes and test
- [ ] Commit your work

### Pre-Deployment Checklist
- [ ] Review [PROJECT_STATUS.md](PROJECT_STATUS.md)
- [ ] Complete all pending features
- [ ] Update connection strings for production
- [ ] Change default passwords
- [ ] Test all user roles
- [ ] Run security audit
- [ ] Follow deployment guide in [README.md](README.md)

---

## 🎓 Learning Path

### Week 1: Foundation
1. Read [BUILD_GUIDE.md](BUILD_GUIDE.md)
2. Set up development environment
3. Explore backend API via Swagger
4. Test database queries in SSMS
5. Understand core services in Angular

### Week 2: Backend Development
1. Study [Backend/README.md](Backend/README.md)
2. Review controller implementations
3. Understand Dapper repository pattern
4. Learn JWT authentication flow
5. Test API endpoints with Postman/curl

### Week 3: Frontend Development
1. Study [Frontend/README.md](Frontend/README.md)
2. Generate components
3. Implement login page
4. Build dashboards
5. Create forms

### Week 4: Integration & Testing
1. Connect frontend to backend
2. Test authentication flow
3. Implement CRUD operations
4. Test different user roles
5. Polish UI/UX

---

## 📞 Support & Resources

### Internal Documentation
- Technical questions → [HD_Scheduler_Technical_Specification.md](HD_Scheduler_Technical_Specification.md)
- Setup issues → [README.md](README.md) Troubleshooting section
- Quick answers → [QUICKSTART.md](QUICKSTART.md)

### External Resources
- **ASP.NET Core:** https://docs.microsoft.com/aspnet/core
- **Angular:** https://angular.io/docs
- **Dapper:** https://github.com/DapperLib/Dapper
- **Angular Material:** https://material.angular.io
- **JWT:** https://jwt.io/introduction

---

## 🔄 Document Updates

This documentation is organized for easy navigation. If you add new files or sections:

1. Update this index file
2. Add links in relevant guides
3. Update the project structure diagram
4. Keep the quick reference sections current

---

## 📊 Documentation Status

| Document | Status | Last Updated | Completeness |
|----------|--------|--------------|--------------|
| BUILD_GUIDE.md | ✅ Complete | Current | 100% |
| QUICKSTART.md | ✅ Complete | Current | 100% |
| README.md | ✅ Complete | Current | 100% |
| PROJECT_STATUS.md | ✅ Complete | Current | 100% |
| Technical Specification | ✅ Complete | Current | 100% |
| Backend README | ✅ Complete | Current | 100% |
| Frontend README | ✅ Complete | Current | 100% |
| Database README | ✅ Complete | Current | 100% |

---

## 🎯 Quick Start Commands

```powershell
# Full automated setup
.\setup.ps1

# Start all services
.\start.ps1

# Manual backend start
cd Backend; dotnet run

# Manual frontend start
cd Frontend\hd-scheduler-app; ng serve

# Create database
sqlcmd -S localhost -d HDScheduler -i Database\01_CreateSchema.sql
sqlcmd -S localhost -d HDScheduler -i Database\02_SeedData.sql

# Test API
curl -X POST https://localhost:7001/api/auth/login `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

**Ready to build? Start with [BUILD_GUIDE.md](BUILD_GUIDE.md)!** 🚀
