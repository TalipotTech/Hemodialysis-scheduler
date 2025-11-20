using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;
using Dapper;
using HDScheduler.API.Data;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly DapperContext _context;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        DapperContext context,
        IAuditLogRepository auditLogRepository,
        ILogger<SystemSettingsController> logger)
    {
        _context = context;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    // Slot Management
    [HttpGet("slots")]
    public async Task<ActionResult<ApiResponse<List<SlotConfiguration>>>> GetSlots()
    {
        try
        {
            using var connection = _context.CreateConnection();
            var query = "SELECT SlotID, SlotName, StartTime, EndTime, MaxBeds, IsActive FROM Slots ORDER BY SlotID";
            var slots = await connection.QueryAsync<SlotConfiguration>(query);
            return Ok(ApiResponse<List<SlotConfiguration>>.SuccessResponse(slots.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving slots");
            return StatusCode(500, ApiResponse<List<SlotConfiguration>>.ErrorResponse("Error retrieving slots"));
        }
    }

    [HttpGet("slots/{id}")]
    public async Task<ActionResult<ApiResponse<SlotConfiguration>>> GetSlot(int id)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var query = "SELECT SlotID, SlotName, StartTime, EndTime, MaxBeds, IsActive FROM Slots WHERE SlotID = @SlotID";
            var slot = await connection.QueryFirstOrDefaultAsync<SlotConfiguration>(query, new { SlotID = id });
            
            if (slot == null)
            {
                return NotFound(ApiResponse<SlotConfiguration>.ErrorResponse("Slot not found"));
            }
            
            return Ok(ApiResponse<SlotConfiguration>.SuccessResponse(slot));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving slot {SlotId}", id);
            return StatusCode(500, ApiResponse<SlotConfiguration>.ErrorResponse("Error retrieving slot"));
        }
    }

    [HttpPost("slots")]
    public async Task<ActionResult<ApiResponse<int>>> CreateSlot([FromBody] CreateSlotRequest request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check if slot name already exists
            var existingSlot = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM Slots WHERE SlotName = @SlotName",
                new { request.SlotName });

            if (existingSlot > 0)
            {
                return BadRequest(ApiResponse<int>.ErrorResponse("Slot with this name already exists"));
            }

            var query = @"INSERT INTO Slots (SlotName, StartTime, EndTime, MaxBeds, IsActive) 
                         VALUES (@SlotName, @StartTime, @EndTime, @MaxBeds, 1);
                         SELECT CAST(SCOPE_IDENTITY() as int)";
            
            var slotId = await connection.QuerySingleAsync<int>(query, new
            {
                request.SlotName,
                request.StartTime,
                request.EndTime,
                request.MaxBeds
            });

            await LogAuditAsync("CREATE", "Slot", slotId, null, $"Created slot: {request.SlotName}");

            return CreatedAtAction(nameof(GetSlot), new { id = slotId },
                ApiResponse<int>.SuccessResponse(slotId, "Slot created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating slot");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error creating slot"));
        }
    }

    [HttpPut("slots/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSlot(int id, [FromBody] UpdateSlotRequest request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            var oldSlot = await connection.QueryFirstOrDefaultAsync<SlotConfiguration>(
                "SELECT * FROM Slots WHERE SlotID = @SlotID", new { SlotID = id });

            if (oldSlot == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Slot not found"));
            }

            var query = @"UPDATE Slots 
                         SET SlotName = @SlotName, StartTime = @StartTime, EndTime = @EndTime, 
                             MaxBeds = @MaxBeds, IsActive = @IsActive 
                         WHERE SlotID = @SlotID";
            
            await connection.ExecuteAsync(query, new
            {
                SlotID = id,
                request.SlotName,
                request.StartTime,
                request.EndTime,
                request.MaxBeds,
                request.IsActive
            });

            var oldValues = $"Name: {oldSlot.SlotName}, MaxBeds: {oldSlot.MaxBeds}";
            var newValues = $"Name: {request.SlotName}, MaxBeds: {request.MaxBeds}";
            await LogAuditAsync("UPDATE", "Slot", id, oldValues, newValues);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Slot updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating slot {SlotId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating slot"));
        }
    }

    [HttpDelete("slots/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSlot(int id)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check if slot has assignments
            var hasAssignments = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM BedAssignments WHERE SlotID = @SlotID", new { SlotID = id });

            if (hasAssignments > 0)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Cannot delete slot with existing bed assignments. Deactivate it instead."));
            }

            var slot = await connection.QueryFirstOrDefaultAsync<SlotConfiguration>(
                "SELECT * FROM Slots WHERE SlotID = @SlotID", new { SlotID = id });

            if (slot == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Slot not found"));
            }

            await connection.ExecuteAsync("DELETE FROM Slots WHERE SlotID = @SlotID", new { SlotID = id });

            await LogAuditAsync("DELETE", "Slot", id, $"Deleted slot: {slot.SlotName}", null);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Slot deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting slot {SlotId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting slot"));
        }
    }

    // Bed Configuration
    [HttpGet("beds/capacity")]
    public async Task<ActionResult<ApiResponse<List<BedCapacity>>>> GetBedCapacity()
    {
        try
        {
            using var connection = _context.CreateConnection();
            var query = @"
                SELECT 
                    s.SlotID,
                    s.SlotName,
                    s.MaxBeds,
                    COUNT(ba.BedID) as UsedBeds,
                    (s.MaxBeds - COUNT(ba.BedID)) as AvailableBeds,
                    CAST(COUNT(ba.BedID) * 100.0 / s.MaxBeds as decimal(5,2)) as OccupancyRate
                FROM Slots s
                LEFT JOIN BedAssignments ba ON s.SlotID = ba.SlotID AND ba.IsActive = 1
                WHERE s.IsActive = 1
                GROUP BY s.SlotID, s.SlotName, s.MaxBeds
                ORDER BY s.SlotID";
            
            var capacity = await connection.QueryAsync<BedCapacity>(query);
            return Ok(ApiResponse<List<BedCapacity>>.SuccessResponse(capacity.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bed capacity");
            return StatusCode(500, ApiResponse<List<BedCapacity>>.ErrorResponse("Error retrieving bed capacity"));
        }
    }

    [HttpPut("beds/capacity/{slotId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBedCapacity(int slotId, [FromBody] UpdateBedCapacityRequest request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check current usage
            var currentUsage = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM BedAssignments WHERE SlotID = @SlotID AND IsActive = 1",
                new { SlotID = slotId });

            if (request.MaxBeds < currentUsage)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    $"Cannot set capacity below current usage ({currentUsage} beds in use)"));
            }

            var oldCapacity = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MaxBeds FROM Slots WHERE SlotID = @SlotID", new { SlotID = slotId });

            await connection.ExecuteAsync(
                "UPDATE Slots SET MaxBeds = @MaxBeds WHERE SlotID = @SlotID",
                new { SlotID = slotId, request.MaxBeds });

            await LogAuditAsync("UPDATE_CAPACITY", "Slot", slotId, 
                $"Capacity: {oldCapacity}", $"Capacity: {request.MaxBeds}");

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Bed capacity updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bed capacity for slot {SlotId}", slotId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating bed capacity"));
        }
    }

    // System Parameters
    [HttpGet("parameters")]
    public async Task<ActionResult<ApiResponse<SystemParameters>>> GetSystemParameters()
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            var totalSlots = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM Slots WHERE IsActive = 1");
            var totalBeds = await connection.QueryFirstOrDefaultAsync<int>("SELECT SUM(MaxBeds) FROM Slots WHERE IsActive = 1");
            var totalPatients = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM Patients WHERE IsActive = 1");
            var totalStaff = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM Staff WHERE IsActive = 1");

            var parameters = new SystemParameters
            {
                TotalActiveSlots = totalSlots,
                TotalBedCapacity = totalBeds,
                TotalActivePatients = totalPatients,
                TotalActiveStaff = totalStaff,
                DatabaseVersion = "1.0",
                LastBackup = null // Would need backup tracking table
            };

            return Ok(ApiResponse<SystemParameters>.SuccessResponse(parameters));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system parameters");
            return StatusCode(500, ApiResponse<SystemParameters>.ErrorResponse("Error retrieving system parameters"));
        }
    }

    private async Task LogAuditAsync(string action, string entityType, int? entityId, string? oldValues, string? newValues)
    {
        try
        {
            var log = new Models.AuditLog
            {
                UserID = GetCurrentUserId(),
                Username = User.Identity?.Name ?? "System",
                Action = action,
                EntityType = entityType,
                EntityID = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                CreatedAt = DateTime.Now
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

public class SlotConfiguration
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty; // Changed to string for SQLite
    public string EndTime { get; set; } = string.Empty; // Changed to string for SQLite
    public int MaxBeds { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSlotRequest
{
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty; // Changed to string for SQLite
    public string EndTime { get; set; } = string.Empty; // Changed to string for SQLite
    public int MaxBeds { get; set; }
}

public class UpdateSlotRequest
{
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty; // Changed to string for SQLite
    public string EndTime { get; set; } = string.Empty; // Changed to string for SQLite
    public int MaxBeds { get; set; }
    public bool IsActive { get; set; }
}

public class BedCapacity
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int MaxBeds { get; set; }
    public int UsedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public decimal OccupancyRate { get; set; }
}

public class UpdateBedCapacityRequest
{
    public int MaxBeds { get; set; }
}

public class SystemParameters
{
    public int TotalActiveSlots { get; set; }
    public int TotalBedCapacity { get; set; }
    public int TotalActivePatients { get; set; }
    public int TotalActiveStaff { get; set; }
    public string DatabaseVersion { get; set; } = string.Empty;
    public DateTime? LastBackup { get; set; }
}
