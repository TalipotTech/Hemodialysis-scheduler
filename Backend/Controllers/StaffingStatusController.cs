using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Data;
using HDScheduler.API.DTOs;
using Dapper;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffingStatusController : ControllerBase
{
    private readonly DapperContext _context;
    private readonly ILogger<StaffingStatusController> _logger;

    public StaffingStatusController(
        DapperContext context,
        ILogger<StaffingStatusController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ShiftStaffingStatus>>>> GetStaffingStatus()
    {
        try
        {
            var query = @"
                SELECT 
                    s.SlotID,
                    s.SlotName,
                    CONCAT(FORMAT(CAST(s.StartTime AS TIME), 'hh:mm tt'), ' - ', FORMAT(CAST(s.EndTime AS TIME), 'hh:mm tt')) as SlotTime,
                    s.BedCapacity,
                    COUNT(CASE WHEN st.Role = 'Doctor' THEN 1 END) as DoctorCount,
                    COUNT(CASE WHEN st.Role = 'Nurse' THEN 1 END) as NurseCount,
                    COUNT(CASE WHEN st.Role = 'Technician' THEN 1 END) as TechnicianCount,
                    COUNT(st.StaffID) as TotalStaff
                FROM Slots s
                LEFT JOIN Staff st ON s.SlotID = st.AssignedSlot AND st.IsActive = 1
                GROUP BY s.SlotID, s.SlotName, s.StartTime, s.EndTime, s.BedCapacity
                ORDER BY s.SlotID";

            using var connection = _context.CreateConnection();
            var results = await connection.QueryAsync<ShiftStaffingStatus>(query);

            // Calculate recommended staffing and status for each shift
            foreach (var shift in results)
            {
                // Recommended: 1 doctor per 6 patients, 1 nurse per 4 patients, 1 tech per 5 patients
                shift.RecommendedDoctors = Math.Max(1, (int)Math.Ceiling(shift.BedCapacity / 6.0));
                shift.RecommendedNurses = Math.Max(2, (int)Math.Ceiling(shift.BedCapacity / 4.0));
                shift.RecommendedTechnicians = Math.Max(1, (int)Math.Ceiling(shift.BedCapacity / 5.0));
                shift.RecommendedTotal = shift.RecommendedDoctors + shift.RecommendedNurses + shift.RecommendedTechnicians;

                // Calculate staffing percentage
                shift.StaffingPercentage = shift.RecommendedTotal > 0
                    ? (int)((shift.TotalStaff / (double)shift.RecommendedTotal) * 100)
                    : 0;

                // Determine status based on staffing percentage
                if (shift.StaffingPercentage < 50)
                {
                    shift.Status = "Critical";
                    shift.StatusColor = "red";
                }
                else if (shift.StaffingPercentage < 80)
                {
                    shift.Status = "Understaffed";
                    shift.StatusColor = "yellow";
                }
                else
                {
                    shift.Status = "Adequate";
                    shift.StatusColor = "green";
                }
            }

            return Ok(ApiResponse<List<ShiftStaffingStatus>>.SuccessResponse(results.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating staffing status");
            return StatusCode(500, ApiResponse<List<ShiftStaffingStatus>>.ErrorResponse("Error calculating staffing status"));
        }
    }

    [HttpGet("{slotId}")]
    public async Task<ActionResult<ApiResponse<ShiftStaffingStatus>>> GetShiftStaffingStatus(int slotId)
    {
        try
        {
            var query = @"
                SELECT 
                    s.SlotID,
                    s.SlotName,
                    CONCAT(FORMAT(CAST(s.StartTime AS TIME), 'hh:mm tt'), ' - ', FORMAT(CAST(s.EndTime AS TIME), 'hh:mm tt')) as SlotTime,
                    s.BedCapacity,
                    COUNT(CASE WHEN st.Role = 'Doctor' THEN 1 END) as DoctorCount,
                    COUNT(CASE WHEN st.Role = 'Nurse' THEN 1 END) as NurseCount,
                    COUNT(CASE WHEN st.Role = 'Technician' THEN 1 END) as TechnicianCount,
                    COUNT(st.StaffID) as TotalStaff
                FROM Slots s
                LEFT JOIN Staff st ON s.SlotID = st.AssignedSlot AND st.IsActive = 1
                WHERE s.SlotID = @SlotID
                GROUP BY s.SlotID, s.SlotName, s.StartTime, s.EndTime, s.BedCapacity";

            using var connection = _context.CreateConnection();
            var shift = await connection.QueryFirstOrDefaultAsync<ShiftStaffingStatus>(query, new { SlotID = slotId });

            if (shift == null)
            {
                return NotFound(ApiResponse<ShiftStaffingStatus>.ErrorResponse("Shift not found"));
            }

            // Calculate recommended staffing and status
            shift.RecommendedDoctors = Math.Max(1, (int)Math.Ceiling(shift.BedCapacity / 6.0));
            shift.RecommendedNurses = Math.Max(2, (int)Math.Ceiling(shift.BedCapacity / 4.0));
            shift.RecommendedTechnicians = Math.Max(1, (int)Math.Ceiling(shift.BedCapacity / 5.0));
            shift.RecommendedTotal = shift.RecommendedDoctors + shift.RecommendedNurses + shift.RecommendedTechnicians;

            shift.StaffingPercentage = shift.RecommendedTotal > 0
                ? (int)((shift.TotalStaff / (double)shift.RecommendedTotal) * 100)
                : 0;

            if (shift.StaffingPercentage < 50)
            {
                shift.Status = "Critical";
                shift.StatusColor = "red";
            }
            else if (shift.StaffingPercentage < 80)
            {
                shift.Status = "Understaffed";
                shift.StatusColor = "yellow";
            }
            else
            {
                shift.Status = "Adequate";
                shift.StatusColor = "green";
            }

            return Ok(ApiResponse<ShiftStaffingStatus>.SuccessResponse(shift));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shift staffing status");
            return StatusCode(500, ApiResponse<ShiftStaffingStatus>.ErrorResponse("Error calculating shift staffing status"));
        }
    }
}

public class ShiftStaffingStatus
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string SlotTime { get; set; } = string.Empty;
    public int BedCapacity { get; set; }
    public int DoctorCount { get; set; }
    public int NurseCount { get; set; }
    public int TechnicianCount { get; set; }
    public int TotalStaff { get; set; }
    public int RecommendedDoctors { get; set; }
    public int RecommendedNurses { get; set; }
    public int RecommendedTechnicians { get; set; }
    public int RecommendedTotal { get; set; }
    public int StaffingPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}
