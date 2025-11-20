using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IAuditLogRepository
{
    Task<int> CreateAsync(AuditLog log);
    Task<List<AuditLog>> GetAllAsync(int skip = 0, int take = 100);
    Task<List<AuditLog>> GetByUserAsync(int userId, int skip = 0, int take = 100);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<UserActivity>> GetUserActivitySummaryAsync(DateTime? startDate = null);
    Task<List<UserActivity>> GetUserActivitySummaryAsync(DateTime startDate, DateTime endDate);
    Task<List<AuditLog>> GetLoginHistoryAsync(int skip = 0, int take = 100);
    Task<List<AuditLog>> GetLoginHistoryAsync(int? userId, DateTime startDate);
}
