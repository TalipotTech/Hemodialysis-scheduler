namespace HDScheduler.API.Models;

// Patient Model - Basic Patient Information with Latest Session Data
public class Patient
{
    public int PatientID { get; set; } // Unique patient identifier
    public string? MRN { get; set; } // Medical Record Number
    public string Name { get; set; } = string.Empty; // Patient full name
    public int Age { get; set; } // Patient age in years
    public string? Gender { get; set; } // Patient gender
    public string ContactNumber { get; set; } = string.Empty; // Patient contact information
    public string? EmergencyContact { get; set; } // Emergency contact details
    public string? Address { get; set; } // Patient address
    public string? GuardianName { get; set; } // Guardian name
    public string? HDCycle { get; set; } // HD Cycle pattern (e.g., "MWF", "TTS", "Daily")
    public int? HDFrequency { get; set; } // Number of sessions per week
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Latest Session Data
    public int? ScheduleID { get; set; }
    public int? SlotID { get; set; }
    public int? BedNumber { get; set; }
    public string? DialyserType { get; set; }
    public int? AssignedDoctor { get; set; }
    public int? AssignedNurse { get; set; }
    public string? AssignedDoctorName { get; set; }
    public string? AssignedNurseName { get; set; }
    public DateTime? SessionStartTime { get; set; } // Time when dialysis session started
    public bool IsDischarged { get; set; } = false;
}

// Patient search/lookup models
public class PatientSearchResult
{
    public int PatientID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MRN { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Gender { get; set; }
}

// Combined view for patients with their latest session
public class PatientWithLatestSession
{
    // Patient Demographics
    public int PatientID { get; set; }
    public string? MRN { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }
    public string? GuardianName { get; set; }
    
    // Latest Session Info (if exists)
    public int? LatestScheduleID { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public int? LastSlotID { get; set; }
    public int? LastBedNumber { get; set; }
    public bool? LastSessionDischarged { get; set; }
}
