-- Initialize Slots with default configuration
-- This script sets up 4 time slots with configurable bed capacity

-- Clear existing slots (optional - comment out if you want to keep existing data)
DELETE FROM Slots;

-- Insert default 4 time slots
INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, MaxBeds, BedCapacity, IsActive) VALUES
(1, 'Slot 1 - Morning', '06:00', '10:00', 10, 10, 1),
(2, 'Slot 2 - Afternoon', '11:00', '15:00', 10, 10, 1),
(3, 'Slot 3 - Evening', '16:00', '20:00', 10, 10, 1),
(4, 'Slot 4 - Night', '21:00', '01:00', 10, 10, 1);

-- Verify the slots were created
SELECT * FROM Slots;

-- Show summary
SELECT 
    'Total Slots' as Info, 
    COUNT(*) as Count 
FROM Slots 
WHERE IsActive = 1

UNION ALL

SELECT 
    'Total Bed Capacity' as Info, 
    SUM(MaxBeds) as Count 
FROM Slots 
WHERE IsActive = 1;
