using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly DapperContext _context;

    public AuditLogRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(AuditLog log)
    {
        var query = @"INSERT INTO AuditLogs (Username, Role, Action, EntityType, EntityID, Details, IPAddress, Timestamp)
                     VALUES (@Username, @Role, @Action, @EntityType, @EntityID, @Details, @IPAddress, GETUTCDATE());
                     SELECT CAST(SCOPE_IDENTITY() AS INT)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, log);
    }

    public async Task<List<AuditLog>> GetAllAsync(int skip = 0, int take = 100)
    {
        var query = @"SELECT * FROM AuditLogs 
                     ORDER BY Timestamp DESC 
                     OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { Skip = skip, Take = take });
        return logs.ToList();
    }

    public async Task<List<AuditLog>> GetByUserAsync(int userId, int skip = 0, int take = 100)
    {
        var query = @"SELECT * FROM AuditLogs WHERE Username = (SELECT Username FROM Users WHERE UserID = @UserID)
                     ORDER BY Timestamp DESC 
                     OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { UserID = userId, Skip = skip, Take = take });
        return logs.ToList();
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId)
    {
        var query = @"SELECT * FROM AuditLogs 
                     WHERE EntityType = @EntityType AND EntityID = @EntityID 
                     ORDER BY Timestamp DESC";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { EntityType = entityType, EntityID = entityId });
        return logs.ToList();
    }

    public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var query = @"SELECT * FROM AuditLogs 
                     WHERE Timestamp BETWEEN @StartDate AND @EndDate 
                     ORDER BY Timestamp DESC";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { StartDate = startDate, EndDate = endDate });
        return logs.ToList();
    }

    public async Task<List<UserActivity>> GetUserActivitySummaryAsync(DateTime? startDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var query = @"SELECT 
                         Username,
                         COALESCE((SELECT TOP 1 Role FROM Users WHERE Username = AuditLogs.Username), 'Unknown') as Role,
                         MAX(Action) as LastAction,
                         MAX(Timestamp) as LastActivityTime,
                         COUNT(*) as ActionCount
                     FROM AuditLogs
                     WHERE Timestamp >= @StartDate
                     GROUP BY Username
                     ORDER BY LastActivityTime DESC";
        using var connection = _context.CreateConnection();
        var activities = await connection.QueryAsync<UserActivity>(query, new { StartDate = start });
        return activities.ToList();
    }

    public async Task<List<UserActivity>> GetUserActivitySummaryAsync(DateTime startDate, DateTime endDate)
    {
        var query = @"SELECT 
                         Username,
                         COALESCE((SELECT TOP 1 Role FROM Users WHERE Username = AuditLogs.Username), 'Unknown') as Role,
                         MAX(Action) as LastAction,
                         MAX(Timestamp) as LastActivityTime,
                         COUNT(*) as ActionCount
                     FROM AuditLogs
                     WHERE Timestamp BETWEEN @StartDate AND @EndDate
                     GROUP BY Username
                     ORDER BY LastActivityTime DESC";
        using var connection = _context.CreateConnection();
        var activities = await connection.QueryAsync<UserActivity>(query, new { StartDate = startDate, EndDate = endDate });
        return activities.ToList();
    }

    public async Task<List<AuditLog>> GetLoginHistoryAsync(int skip = 0, int take = 100)
    {
        var query = @"SELECT * FROM AuditLogs 
                     WHERE Action IN ('LOGIN', 'LOGOUT') 
                     ORDER BY Timestamp DESC 
                     LIMIT @Take OFFSET @Skip";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { Skip = skip, Take = take });
        return logs.ToList();
    }

    public async Task<List<AuditLog>> GetLoginHistoryAsync(int? userId, DateTime startDate)
    {
        var query = userId.HasValue 
            ? @"SELECT * FROM AuditLogs 
                WHERE Action IN ('LOGIN', 'LOGOUT') 
                AND UserID = @UserID
                AND Timestamp >= @StartDate
                ORDER BY Timestamp DESC"
            : @"SELECT * FROM AuditLogs 
                WHERE Action IN ('LOGIN', 'LOGOUT') 
                AND Timestamp >= @StartDate
                ORDER BY Timestamp DESC";
        using var connection = _context.CreateConnection();
        var logs = await connection.QueryAsync<AuditLog>(query, new { UserID = userId, StartDate = startDate });
        return logs.ToList();
    }
}
