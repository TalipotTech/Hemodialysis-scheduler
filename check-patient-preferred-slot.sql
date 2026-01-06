-- Check patient with ID 9 and their preferred slot
SELECT 
    p.PatientID,
    p.Name,
    p.MRN,
    p.Age,
    p.Gender,
    p.ContactNumber,
    p.EmergencyContact,
    p.PreferredSlotID,
    s.SlotName,
    s.StartTime,
    s.EndTime,
    p.HDCycle,
    p.HDFrequency,
    p.DialyserCount,
    p.BloodTubingCount,
    p.TotalDialysisCompleted
FROM Patients p
LEFT JOIN Slots s ON p.PreferredSlotID = s.SlotID
WHERE p.PatientID = 9;

-- Check all data for patient 9 to debug validation
SELECT * FROM Patients WHERE PatientID = 9;

-- Check if PreferredSlotID is null or has value
SELECT 
    PatientID,
    Name,
    PreferredSlotID,
    CASE 
        WHEN PreferredSlotID IS NULL THEN 'Not Set'
        WHEN PreferredSlotID = 1 THEN 'Morning'
        WHEN PreferredSlotID = 2 THEN 'Afternoon'
        WHEN PreferredSlotID = 3 THEN 'Evening'
        WHEN PreferredSlotID = 4 THEN 'Night'
        ELSE 'Unknown'
    END AS PreferredSlot
FROM Patients
WHERE PatientID = 9;
