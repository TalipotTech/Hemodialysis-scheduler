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
    public int? PreferredSlotID { get; set; } // Preferred time slot for recurring sessions (1=Morning, 2=Afternoon, 3=Evening, 4=Night)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // HD Treatment Information (added 2025-11-28)
    public decimal? DryWeight { get; set; } // Dry weight in kg
    public DateTime? HDStartDate { get; set; } // Date when HD treatment started
    public string? DialyserType { get; set; } // Hi Flux or Lo Flux
    public string? DialyserModel { get; set; } // Specific dialyser model
    public decimal? PrescribedDuration { get; set; } // Prescribed treatment duration in hours
    public int? PrescribedBFR { get; set; } // Prescribed Blood Flow Rate in mL/min
    public string? DialysatePrescription { get; set; } // Dialysate prescription (e.g., K+ Free, Normal, etc.)
    public int DialyserCount { get; set; } = 0; // Current dialyser usage count
    public int BloodTubingCount { get; set; } = 0; // Current blood tubing usage count
    public int TotalDialysisCompleted { get; set; } = 0; // Total number of dialysis sessions completed
    public int DialysersPurchased { get; set; } = 0; // Total number of dialysers purchased (lifetime)
    public int BloodTubingPurchased { get; set; } = 0; // Total number of blood tubing sets purchased (lifetime)
    
    // Latest Session Data
    public int? ScheduleID { get; set; }
    public int? SlotID { get; set; }
    public int? BedNumber { get; set; }
    public DateTime? SessionDate { get; set; } // Date of the session
    public string? SessionStatus { get; set; } // Session status (Pre-Scheduled, Active, In Progress, Completed)
    public int? AssignedDoctor { get; set; }
    public int? AssignedNurse { get; set; }
    public string? AssignedDoctorName { get; set; }
    public string? AssignedNurseName { get; set; }
    public DateTime? SessionStartTime { get; set; } // Time when dialysis session started (TreatmentStartTime)
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
