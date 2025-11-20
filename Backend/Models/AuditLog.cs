namespace HDScheduler.API.Models;

public class AuditLog
{
    public int AuditLogID { get; set; }
    public int? UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    public string EntityType { get; set; } = string.Empty; // Patient, User, Schedule, etc.
    public int? EntityID { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserActivity
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string LastAction { get; set; } = string.Empty;
    public DateTime LastActivityTime { get; set; }
    public int ActionCount { get; set; }
}
