namespace HDScheduler.API.Models;

public class AuditLog
{
    public int LogID { get; set; }
    public DateTime Timestamp { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    public string? EntityType { get; set; }
    public int? EntityID { get; set; }
    public string? Details { get; set; }
    public string? IPAddress { get; set; }
}

public class UserActivity
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string LastAction { get; set; } = string.Empty;
    public DateTime LastActivityTime { get; set; }
    public int ActionCount { get; set; }
}
