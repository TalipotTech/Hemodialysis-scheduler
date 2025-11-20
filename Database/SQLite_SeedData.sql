-- HD Scheduler SQLite Seed Data
-- Insert default data for testing

-- Insert Slots (Time slots for HD sessions)
INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, BedCapacity, MaxBeds, IsActive) VALUES
(1, 'Morning', '06:00:00', '10:00:00', 10, 10, 1),
(2, 'Afternoon', '11:00:00', '15:00:00', 10, 10, 1),
(3, 'Evening', '16:00:00', '20:00:00', 10, 10, 1),
(4, 'Night', '21:00:00', '01:00:00', 10, 10, 1);

-- Insert Users (Pre-hashed passwords using BCrypt)
-- Password for all users: Admin@123, Doctor@123, Nurse@123, Tech@123
INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) VALUES
('admin', '$2a$11$8XZVx2F5t7YnZmkxI6Z2ZurKx0b9Rj8W6Pg5hMQU0hKV9fBZL8Tqm', 'Admin', 1, datetime('now')),
('hod', '$2a$11$8XZVx2F5t7YnZmkxI6Z2ZurKx0b9Rj8W6Pg5hMQU0hKV9fBZL8Tqm', 'HOD', 1, datetime('now')),
('doctor1', '$2a$11$8XZVx2F5t7YnZmkxI6Z2ZurKx0b9Rj8W6Pg5hMQU0hKV9fBZL8Tqm', 'Doctor', 1, datetime('now')),
('nurse1', '$2a$11$8XZVx2F5t7YnZmkxI6Z2ZurKx0b9Rj8W6Pg5hMQU0hKV9fBZL8Tqm', 'Nurse', 1, datetime('now')),
('tech1', '$2a$11$8XZVx2F5t7YnZmkxI6Z2ZurKx0b9Rj8W6Pg5hMQU0hKV9fBZL8Tqm', 'Technician', 1, datetime('now'));

-- Insert Staff Members
INSERT INTO Staff (Name, Role, ContactNumber, StaffSpecialization, AssignedSlot, IsActive, CreatedAt) VALUES
('Dr. John Smith', 'Doctor', '555-0101', 'Nephrology', 1, 1, datetime('now')),
('Dr. Sarah Johnson', 'Doctor', '555-0102', 'Internal Medicine', 2, 1, datetime('now')),
('Nurse Mary Williams', 'Nurse', '555-0103', 'Dialysis Specialist', 1, 1, datetime('now')),
('Nurse Lisa Brown', 'Nurse', '555-0104', 'Critical Care', 2, 1, datetime('now')),
('Tech Mike Davis', 'Technician', '555-0105', 'Dialysis Technician', 1, 1, datetime('now')),
('HOD Dr. Robert Wilson', 'HOD', '555-0100', 'Head of Department', NULL, 1, datetime('now'));

-- Insert Sample Patients
INSERT INTO Patients (
    Name, Age, DryWeight, HDStartDate, HDCycle, WeightGain, DialyserType, 
    DialyserReuseCount, BloodTubingReuse, HeparinDose, Symptoms, BloodTestDone, 
    BloodPressure, SlotID, BedNumber, CreatedByStaffName, CreatedByStaffRole, 
    IsDischarged, CreatedAt, UpdatedAt, AssignedDoctor, AssignedNurse,
    MRN, AccessType, PrescribedDuration, HDUnitNumber, UFGoal
) VALUES
('Ahmed Hassan', 65, 70.5, '2024-01-15', 'Mon-Wed-Fri', 2.5, 'HI', 3, 0, 5000, 'None', 1, '140/80', 1, 1, 'Dr. John Smith', 'Doctor', 0, datetime('now'), datetime('now'), 1, 3, 'MRN001', 'AVF', 4.0, 'HD Unit 1', 2.5),
('Fatima Ali', 58, 65.0, '2024-02-01', 'Tue-Thu-Sat', 2.0, 'HI', 2, 0, 4500, 'Mild fatigue', 1, '130/75', 1, 2, 'Dr. John Smith', 'Doctor', 0, datetime('now'), datetime('now'), 1, 3, 'MRN002', 'AVF', 4.0, 'HD Unit 2', 2.0),
('Mohammed Youssef', 72, 75.0, '2023-11-20', 'Mon-Wed-Fri', 3.0, 'LO', 4, 1, 5500, 'Hypertension', 1, '150/90', 2, 1, 'Dr. Sarah Johnson', 'Doctor', 0, datetime('now'), datetime('now'), 2, 4, 'MRN003', 'CVC', 3.5, 'HD Unit 3', 3.0),
('Aisha Ibrahim', 63, 68.0, '2024-03-10', 'Tue-Thu-Sat', 2.2, 'HI', 1, 0, 4800, 'None', 0, '135/78', 2, 2, 'Dr. Sarah Johnson', 'Doctor', 0, datetime('now'), datetime('now'), 2, 4, 'MRN004', 'AVG', 4.0, 'HD Unit 1', 2.2);

-- Insert Sample Bed Assignments
INSERT INTO BedAssignments (PatientID, SlotID, BedNumber, AssignmentDate, IsActive, CreatedAt) VALUES
(1, 1, 1, date('now'), 1, datetime('now')),
(2, 1, 2, date('now'), 1, datetime('now')),
(3, 2, 1, date('now'), 1, datetime('now')),
(4, 2, 2, date('now'), 1, datetime('now'));

-- Insert Sample Audit Log
INSERT INTO AuditLogs (UserID, Username, Action, EntityType, EntityID, NewValues, IPAddress, CreatedAt) VALUES
(1, 'admin', 'LOGIN', 'User', 1, 'System initialized', '127.0.0.1', datetime('now'));

-- Print success message
SELECT 'SQLite seed data inserted successfully!' AS Message;
SELECT 'Total Users: ' || COUNT(*) AS Info FROM Users;
SELECT 'Total Staff: ' || COUNT(*) AS Info FROM Staff;
SELECT 'Total Patients: ' || COUNT(*) AS Info FROM Patients;
SELECT 'Total Slots: ' || COUNT(*) AS Info FROM Slots;
