using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HOD")]
public class StaffManagementController : ControllerBase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<StaffManagementController> _logger;

    public StaffManagementController(
        IStaffRepository staffRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<StaffManagementController> logger)
    {
        _staffRepository = staffRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Staff>>>> GetAllStaff()
    {
        try
        {
            var staff = await _staffRepository.GetAllAsync();
            return Ok(ApiResponse<List<Staff>>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff");
            return StatusCode(500, ApiResponse<List<Staff>>.ErrorResponse("Error retrieving staff"));
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<Staff>>>> GetActiveStaff()
    {
        try
        {
            var staff = await _staffRepository.GetActiveAsync();
            return Ok(ApiResponse<List<Staff>>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active staff");
            return StatusCode(500, ApiResponse<List<Staff>>.ErrorResponse("Error retrieving active staff"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Staff>>> GetStaff(int id)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound(ApiResponse<Staff>.ErrorResponse("Staff not found"));
            }
            return Ok(ApiResponse<Staff>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff {StaffId}", id);
            return StatusCode(500, ApiResponse<Staff>.ErrorResponse("Error retrieving staff"));
        }
    }

    [HttpGet("role/{role}")]
    public async Task<ActionResult<ApiResponse<List<Staff>>>> GetStaffByRole(string role)
    {
        try
        {
            var staff = await _staffRepository.GetByRoleAsync(role);
            return Ok(ApiResponse<List<Staff>>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff by role {Role}", role);
            return StatusCode(500, ApiResponse<List<Staff>>.ErrorResponse("Error retrieving staff by role"));
        }
    }

    [HttpGet("slot/{slotId}")]
    public async Task<ActionResult<ApiResponse<List<Staff>>>> GetStaffBySlot(int slotId)
    {
        try
        {
            var staff = await _staffRepository.GetBySlotAsync(slotId);
            return Ok(ApiResponse<List<Staff>>.SuccessResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff by slot {SlotId}", slotId);
            return StatusCode(500, ApiResponse<List<Staff>>.ErrorResponse("Error retrieving staff by slot"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HOD")]
    public async Task<ActionResult<ApiResponse<int>>> CreateStaff([FromBody] CreateStaffRequest request)
    {
        try
        {
            var staff = new Staff
            {
                Name = request.Name,
                Role = request.Role,
                ContactNumber = request.ContactNumber,
                StaffSpecialization = request.Specialization,
                AssignedSlot = null,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var staffId = await _staffRepository.CreateAsync(staff);

            await LogAuditAsync("CREATE", "Staff", staffId, null, $"Created staff: {request.Name}, Role: {request.Role}");

            return CreatedAtAction(nameof(GetStaff), new { id = staffId },
                ApiResponse<int>.SuccessResponse(staffId, "Staff created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error creating staff"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HOD")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Staff not found"));
            }

            var oldValues = $"Name: {staff.Name}, Role: {staff.Role}, Slot: {staff.AssignedSlot}";

            staff.Name = request.Name;
            staff.Role = request.Role;
            staff.ContactNumber = request.ContactNumber;
            staff.StaffSpecialization = request.Specialization;
            staff.IsActive = request.IsActive;

            var result = await _staffRepository.UpdateAsync(staff);

            if (result)
            {
                var newValues = $"Name: {request.Name}, Role: {request.Role}";
                await LogAuditAsync("UPDATE", "Staff", id, oldValues, newValues);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff updated successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update staff"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff {StaffId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating staff"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStaff(int id)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Staff not found"));
            }

            var result = await _staffRepository.DeleteAsync(id);

            if (result)
            {
                await LogAuditAsync("DELETE", "Staff", id, $"Deleted staff: {staff.Name}", null);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff deleted successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete staff"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff {StaffId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting staff"));
        }
    }

    [HttpPost("{id}/assign-slot")]
    [Authorize(Roles = "Admin,HOD")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignToSlot(int id, [FromBody] AssignSlotRequest request)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Staff not found"));
            }

            var result = await _staffRepository.AssignToSlotAsync(id, request.SlotID);

            if (result)
            {
                await LogAuditAsync("ASSIGN_SLOT", "Staff", id, $"Previous slot: {staff.AssignedSlot}", $"New slot: {request.SlotID}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff assigned to slot successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to assign staff to slot"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning staff {StaffId} to slot", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error assigning staff to slot"));
        }
    }

    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "Admin,HOD")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStaffStatus(int id)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Staff not found"));
            }

            var newStatus = !staff.IsActive;
            var result = await _staffRepository.ToggleActiveStatusAsync(id, newStatus);

            if (result)
            {
                var action = newStatus ? "ENABLED" : "DISABLED";
                await LogAuditAsync(action, "Staff", id, null, $"{action} staff: {staff.Name}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, $"Staff {(newStatus ? "enabled" : "disabled")} successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update staff status"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for staff {StaffId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating staff status"));
        }
    }

    private async Task LogAuditAsync(string action, string entityType, int? entityId, string? oldValues, string? newValues)
    {
        try
        {
            var log = new AuditLog
            {
                Username = User.Identity?.Name ?? "System",
                Role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                Action = action,
                EntityType = entityType,
                EntityID = entityId,
                Details = $"Old: {oldValues}, New: {newValues}",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };
            await _auditLogRepository.CreateAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit");
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}

public class CreateStaffRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Specialization { get; set; }
}

public class UpdateStaffRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Specialization { get; set; }
    public bool IsActive { get; set; }
}

public class AssignSlotRequest
{
    public int? SlotID { get; set; }
}
