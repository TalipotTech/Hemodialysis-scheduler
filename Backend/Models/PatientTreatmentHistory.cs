namespace HDScheduler.API.Models;

// Complete Patient Treatment History
public class PatientTreatmentHistory
{
    public Patient? PatientInfo { get; set; }
    public List<TreatmentSessionSummary> Sessions { get; set; } = new();
    public TreatmentStatistics? Statistics { get; set; }
}

// Individual Treatment Session with all related data
public class TreatmentSessionSummary
{
    // Session Basic Info
    public int ScheduleID { get; set; }
    public DateTime SessionDate { get; set; }
    public int PatientID { get; set; }
    public string? PatientName { get; set; }
    public int? SlotID { get; set; }
    public string? SlotName { get; set; }
    public int? BedNumber { get; set; }
    
    // Prescription
    public string? HDCycle { get; set; }
    public string? DialyserType { get; set; }
    public string? DialyserModel { get; set; }
    public int? DialyserReuseCount { get; set; }
    public int? BloodTubingReuse { get; set; }
    public int? HDUnitNumber { get; set; }
    public decimal? PrescribedDuration { get; set; }
    public decimal? UFGoal { get; set; }
    public string? DialysatePrescription { get; set; }
    public int? PrescribedBFR { get; set; }
    public string? AccessType { get; set; }
    public string? AccessLocation { get; set; }
    public string? DryWeight { get; set; }
    public decimal? WeightGain { get; set; }
    public DateTime? HDStartDate { get; set; }
    
    // Anticoagulation
    public string? AnticoagulationType { get; set; }
    public decimal? HeparinDose { get; set; }
    public string? SyringeType { get; set; }
    public decimal? BolusDose { get; set; }
    public decimal? HeparinInfusionRate { get; set; }
    
    // Session Timing
    public string? StartTime { get; set; }
    public string? SessionStartTime { get; set; }
    
    // Pre-Dialysis Assessment
    public decimal? PreWeight { get; set; }
    public string? PreBPSitting { get; set; }
    public decimal? PreTemperature { get; set; }
    public string? AccessBleedingTime { get; set; }
    public string? AccessStatus { get; set; }
    public string? Complications { get; set; }
    
    // Intra-Dialytic Monitoring
    public string? MonitoringTime { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRate { get; set; }
    public int? ActualBFR { get; set; }
    public int? VenousPressure { get; set; }
    public int? ArterialPressure { get; set; }
    public decimal? CurrentUFR { get; set; }
    public decimal? TotalUFAchieved { get; set; }
    public int? TmpPressure { get; set; }
    public string? Symptoms { get; set; }
    public string? Interventions { get; set; }
    public string? StaffInitials { get; set; }
    
    // Post-Dialysis Medications
    public string? MedicationType { get; set; }
    public string? MedicationName { get; set; }
    public string? Dose { get; set; }
    public string? Route { get; set; }
    public string? AdministeredAt { get; set; }
    
    // Alerts
    public string? AlertType { get; set; }
    public string? AlertMessage { get; set; }
    public string? Severity { get; set; }
    public string? Resolution { get; set; }
    
    // Staff
    public int? AssignedDoctor { get; set; }
    public string? AssignedDoctorName { get; set; }
    public int? AssignedNurse { get; set; }
    public string? AssignedNurseName { get; set; }
    
    // Vitals from HDLog
    public decimal? PostWeight { get; set; }
    public decimal? WeightLoss { get; set; }
    public string? BloodPressurePre { get; set; }
    public string? BloodPressurePost { get; set; }
    public decimal? Temperature { get; set; }
    
    // Post-Dialysis Vitals
    public int? PostSBP { get; set; }
    public int? PostDBP { get; set; }
    public int? PostHR { get; set; }
    public string? PostAccessStatus { get; set; }
    public decimal? TotalFluidRemoved { get; set; }
    
    // Additional Session Info
    public string? SessionStatus { get; set; }
    public bool? BloodTestDone { get; set; }
    
    // Related Records Count
    public int IntraDialyticRecordsCount { get; set; }
    public int MedicationsCount { get; set; }
    
    // Status
    public bool IsDischarged { get; set; }
    public bool? IsMovedToHistory { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Treatment Statistics for Timeline/Graphs
public class TreatmentStatistics
{
    public int TotalSessions { get; set; }
    public DateTime? FirstSessionDate { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public decimal? AverageWeightLoss { get; set; }
    public decimal? AveragePreWeight { get; set; }
    public decimal? AveragePostWeight { get; set; }
    public int TotalMedicationsAdministered { get; set; }
    public List<string> CommonMedications { get; set; } = new();
}

// Detailed Session View (when clicking on a specific session)
public class DetailedSessionView
{
    public TreatmentSessionSummary? SessionInfo { get; set; }
    public List<object> IntraDialyticRecords { get; set; } = new(); // Using object to support dynamic mapping
    public List<PostDialysisMedication> Medications { get; set; } = new();
    public HDLog? SessionLog { get; set; }
}

// Time-series data for graphs
public class VitalTrend
{
    public DateTime SessionDate { get; set; }
    public decimal? Value { get; set; }
    public string? Label { get; set; }
}

public class PatientVitalTrends
{
    public List<VitalTrend> WeightTrend { get; set; } = new();
    public List<VitalTrend> WeightLossTrend { get; set; } = new();
    public List<VitalTrend> UFGoalTrend { get; set; } = new();
    public List<BPTrend> BloodPressureTrend { get; set; } = new();
}

public class BPTrend
{
    public DateTime SessionDate { get; set; }
    public string? PreBP { get; set; }
    public string? PostBP { get; set; }
    public int? PreSystolic { get; set; }
    public int? PreDiastolic { get; set; }
    public int? PostSystolic { get; set; }
    public int? PostDiastolic { get; set; }
}
