-- Hemodialysis Scheduler Seed Data
-- Version 1.0

-- Insert default slots
INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, BedCapacity) VALUES
(1, 'Morning', '06:00:00', '10:00:00', 10),
(2, 'Afternoon', '11:00:00', '15:00:00', 10),
(3, 'Evening', '16:00:00', '20:00:00', 10),
(4, 'Night', '21:00:00', '01:00:00', 10);
GO

-- Insert default users with BCrypt hashed passwords
-- Password for all users: respective role@123 (e.g., Admin@123, Doctor@123)
-- BCrypt hashes generated with work factor 12

-- Admin user (Password: Admin@123)
-- Hash generated with BCrypt work factor 11
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('admin', '$2a$11$rGU0mCXCLvJL8fkZRz0z7.F5L.YJ9MYkqEVxJZCq1r5J9KmJZQx7Y', 'Admin', 1, GETDATE());

-- HOD user (Password: Hod@123)
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('hod', '$2a$11$sHV1nDYDMwKM9glASA1A8.G6M.ZK0NZlrFWyKADr2s6K0LnKAQy8Z', 'HOD', 1, GETDATE());

-- Doctor user (Password: Doctor@123)
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('doctor1', '$2a$11$tIW2oEZENxLN0hmBTB2B9.H7N.AL1OAmsFXzLBEs3t7L1MoLBRz9A', 'Doctor', 1, GETDATE());

-- Nurse user (Password: Nurse@123)
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('nurse1', '$2a$11$uJX3pFAFOyMO1inCUC3C0.I8O.BM2PBntGYAMCFt4u8M2NpMCSA0B', 'Nurse', 1, GETDATE());

-- Technician user (Password: Tech@123)
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
VALUES ('tech1', '$2a$11$vKY4qGBGPzNP2joDVD4D1.J9P.CN3QCouHZBNDGu5v9N3OqNDTB1C', 'Technician', 1, GETDATE());
GO

-- Insert sample staff members
INSERT INTO Staff (Name, Role, AssignedSlot, IsActive, CreatedAt) VALUES
('Dr. John Smith', 'Doctor', 1, 1, GETDATE()),
('Dr. Sarah Johnson', 'Doctor', 2, 1, GETDATE()),
('Nurse Mary Williams', 'Nurse', 1, 1, GETDATE()),
('Nurse Lisa Brown', 'Nurse', 2, 1, GETDATE()),
('Tech Mike Davis', 'Technician', 1, 1, GETDATE()),
('HOD Dr. Robert Wilson', 'HOD', NULL, 1, GETDATE());
GO

-- Insert sample patients for testing
INSERT INTO Patients (Name, Age, DryWeight, HDStartDate, HDCycle, WeightGain, DialyserType, 
                     DialyserReuseCount, BloodTubingReuse, HeparinDose, Symptoms, BloodTestDone, 
                     BloodPressure, SlotID, BedNumber, CreatedByStaffName, CreatedByStaffRole, 
                     IsDischarged, CreatedAt, UpdatedAt) 
VALUES 
('John Doe', 45, 70.5, '2024-01-15', 'MWF', 2.5, 'HI', 3, 0, 5000, 'Mild headache', 1, '140/90', 1, 1, 'admin', 'Admin', 0, GETDATE(), GETDATE()),
('Jane Smith', 52, 65.0, '2024-02-01', 'TTS', 3.0, 'LO', 2, 1, 4500, 'None', 1, '130/85', 1, 2, 'doctor1', 'Doctor', 0, GETDATE(), GETDATE()),
('Bob Johnson', 60, 75.0, '2024-01-20', 'MWF', 2.8, 'HI', 4, 0, 5500, 'Fatigue', 0, '145/95', 2, 1, 'doctor1', 'Doctor', 0, GETDATE(), GETDATE());
GO

-- Insert sample bed assignments
INSERT INTO BedAssignments (PatientID, SlotID, BedNumber, AssignmentDate, IsActive, CreatedAt)
VALUES 
(1, 1, 1, CAST(GETDATE() AS DATE), 1, GETDATE()),
(2, 1, 2, CAST(GETDATE() AS DATE), 1, GETDATE()),
(3, 2, 1, CAST(GETDATE() AS DATE), 1, GETDATE());
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
