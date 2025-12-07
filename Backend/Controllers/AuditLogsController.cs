using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Allow all authenticated users to view audit logs
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogsController> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetAllLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var logs = await _auditLogRepository.GetAllAsync();
            
            // Simple pagination
            var pagedLogs = logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(pagedLogs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving audit logs"));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetLogsByUser(int userId)
    {
        try
        {
            var logs = await _auditLogRepository.GetByUserAsync(userId);
            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for user {UserId}", userId);
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving user logs"));
        }
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetLogsByEntity(string entityType, int entityId)
    {
        try
        {
            var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving entity logs"));
        }
    }

    [HttpGet("daterange")]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetLogsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var logs = await _auditLogRepository.GetByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for date range");
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving logs"));
        }
    }

    [HttpGet("activity-summary")]
    public async Task<ActionResult<ApiResponse<List<UserActivity>>>> GetActivitySummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            
            var summary = await _auditLogRepository.GetUserActivitySummaryAsync(start, end);
            return Ok(ApiResponse<List<UserActivity>>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity summary");
            return StatusCode(500, ApiResponse<List<UserActivity>>.ErrorResponse("Error retrieving activity summary"));
        }
    }

    [HttpGet("login-history")]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetLoginHistory(
        [FromQuery] int? userId = null,
        [FromQuery] int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days);
            var logs = await _auditLogRepository.GetLoginHistoryAsync(userId, startDate);
            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login history");
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving login history"));
        }
    }

    [HttpGet("actions/{action}")]
    public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetLogsByAction(string action)
    {
        try
        {
            var logs = await _auditLogRepository.GetAllAsync(0, 10000);
            var filtered = logs
                .Where(l => l.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(filtered));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for action {Action}", action);
            return StatusCode(500, ApiResponse<List<AuditLog>>.ErrorResponse("Error retrieving action logs"));
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<AuditStatistics>>> GetStatistics(
        [FromQuery] int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days);
            var allLogs = await _auditLogRepository.GetByDateRangeAsync(startDate, DateTime.Now);

            var statistics = new AuditStatistics
            {
                TotalActions = allLogs.Count,
                UniqueUsers = allLogs.Select(l => l.Username).Distinct().Count(),
                LoginCount = allLogs.Count(l => l.Action == "LOGIN"),
                CreateCount = allLogs.Count(l => l.Action == "CREATE"),
                UpdateCount = allLogs.Count(l => l.Action == "UPDATE"),
                DeleteCount = allLogs.Count(l => l.Action == "DELETE"),
                MostActiveUser = allLogs
                    .GroupBy(l => l.Username)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "N/A",
                MostCommonAction = allLogs
                    .GroupBy(l => l.Action)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "N/A",
                ActionsByDay = allLogs
                    .GroupBy(l => l.Timestamp.Date)
                    .Select(g => new DailyActionCount
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return Ok(ApiResponse<AuditStatistics>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating audit statistics");
            return StatusCode(500, ApiResponse<AuditStatistics>.ErrorResponse("Error calculating statistics"));
        }
    }
}

public class AuditStatistics
{
    public int TotalActions { get; set; }
    public int UniqueUsers { get; set; }
    public int LoginCount { get; set; }
    public int CreateCount { get; set; }
    public int UpdateCount { get; set; }
    public int DeleteCount { get; set; }
    public string MostActiveUser { get; set; } = string.Empty;
    public string MostCommonAction { get; set; } = string.Empty;
    public List<DailyActionCount> ActionsByDay { get; set; } = new();
}

public class DailyActionCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
