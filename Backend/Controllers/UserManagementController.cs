using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<UserManagementController> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            // Remove password hashes from response
            users.ForEach(u => u.PasswordHash = "");
            return Ok(ApiResponse<List<User>>.SuccessResponse(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse<List<User>>.ErrorResponse("Error retrieving users"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found"));
            }
            user.PasswordHash = ""; // Remove password hash
            return Ok(ApiResponse<User>.SuccessResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Error retrieving user"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<int>.ErrorResponse("Username already exists"));
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var userId = await _userRepository.CreateAsync(user);

            // Log the action
            await LogAuditAsync("CREATE", "User", userId, null, $"Created user: {request.Username} with role: {request.Role}");

            return CreatedAtAction(nameof(GetUser), new { id = userId },
                ApiResponse<int>.SuccessResponse(userId, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error creating user"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            var oldValues = $"Username: {user.Username}, Role: {user.Role}";

            user.Username = request.Username;
            user.Role = request.Role;

            var result = await _userRepository.UpdateAsync(user);

            if (result)
            {
                var newValues = $"Username: {request.Username}, Role: {request.Role}";
                await LogAuditAsync("UPDATE", "User", id, oldValues, newValues);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "User updated successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update user"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating user"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            // Don't allow deleting yourself
            if (User.Identity?.Name == user.Username)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete your own account"));
            }

            var result = await _userRepository.DeleteAsync(id);

            if (result)
            {
                await LogAuditAsync("DELETE", "User", id, $"Deleted user: {user.Username}", null);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete user"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting user"));
        }
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
            var result = await _userRepository.UpdatePasswordAsync(id, newPasswordHash);

            if (result)
            {
                await LogAuditAsync("PASSWORD_RESET", "User", id, null, $"Password reset for user: {user.Username}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Password reset successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to reset password"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error resetting password"));
        }
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleUserStatus(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            // Don't allow disabling yourself
            if (User.Identity?.Name == user.Username)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot disable your own account"));
            }

            var newStatus = !user.IsActive;
            var result = await _userRepository.ToggleActiveStatusAsync(id, newStatus);

            if (result)
            {
                var action = newStatus ? "ENABLED" : "DISABLED";
                await LogAuditAsync(action, "User", id, null, $"{action} user: {user.Username}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, $"User {(newStatus ? "enabled" : "disabled")} successfully"));
            }

            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update user status"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating user status"));
        }
    }

    private async Task LogAuditAsync(string action, string entityType, int? entityId, string? oldValues, string? newValues)
    {
        try
        {
            var log = new AuditLog
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

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
