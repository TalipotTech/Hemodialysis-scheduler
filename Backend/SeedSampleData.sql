-- Add sample patients for testing
INSERT INTO Patients (MRN, Name, Age, Gender, ContactNumber, Address, IsActive) VALUES
('MRN001', 'John Doe', 45, 'Male', '555-0101', '123 Main St', 1),
('MRN002', 'Jane Smith', 52, 'Female', '555-0102', '456 Oak Ave', 1),
('MRN003', 'Robert Johnson', 38, 'Male', '555-0103', '789 Pine Rd', 1),
('MRN004', 'Mary Williams', 61, 'Female', '555-0104', '321 Elm St', 1),
('MRN005', 'James Brown', 47, 'Male', '555-0105', '654 Maple Dr', 1);

-- Add HD schedules for today
INSERT INTO HDSchedule (PatientID, SessionDate, SlotID, BedNumber, DryWeight, DialyserType, BloodPressure, IsDischarged) VALUES
(1, date('now'), 1, 1, 65.5, 'HI', '120/80', 0),
(2, date('now'), 1, 2, 58.2, 'LO', '130/85', 0),
(3, date('now'), 2, 1, 72.0, 'HI', '125/82', 0),
(4, date('now'), 2, 3, 54.8, 'LO', '118/75', 0),
(5, date('now'), 3, 1, 68.5, 'HI', '135/90', 0);

SELECT 'Sample data added successfully!' as Message;
SELECT 'Patients: ' || COUNT(*) as PatientCount FROM Patients;
SELECT 'HD Schedules: ' || COUNT(*) as ScheduleCount FROM HDSchedule;
