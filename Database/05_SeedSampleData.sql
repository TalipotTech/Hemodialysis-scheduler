-- Sample Data for User Management and Staff Management
-- Run this after initial setup to populate test data

USE HDScheduler;
GO

-- Add sample staff members (if not already exists)
IF NOT EXISTS (SELECT 1 FROM Staff WHERE Name = 'Dr. Rajesh Kumar')
BEGIN
    INSERT INTO Staff (Name, Role, ContactNumber, Specialization, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('Dr. Rajesh Kumar', 'Doctor', '+91-9876543210', 'Nephrology', 1, GETDATE(), GETDATE()),
    ('Dr. Priya Sharma', 'Doctor', '+91-9876543211', 'Internal Medicine', 1, GETDATE(), GETDATE()),
    ('Nurse Anita Verma', 'Nurse', '+91-9876543212', 'Critical Care', 1, GETDATE(), GETDATE()),
    ('Nurse Sanjay Patel', 'Nurse', '+91-9876543213', 'Dialysis Care', 1, GETDATE(), GETDATE()),
    ('Nurse Rita Mehta', 'Nurse', '+91-9876543214', 'Patient Care', 1, GETDATE(), GETDATE()),
    ('Tech Amit Singh', 'Technician', '+91-9876543215', 'Dialysis Equipment', 1, GETDATE(), GETDATE()),
    ('Tech Vikram Joshi', 'Technician', '+91-9876543216', 'Medical Equipment', 1, GETDATE(), GETDATE());
    
    PRINT 'Sample staff members added successfully!';
END
ELSE
BEGIN
    PRINT 'Staff members already exist.';
END

-- Add sample users (if not already exists)
-- Password for all: "Password@123" 
-- Hash: $2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'dr.kumar')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt)
    VALUES 
    ('dr.kumar', '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S', 'Doctor', 1, GETDATE()),
    ('dr.sharma', '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S', 'Doctor', 1, GETDATE()),
    ('nurse.anita', '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S', 'Nurse', 1, GETDATE()),
    ('nurse.sanjay', '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S', 'Nurse', 1, GETDATE()),
    ('tech.amit', '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S', 'Technician', 1, GETDATE());
    
    PRINT 'Sample users added successfully!';
    PRINT 'All sample users have password: Password@123';
END
ELSE
BEGIN
    PRINT 'Sample users already exist.';
END

-- Verify the data
SELECT 'Staff Members' as TableName, COUNT(*) as RecordCount FROM Staff
UNION ALL
SELECT 'Users', COUNT(*) FROM Users;

-- Show staff details
SELECT Name, Role, ContactNumber, Specialization, IsActive 
FROM Staff 
ORDER BY Role, Name;

-- Show user details
SELECT Username, Role, IsActive 
FROM Users 
ORDER BY Role, Username;
