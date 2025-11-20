using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.DTOs;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HOD")]
public class ReportsController : ControllerBase
{
    private readonly DapperContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(DapperContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("patient-volume")]
    public async Task<ActionResult<ApiResponse<PatientVolumeReport>>> GetPatientVolume(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            using var connection = _context.CreateConnection();
            
            var dailyVolume = await connection.QueryAsync<DailyVolume>(@"
                SELECT 
                    CAST(ba.AssignedDate as DATE) as Date,
                    COUNT(DISTINCT ba.PatientID) as PatientCount
                FROM BedAssignments ba
                WHERE ba.AssignedDate >= @StartDate AND ba.AssignedDate <= @EndDate
                GROUP BY CAST(ba.AssignedDate as DATE)
                ORDER BY CAST(ba.AssignedDate as DATE)",
                new { StartDate = start, EndDate = end });

            var volumeBySlot = await connection.QueryAsync<SlotVolume>(@"
                SELECT 
                    s.SlotName,
                    COUNT(DISTINCT ba.BedID) as TotalAssignments,
                    COUNT(DISTINCT ba.PatientID) as UniquePatients
                FROM BedAssignments ba
                JOIN Slots s ON ba.SlotID = s.SlotID
                WHERE ba.AssignedDate >= @StartDate AND ba.AssignedDate <= @EndDate
                GROUP BY s.SlotName",
                new { StartDate = start, EndDate = end });

            var totalPatients = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(DISTINCT PatientID) FROM BedAssignments WHERE AssignedDate >= @StartDate AND AssignedDate <= @EndDate",
                new { StartDate = start, EndDate = end });

            var totalSessions = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM BedAssignments WHERE AssignedDate >= @StartDate AND AssignedDate <= @EndDate",
                new { StartDate = start, EndDate = end });

            var report = new PatientVolumeReport
            {
                StartDate = start,
                EndDate = end,
                TotalPatients = totalPatients,
                TotalSessions = totalSessions,
                DailyVolume = dailyVolume.ToList(),
                VolumeBySlot = volumeBySlot.ToList()
            };

            return Ok(ApiResponse<PatientVolumeReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patient volume report");
            return StatusCode(500, ApiResponse<PatientVolumeReport>.ErrorResponse("Error generating report"));
        }
    }

    [HttpGet("occupancy-rates")]
    public async Task<ActionResult<ApiResponse<OccupancyReport>>> GetOccupancyRates(
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.Now;

            using var connection = _context.CreateConnection();
            
            var slotOccupancy = await connection.QueryAsync<SlotOccupancy>(@"
                SELECT 
                    s.SlotID,
                    s.SlotName,
                    s.MaxBeds,
                    COUNT(ba.BedID) as OccupiedBeds,
                    CAST(COUNT(ba.BedID) * 100.0 / s.MaxBeds as decimal(5,2)) as OccupancyRate
                FROM Slots s
                LEFT JOIN BedAssignments ba ON s.SlotID = ba.SlotID 
                    AND CAST(ba.AssignedDate as DATE) = CAST(@TargetDate as DATE)
                    AND ba.IsActive = 1
                WHERE s.IsActive = 1
                GROUP BY s.SlotID, s.SlotName, s.MaxBeds
                ORDER BY s.SlotID",
                new { TargetDate = targetDate });

            var totalBeds = slotOccupancy.Sum(s => s.MaxBeds);
            var totalOccupied = slotOccupancy.Sum(s => s.OccupiedBeds);
            var overallOccupancyRate = totalBeds > 0 ? (decimal)totalOccupied * 100 / totalBeds : 0;

            var report = new OccupancyReport
            {
                ReportDate = targetDate,
                TotalBeds = totalBeds,
                OccupiedBeds = totalOccupied,
                AvailableBeds = totalBeds - totalOccupied,
                OverallOccupancyRate = overallOccupancyRate,
                SlotOccupancy = slotOccupancy.ToList()
            };

            return Ok(ApiResponse<OccupancyReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating occupancy report");
            return StatusCode(500, ApiResponse<OccupancyReport>.ErrorResponse("Error generating report"));
        }
    }

    [HttpGet("treatment-completion")]
    public async Task<ActionResult<ApiResponse<TreatmentCompletionReport>>> GetTreatmentCompletion(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            using var connection = _context.CreateConnection();
            
            var totalSessions = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM HDLog 
                WHERE SessionDate >= @StartDate AND SessionDate <= @EndDate",
                new { StartDate = start, EndDate = end });

            var completedSessions = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM HDLog 
                WHERE SessionDate >= @StartDate AND SessionDate <= @EndDate 
                AND SessionStatus = 'Completed'",
                new { StartDate = start, EndDate = end });

            var patientCompletion = await connection.QueryAsync<PatientCompletion>(@"
                SELECT 
                    p.PatientID,
                    p.Name as PatientName,
                    COUNT(h.LogID) as TotalSessions,
                    SUM(CASE WHEN h.SessionStatus = 'Completed' THEN 1 ELSE 0 END) as CompletedSessions,
                    CAST(SUM(CASE WHEN h.SessionStatus = 'Completed' THEN 1 ELSE 0 END) * 100.0 / COUNT(h.LogID) as decimal(5,2)) as CompletionRate
                FROM Patients p
                LEFT JOIN HDLog h ON p.PatientID = h.PatientID 
                    AND h.SessionDate >= @StartDate AND h.SessionDate <= @EndDate
                GROUP BY p.PatientID, p.Name
                HAVING COUNT(h.LogID) > 0
                ORDER BY CompletionRate DESC",
                new { StartDate = start, EndDate = end });

            var report = new TreatmentCompletionReport
            {
                StartDate = start,
                EndDate = end,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                CompletionRate = totalSessions > 0 ? (decimal)completedSessions * 100 / totalSessions : 0,
                PatientCompletion = patientCompletion.ToList()
            };

            return Ok(ApiResponse<TreatmentCompletionReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating treatment completion report");
            return StatusCode(500, ApiResponse<TreatmentCompletionReport>.ErrorResponse("Error generating report"));
        }
    }

    [HttpGet("staff-performance")]
    public async Task<ActionResult<ApiResponse<List<StaffPerformance>>>> GetStaffPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            using var connection = _context.CreateConnection();
            
            var performance = await connection.QueryAsync<StaffPerformance>(@"
                SELECT 
                    s.StaffID,
                    s.Name as StaffName,
                    s.Role,
                    COUNT(h.LogID) as SessionsHandled,
                    AVG(DATEDIFF(MINUTE, h.ActualStartTime, h.ActualEndTime)) as AvgSessionDuration,
                    SUM(CASE WHEN h.SessionStatus = 'Completed' THEN 1 ELSE 0 END) as CompletedSessions
                FROM Staff s
                LEFT JOIN HDLog h ON s.StaffID = h.TechnicianID 
                    AND h.SessionDate >= @StartDate AND h.SessionDate <= @EndDate
                WHERE s.IsActive = 1
                GROUP BY s.StaffID, s.Name, s.Role
                ORDER BY SessionsHandled DESC",
                new { StartDate = start, EndDate = end });

            return Ok(ApiResponse<List<StaffPerformance>>.SuccessResponse(performance.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating staff performance report");
            return StatusCode(500, ApiResponse<List<StaffPerformance>>.ErrorResponse("Error generating report"));
        }
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<ApiResponse<MonthlySummary>>> GetMonthlySummary(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            using var connection = _context.CreateConnection();
            
            var totalPatients = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(DISTINCT PatientID) FROM HDLog 
                WHERE SessionDate >= @StartDate AND SessionDate <= @EndDate",
                new { StartDate = startDate, EndDate = endDate });

            var totalSessions = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM HDLog 
                WHERE SessionDate >= @StartDate AND SessionDate <= @EndDate",
                new { StartDate = startDate, EndDate = endDate });

            var completedSessions = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM HDLog 
                WHERE SessionDate >= @StartDate AND SessionDate <= @EndDate 
                AND SessionStatus = 'Completed'",
                new { StartDate = startDate, EndDate = endDate });

            var avgOccupancy = await connection.QueryFirstOrDefaultAsync<decimal>(@"
                SELECT AVG(CAST(DailyOccupied * 100.0 / DailyCapacity as decimal(5,2)))
                FROM (
                    SELECT 
                        CAST(ba.AssignedDate as DATE) as AssignmentDate,
                        COUNT(ba.BedID) as DailyOccupied,
                        (SELECT SUM(MaxBeds) FROM Slots WHERE IsActive = 1) as DailyCapacity
                    FROM BedAssignments ba
                    WHERE ba.AssignedDate >= @StartDate AND ba.AssignedDate <= @EndDate
                    GROUP BY CAST(ba.AssignedDate as DATE)
                ) as DailyStats",
                new { StartDate = startDate, EndDate = endDate });

            var newPatients = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM Patients 
                WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate",
                new { StartDate = startDate, EndDate = endDate });

            var summary = new MonthlySummary
            {
                Year = targetYear,
                Month = targetMonth,
                TotalPatients = totalPatients,
                NewPatients = newPatients,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                CompletionRate = totalSessions > 0 ? (decimal)completedSessions * 100 / totalSessions : 0,
                AverageOccupancyRate = avgOccupancy
            };

            return Ok(ApiResponse<MonthlySummary>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly summary");
            return StatusCode(500, ApiResponse<MonthlySummary>.ErrorResponse("Error generating summary"));
        }
    }

    [HttpGet("yearly-summary")]
    public async Task<ActionResult<ApiResponse<YearlySummary>>> GetYearlySummary([FromQuery] int? year = null)
    {
        try
        {
            var targetYear = year ?? DateTime.Now.Year;
            var startDate = new DateTime(targetYear, 1, 1);
            var endDate = new DateTime(targetYear, 12, 31);

            using var connection = _context.CreateConnection();
            
            var monthlyBreakdown = await connection.QueryAsync<MonthlyBreakdown>(@"
                SELECT 
                    MONTH(h.SessionDate) as Month,
                    COUNT(*) as TotalSessions,
                    COUNT(DISTINCT h.PatientID) as UniquePatients,
                    SUM(CASE WHEN h.SessionStatus = 'Completed' THEN 1 ELSE 0 END) as CompletedSessions
                FROM HDLog h
                WHERE YEAR(h.SessionDate) = @Year
                GROUP BY MONTH(h.SessionDate)
                ORDER BY MONTH(h.SessionDate)",
                new { Year = targetYear });

            var totalPatients = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(DISTINCT PatientID) FROM HDLog 
                WHERE YEAR(SessionDate) = @Year",
                new { Year = targetYear });

            var totalSessions = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) FROM HDLog WHERE YEAR(SessionDate) = @Year",
                new { Year = targetYear });

            var summary = new YearlySummary
            {
                Year = targetYear,
                TotalPatients = totalPatients,
                TotalSessions = totalSessions,
                MonthlyBreakdown = monthlyBreakdown.ToList()
            };

            return Ok(ApiResponse<YearlySummary>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating yearly summary");
            return StatusCode(500, ApiResponse<YearlySummary>.ErrorResponse("Error generating summary"));
        }
    }
}

// Report DTOs
public class PatientVolumeReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalPatients { get; set; }
    public int TotalSessions { get; set; }
    public List<DailyVolume> DailyVolume { get; set; } = new();
    public List<SlotVolume> VolumeBySlot { get; set; } = new();
}

public class DailyVolume
{
    public DateTime Date { get; set; }
    public int PatientCount { get; set; }
}

public class SlotVolume
{
    public string SlotName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int UniquePatients { get; set; }
}

public class OccupancyReport
{
    public DateTime ReportDate { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public decimal OverallOccupancyRate { get; set; }
    public List<SlotOccupancy> SlotOccupancy { get; set; } = new();
}

public class SlotOccupancy
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int MaxBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public decimal OccupancyRate { get; set; }
}

public class TreatmentCompletionReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public decimal CompletionRate { get; set; }
    public List<PatientCompletion> PatientCompletion { get; set; } = new();
}

public class PatientCompletion
{
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public decimal CompletionRate { get; set; }
}

public class StaffPerformance
{
    public int StaffID { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int SessionsHandled { get; set; }
    public int? AvgSessionDuration { get; set; }
    public int CompletedSessions { get; set; }
}

public class MonthlySummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalPatients { get; set; }
    public int NewPatients { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageOccupancyRate { get; set; }
}

public class YearlySummary
{
    public int Year { get; set; }
    public int TotalPatients { get; set; }
    public int TotalSessions { get; set; }
    public List<MonthlyBreakdown> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyBreakdown
{
    public int Month { get; set; }
    public int TotalSessions { get; set; }
    public int UniquePatients { get; set; }
    public int CompletedSessions { get; set; }
}
