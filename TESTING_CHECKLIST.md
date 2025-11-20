# HD Scheduler Testing Checklist

## Test Date: November 11, 2025

## ✅ System Status

### Backend Status
- **URL:** https://localhost:7001 (HTTPS) / http://localhost:5001 (HTTP)
- **Swagger UI:** https://localhost:7001/swagger
- **Status:** ✅ Running
- **Database:** ✅ HDScheduler with 9 tables (including AuditLogs)

### Frontend Status  
- **URL:** http://localhost:4200
- **Status:** ✅ Running
- **Build:** ✅ Successful (all lazy chunks loaded)

---

## 🧪 Test Plan

### Phase 1: Authentication Testing

#### Test 1.1: Login as Admin
**Steps:**
1. Navigate to http://localhost:4200
2. Should redirect to `/login`
3. Enter credentials:
   - Username: `admin`
   - Password: `Admin@123`
4. Click Login

**Expected Results:**
- ✅ Token stored in localStorage
- ✅ Redirect to `/admin` dashboard
- ✅ See "Administrator Control Panel" welcome message
- ✅ See 7 action cards (Patient, Schedule, User Mgmt, Staff Mgmt, Settings, Reports, Audit Logs)

---

### Phase 2: User Management Testing (FULLY IMPLEMENTED)

#### Test 2.1: Navigate to User Management
**Steps:**
1. From admin dashboard, click "User Management" card
2. Should navigate to `/admin/user-management`

**Expected Results:**
- ✅ See "User Management" header with back button
- ✅ See "Create User" button
- ✅ See search box and role filter
- ✅ See table with existing users (admin, hod, doctor1, nurse1, tech1)
- ✅ Table shows: Username, Role (colored badges), Status, Created Date, Last Login, Actions

#### Test 2.2: Search and Filter Users
**Steps:**
1. Type "admin" in search box
2. Should see filtered results
3. Clear search
4. Select "Doctor" from role filter dropdown
5. Should see only doctors

**Expected Results:**
- ✅ Search filters by username
- ✅ Role filter shows only selected role
- ✅ "All" shows all users again

#### Test 2.3: Create New User
**Steps:**
1. Click "Create User" button
2. Dialog opens with form
3. Enter:
   - Username: `testuser`
   - Password: `Test@123`
   - Role: `Nurse`
4. Click "Create"

**Expected Results:**
- ✅ Form validation works (required fields)
- ✅ Password minimum length validation (6 chars)
- ✅ Success snackbar appears
- ✅ Dialog closes
- ✅ Table refreshes with new user
- ✅ Audit log created in backend

**Backend Verification:**
- Check Swagger: GET `/api/usermanagement` - should see new user
- Check Swagger: GET `/api/auditlogs` - should see CREATE action

#### Test 2.4: Edit User
**Steps:**
1. Find a user in the table
2. Click edit icon (pencil)
3. Change username or role
4. Click "Update"

**Expected Results:**
- ✅ Edit dialog opens with current values
- ✅ Password field NOT shown (can't change in edit)
- ✅ Form validation works
- ✅ Success snackbar appears
- ✅ Table refreshes with updated data
- ✅ Audit log created with old and new values

#### Test 2.5: Reset Password
**Steps:**
1. Find a user in the table
2. Click lock reset icon
3. Enter new password: `NewPass@123`
4. Confirm password: `NewPass@123`
5. Click "Reset Password"

**Expected Results:**
- ✅ Password reset dialog opens
- ✅ Confirm password validation works
- ✅ Mismatch error shown if passwords don't match
- ✅ Success snackbar appears
- ✅ Audit log created

**Test Password Reset:**
- Logout
- Login with that user using new password
- Should work

#### Test 2.6: Toggle User Status (Enable/Disable)
**Steps:**
1. Find an active user
2. Click block/enable icon
3. Confirm action
4. Check status badge changes

**Expected Results:**
- ✅ Confirmation dialog appears
- ✅ Status badge changes from "Active" (green) to "Inactive" (gray)
- ✅ Icon changes from block to check_circle
- ✅ Success snackbar appears
- ✅ Audit log created

**Test Disabled User:**
- Logout
- Try to login with disabled user
- Should fail or show appropriate message

#### Test 2.7: Delete User
**Steps:**
1. Find a non-admin user (preferably the test user created earlier)
2. Click delete icon (red trash)
3. Confirm deletion

**Expected Results:**
- ✅ Confirmation dialog: "Are you sure you want to delete..."
- ✅ User removed from table
- ✅ Success snackbar appears
- ✅ Audit log created

**Negative Test - Try to Delete Self:**
- Try to delete the currently logged in admin user
- Should show error: "Cannot delete yourself" or prevent action

#### Test 2.8: Responsive Design
**Steps:**
1. Resize browser window to mobile size
2. Check layout

**Expected Results:**
- ✅ Filters stack vertically
- ✅ Table scrolls horizontally if needed
- ✅ Buttons stack appropriately

---

### Phase 3: Backend API Testing with Swagger

#### Test 3.1: Test User Management API
**Steps:**
1. Navigate to https://localhost:7001/swagger
2. Click "Authorize" button
3. Login to get token:
   - POST `/api/auth/login`
   - Body: `{"username": "admin", "password": "Admin@123"}`
   - Copy the `data` value (JWT token)
4. Click "Authorize" again
5. Enter: `Bearer <your-token-here>`
6. Click "Authorize" and close

**Test Endpoints:**
- ✅ GET `/api/usermanagement` - Get all users
- ✅ GET `/api/usermanagement/{id}` - Get user by ID (try ID 1)
- ✅ POST `/api/usermanagement` - Create user
- ✅ PUT `/api/usermanagement/{id}` - Update user
- ✅ POST `/api/usermanagement/{id}/reset-password` - Reset password
- ✅ POST `/api/usermanagement/{id}/toggle-status` - Toggle status
- ✅ DELETE `/api/usermanagement/{id}` - Delete user

#### Test 3.2: Test Staff Management API
**Test Endpoints:**
- ✅ GET `/api/staffmanagement` - Get all staff
- ✅ GET `/api/staffmanagement/active` - Get active staff
- ✅ GET `/api/staffmanagement/role/Doctor` - Get by role
- ✅ POST `/api/staffmanagement` - Create staff
- ✅ POST `/api/staffmanagement/{id}/assign-slot` - Assign to slot

#### Test 3.3: Test System Settings API
**Test Endpoints:**
- ✅ GET `/api/systemsettings/slots` - Get all slots
- ✅ GET `/api/systemsettings/beds/capacity` - Get bed capacity
- ✅ GET `/api/systemsettings/parameters` - Get system parameters

#### Test 3.4: Test Reports API
**Test Endpoints:**
- ✅ GET `/api/reports/patient-volume` - Patient volume report
- ✅ GET `/api/reports/occupancy-rates` - Occupancy rates
- ✅ GET `/api/reports/treatment-completion` - Treatment completion
- ✅ GET `/api/reports/staff-performance` - Staff performance
- ✅ GET `/api/reports/monthly-summary?year=2025&month=11` - Monthly summary

#### Test 3.5: Test Audit Logs API
**Test Endpoints:**
- ✅ GET `/api/auditlogs?page=1&pageSize=50` - Get all logs
- ✅ GET `/api/auditlogs/user/1` - Get logs by user ID
- ✅ GET `/api/auditlogs/login-history?days=30` - Login history
- ✅ GET `/api/auditlogs/statistics?days=30` - Audit statistics
- ✅ GET `/api/auditlogs/actions/CREATE` - Get logs by action

**Verify Audit Trail:**
After creating/updating/deleting users, check audit logs to ensure all actions are tracked

---

### Phase 4: Navigation & Placeholder Component Testing

#### Test 4.1: Navigate to Staff Management
**Steps:**
1. From admin dashboard, click "Staff Management" card
2. Should navigate to `/admin/staff-management`

**Expected Results:**
- ✅ See "Staff Management" header
- ✅ See back button
- ✅ See placeholder content with feature list
- ✅ Backend service is ready (can test in console)

#### Test 4.2: Navigate to System Settings
**Steps:**
1. From admin dashboard, click "System Settings" card
2. Should navigate to `/admin/system-settings`

**Expected Results:**
- ✅ See "System Settings" header with tabs
- ✅ See 3 tabs: Slot Configuration, Bed Capacity, System Parameters
- ✅ See placeholder content in each tab
- ✅ Back button works

#### Test 4.3: Navigate to Reports
**Steps:**
1. From admin dashboard, click "Reports & Analytics" card
2. Should navigate to `/admin/reports`

**Expected Results:**
- ✅ See "Reports & Analytics" header
- ✅ See 5 tabs: Patient Volume, Occupancy Rates, Treatment Completion, Staff Performance, Monthly Summary
- ✅ See placeholder content in each tab
- ✅ Back button works

#### Test 4.4: Navigate to Audit Logs
**Steps:**
1. From admin dashboard, click "Audit Logs" card
2. Should navigate to `/admin/audit-logs`

**Expected Results:**
- ✅ See "Audit Logs" header
- ✅ See 3 tabs: All Logs, Login History, Statistics
- ✅ See placeholder content in each tab
- ✅ Back button works

---

### Phase 5: Existing Features Testing

#### Test 5.1: Patient Management
**Steps:**
1. From admin dashboard, click "Patient Management"
2. Should navigate to `/patients`
3. Test patient CRUD operations

**Expected Results:**
- ✅ Patient list loads
- ✅ Can create new patient
- ✅ Can edit patient
- ✅ Can view patient details
- ✅ All HD Log fields are working

#### Test 5.2: Schedule Management
**Steps:**
1. From admin dashboard, click "HD Schedule"
2. Should navigate to `/schedule`

**Expected Results:**
- ✅ Schedule grid loads
- ✅ Shows slots and bed assignments
- ✅ Can view schedule

---

### Phase 6: Security & Authorization Testing

#### Test 6.1: Route Guards
**Steps:**
1. Logout
2. Try to navigate directly to `/admin/user-management`

**Expected Results:**
- ✅ Redirected to `/login`
- ✅ After login, redirected to intended route or dashboard

#### Test 6.2: Role-Based Access (Test with Different Roles)
**Login as HOD:**
- Username: `hod`
- Password: `HOD@123`

**Expected:**
- ✅ Can access `/admin/staff-management` (HOD allowed)
- ✅ Can access `/admin/reports` (HOD allowed)
- ❌ Cannot access `/admin/user-management` (Admin only)
- ❌ Cannot access `/admin/system-settings` (Admin only)
- ❌ Cannot access `/admin/audit-logs` (Admin only)

---

## 🐛 Common Issues & Solutions

### Issue: "Cannot find module" errors in frontend
**Solution:** Run `npm install` in the frontend directory

### Issue: CORS errors
**Solution:** Backend already configured for `http://localhost:4200`

### Issue: Token expired
**Solution:** Logout and login again

### Issue: Backend not responding
**Solution:** Check if backend is running on ports 7001/5001

### Issue: Database connection errors
**Solution:** Verify LocalDB is running: `sqllocaldb info MSSQLLocalDB`

---

## 📊 Test Results Summary

### ✅ Completed Tests
- [ ] Phase 1: Authentication (Admin login)
- [ ] Phase 2: User Management (All 8 sub-tests)
- [ ] Phase 3: Backend API (All 5 API groups)
- [ ] Phase 4: Navigation (All 4 placeholder components)
- [ ] Phase 5: Existing Features (Patient & Schedule)
- [ ] Phase 6: Security (Route guards & role-based access)

### 🐛 Bugs Found
_(Record any bugs discovered during testing)_

1. 
2. 
3. 

### 📝 Notes
_(Record observations or suggestions)_

1. 
2. 
3. 

---

## 🎯 Test Coverage

- **Backend APIs:** 5/5 controllers ready for testing
- **Frontend Services:** 5/5 services ready
- **Frontend UI:** 1/5 fully implemented (User Management)
- **Database:** AuditLogs table verified

**Next Priority:** Complete UI for remaining 4 components (Staff, Settings, Reports, Audit Logs)

---

**Tester:** ________________
**Date:** November 11, 2025
**Version:** 1.0
**Status:** Ready for Testing ✅
