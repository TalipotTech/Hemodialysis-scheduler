-- Run this SQL in your database to initialize slots
-- Copy and paste into your SQL editor or database tool

DELETE FROM Slots;

INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, MaxBeds, BedCapacity, IsActive) VALUES
(1, 'Slot 1 - Morning', '06:00', '10:00', 10, 10, 1),
(2, 'Slot 2 - Afternoon', '11:00', '15:00', 10, 10, 1),
(3, 'Slot 3 - Evening', '16:00', '20:00', 10, 10, 1),
(4, 'Slot 4 - Night', '21:00', '01:00', 10, 10, 1);

SELECT * FROM Slots;
