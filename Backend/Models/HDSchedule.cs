namespace HDScheduler.API.Models;

// HDSchedule Model - Hemodialysis Session Data
public class HDSchedule
{
    public int ScheduleID { get; set; }
    public int PatientID { get; set; }
    public DateTime SessionDate { get; set; }
    
    // Basic HD Info
    public decimal? DryWeight { get; set; }
    public DateTime? HDStartDate { get; set; }
    public string? HDCycle { get; set; }
    public decimal? WeightGain { get; set; }
    
    // Equipment & Supplies
    public string? DialyserType { get; set; } // HI/LO
    public string? DialyserModel { get; set; }
    public int DialyserReuseCount { get; set; }
    public int BloodTubingReuse { get; set; }
    public string? HDUnitNumber { get; set; }
    
    // Prescription
    public decimal? PrescribedDuration { get; set; } // in hours
    public decimal? UFGoal { get; set; } // Ultrafiltration goal in liters
    public string? DialysatePrescription { get; set; } // Normal / K+ Free / Ca++ / Dextrose
    public int? PrescribedBFR { get; set; } // Blood Flow Rate in mL/min
    
    // Anticoagulation
    public string? AnticoagulationType { get; set; } // Heparin / Without Heparin
    public decimal? HeparinDose { get; set; }
    public string? SyringeType { get; set; } // 10/20 ml
    public decimal? BolusDose { get; set; } // in ml
    public decimal? HeparinInfusionRate { get; set; } // in ml/hr
    
    // Access Type
    public string? AccessType { get; set; } // AVF/AVG/CVC
    public string? AccessLocation { get; set; }
    
    // Vitals & Status
    public string? BloodPressure { get; set; }
    public string? Symptoms { get; set; }
    public bool BloodTestDone { get; set; }
    
    // Bed Assignment
    public int? SlotID { get; set; }
    public int? BedNumber { get; set; }
    
    // Staff Assignment
    public int? AssignedDoctor { get; set; }
    public int? AssignedNurse { get; set; }
    public string? CreatedByStaffName { get; set; }
    public string? CreatedByStaffRole { get; set; }
    
    // HDTreatmentSession fields
    public string? StartTime { get; set; }
    public decimal? PreWeight { get; set; }
    public string? PreBPSitting { get; set; }
    public decimal? PreTemperature { get; set; }
    public string? AccessBleedingTime { get; set; }
    public string? AccessStatus { get; set; }
    public string? Complications { get; set; }
    
    // IntraDialyticMonitoring fields
    public string? MonitoringTime { get; set; }
    public int? HeartRate { get; set; }
    public int? ActualBFR { get; set; }
    public int? VenousPressure { get; set; }
    public int? ArterialPressure { get; set; }
    public decimal? CurrentUFR { get; set; }
    public decimal? TotalUFAchieved { get; set; }
    public int? TmpPressure { get; set; }
    public string? Interventions { get; set; }
    public string? StaffInitials { get; set; }
    
    // PostDialysisMedications fields
    public string? MedicationType { get; set; }
    public string? MedicationName { get; set; }
    public string? Dose { get; set; }
    public string? Route { get; set; }
    public string? AdministeredAt { get; set; }
    
    // TreatmentAlerts fields
    public string? AlertType { get; set; }
    public string? AlertMessage { get; set; }
    public string? Severity { get; set; }
    public string? Resolution { get; set; }
    
    // Post-Dialysis Assessment fields
    public decimal? PostWeight { get; set; }
    public int? PostSBP { get; set; }
    public int? PostDBP { get; set; }
    public int? PostHR { get; set; }
    public string? PostAccessStatus { get; set; }
    public decimal? TotalFluidRemoved { get; set; }
    public string? Notes { get; set; }
    
    // Status
    public bool IsDischarged { get; set; }
    public bool IsMovedToHistory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties (from joins)
    public string? PatientName { get; set; }
    public string? AssignedDoctorName { get; set; }
    public string? AssignedNurseName { get; set; }
}

// Request Models
public class CreatePatientRequest
{
    // Basic Patient Information
    public string? MRN { get; set; } // Medical Record Number
    public string Name { get; set; } = string.Empty; // Patient full name
    public int Age { get; set; } // Patient age in years
    public string? Gender { get; set; } // Patient gender
    public string ContactNumber { get; set; } = string.Empty; // Patient contact information
    public string? EmergencyContact { get; set; } // Emergency contact details
    public string? Address { get; set; } // Patient address
    public string? GuardianName { get; set; } // Guardian name
}

public class CreateHDScheduleRequest
{
    public int PatientID { get; set; }
    public DateTime SessionDate { get; set; }
    
    // Basic HD Info
    public decimal? DryWeight { get; set; }
    public DateTime? HDStartDate { get; set; }
    public string? HDCycle { get; set; }
    public decimal? WeightGain { get; set; }
    
    // Equipment
    public string? DialyserType { get; set; }
    public string? DialyserModel { get; set; }
    public int DialyserReuseCount { get; set; }
    public int BloodTubingReuse { get; set; }
    public string? HDUnitNumber { get; set; }
    
    // Prescription
    public decimal? PrescribedDuration { get; set; }
    public decimal? UFGoal { get; set; }
    public string? DialysatePrescription { get; set; }
    public int? PrescribedBFR { get; set; }
    
    // Anticoagulation
    public string? AnticoagulationType { get; set; }
    public decimal? HeparinDose { get; set; }
    public string? SyringeType { get; set; }
    public decimal? BolusDose { get; set; }
    public decimal? HeparinInfusionRate { get; set; }
    
    // Access
    public string? AccessType { get; set; }
    public string? AccessLocation { get; set; }
    
    // Vitals
    public string? BloodPressure { get; set; }
    public string? Symptoms { get; set; }
    public bool BloodTestDone { get; set; }
    
    // Assignment
    public int? SlotID { get; set; }
    public int? BedNumber { get; set; }
    public int? AssignedDoctor { get; set; }
    public int? AssignedNurse { get; set; }
    
    // HDTreatmentSession fields
    public string? StartTime { get; set; }
    public decimal? PreWeight { get; set; }
    public string? PreBPSitting { get; set; }
    public decimal? PreTemperature { get; set; }
    public string? AccessBleedingTime { get; set; }
    public string? AccessStatus { get; set; }
    public string? Complications { get; set; }
    
    // IntraDialyticMonitoring fields
    public string? MonitoringTime { get; set; }
    public int? HeartRate { get; set; }
    public int? ActualBFR { get; set; }
    public int? VenousPressure { get; set; }
    public int? ArterialPressure { get; set; }
    public decimal? CurrentUFR { get; set; }
    public decimal? TotalUFAchieved { get; set; }
    public int? TmpPressure { get; set; }
    public string? Interventions { get; set; }
    public string? StaffInitials { get; set; }
    
    // PostDialysisMedications fields
    public string? MedicationType { get; set; }
    public string? MedicationName { get; set; }
    public string? Dose { get; set; }
    public string? Route { get; set; }
    public string? AdministeredAt { get; set; }
    
    // TreatmentAlerts fields
    public string? AlertType { get; set; }
    public string? AlertMessage { get; set; }
    public string? Severity { get; set; }
    public string? Resolution { get; set; }
    
    // Post-Dialysis Assessment fields
    public decimal? PostWeight { get; set; }
    public int? PostSBP { get; set; }
    public int? PostDBP { get; set; }
    public int? PostHR { get; set; }
    public string? PostAccessStatus { get; set; }
    public decimal? TotalFluidRemoved { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePatientRequest : CreatePatientRequest
{
    public int PatientID { get; set; }
}

public class UpdateHDScheduleRequest : CreateHDScheduleRequest
{
    public int ScheduleID { get; set; }
    public bool IsDischarged { get; set; }
}
