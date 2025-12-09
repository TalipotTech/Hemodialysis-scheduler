namespace HDScheduler.API.Models
{
    // Report Generation Models
    public class DailyReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int MissedSessions { get; set; }
        public int PatientsServed { get; set; }
        public double AverageSessionDuration { get; set; }
        public List<BedUtilizationData> BedUtilization { get; set; } = new();
        public List<SessionData> Sessions { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class WeeklyReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int MissedSessions { get; set; }
        public int UniquePatientsServed { get; set; }
        public double AverageSessionDuration { get; set; }
        public List<DailyData> DailyBreakdown { get; set; } = new();
        public List<PatientFrequencyData> TopPatients { get; set; } = new();
        public string Insights { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class PatientReport
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int HDCycle { get; set; }
        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }
        public int TotalTreatments { get; set; }
        public int CompletedTreatments { get; set; }
        public int MissedTreatments { get; set; }
        public double AverageTreatmentDuration { get; set; }
        public List<TreatmentData> Treatments { get; set; } = new();
        public string ClinicalSummary { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class CustomReport
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class BedUtilizationData
    {
        public string BedNumber { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public string Patients { get; set; } = string.Empty;
    }

    public class SessionData
    {
        public string PatientName { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DailyData
    {
        public DateTime Date { get; set; }
        public int SessionCount { get; set; }
        public int PatientCount { get; set; }
    }

    public class PatientFrequencyData
    {
        public string PatientName { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public double AverageDuration { get; set; }
    }

    public class TreatmentData
    {
        public DateTime Date { get; set; }
        public DateTime? EndDate { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    public class ReportTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class CustomReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class ReportExportRequest
    {
        public string ReportContent { get; set; } = string.Empty;
        public string Title { get; set; } = "Report";
    }
}
