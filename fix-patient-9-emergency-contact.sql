-- Fix Patient 9 (Renjith) - Set Emergency Contact to NULL or a different number
-- Currently both ContactNumber and EmergencyContact are the same: 9873807554

-- Option 1: Set EmergencyContact to NULL (recommended if no emergency contact is available)
UPDATE Patients
SET EmergencyContact = NULL
WHERE PatientID = 9;

-- Option 2: Set EmergencyContact to a different number (use this if you have a real emergency contact)
-- UPDATE Patients
-- SET EmergencyContact = '1234567890'  -- Replace with actual emergency contact number
-- WHERE PatientID = 9;

-- Verify the change
SELECT 
    PatientID,
    Name,
    ContactNumber,
    EmergencyContact,
    CASE 
        WHEN ContactNumber = EmergencyContact THEN '❌ SAME (Invalid)'
        WHEN EmergencyContact IS NULL THEN '✅ No Emergency Contact'
        ELSE '✅ Different (Valid)'
    END AS ValidationStatus
FROM Patients
WHERE PatientID = 9;
