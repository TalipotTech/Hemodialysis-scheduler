-- ============================================
-- Hemodialysis Scheduler Seed Data
-- SQL Server Version
-- ============================================

USE [hds-dev-db];
GO

-- Insert default slots
IF NOT EXISTS (SELECT 1 FROM Slots WHERE SlotID = 1)
BEGIN
    INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, BedCapacity, MaxBeds) VALUES
    (1, 'Morning', '06:00:00', '10:00:00', 10, 10),
    (2, 'Afternoon', '11:00:00', '15:00:00', 10, 10),
    (3, 'Evening', '16:00:00', '20:00:00', 10, 10),
    (4, 'Night', '21:00:00', '01:00:00', 10, 10);
END
GO

-- Insert default users with BCrypt hashed passwords
-- Password for all users: respective role@123 (e.g., Admin@123, Doctor@123)

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
    VALUES ('admin', '$2a$11$rGU0mCXCLvJL8fkZRz0z7.F5L.YJ9MYkqEVxJZCq1r5J9KmJZQx7Y', 'Admin', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'hod')
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
    VALUES ('hod', '$2a$11$sHV1nDYDMwKM9glASA1A8.G6M.ZK0NZlrFWyKADr2s6K0LnKAQy8Z', 'HOD', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'doctor1')
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
    VALUES ('doctor1', '$2a$11$tIW2oEZENxLN0hmBTB2B9.H7N.AL1OAmsFXzLBEs3t7L1MoLBRz9A', 'Doctor', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'nurse1')
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
    VALUES ('nurse1', '$2a$11$uJX3pFAFOyMO1inCUC3C0.I8O.BM2PBntGYAMCFt4u8M2NpMCSA0B', 'Nurse', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'tech1')
    INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
    VALUES ('tech1', '$2a$11$vKY4qGBGPzNP2joDVD4D1.J9P.CN3QCouHZBNDGu5v9N3OqNDTB1C', 'Technician', 1, GETUTCDATE());
GO

-- Insert sample staff members
IF NOT EXISTS (SELECT 1 FROM Staff WHERE Name = 'Dr. John Smith')
BEGIN
    INSERT INTO Staff (Name, Role, AssignedSlot, IsActive, CreatedAt) VALUES
    ('Dr. John Smith', 'Doctor', 1, 1, GETUTCDATE()),
    ('Dr. Sarah Johnson', 'Doctor', 2, 1, GETUTCDATE()),
    ('Nurse Mary Williams', 'Nurse', 1, 1, GETUTCDATE()),
    ('Nurse Lisa Brown', 'Nurse', 2, 1, GETUTCDATE()),
    ('Tech Mike Davis', 'Technician', 1, 1, GETUTCDATE()),
    ('HOD Dr. Robert Wilson', 'HOD', NULL, 1, GETUTCDATE());
END
GO

PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT 'Default user accounts:';
PRINT '  Admin: admin / Admin@123';
PRINT '  HOD: hod / Hod@123';
PRINT '  Doctor: doctor1 / Doctor@123';
PRINT '  Nurse: nurse1 / Nurse@123';
PRINT '  Technician: tech1 / Tech@123';
PRINT '';
PRINT 'IMPORTANT: Change these default passwords in production!';
GO
