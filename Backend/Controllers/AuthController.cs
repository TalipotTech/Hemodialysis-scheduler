using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Services;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Username and password are required"));
            }

            var result = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (result == null)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid username or password"));
            }

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse("An error occurred during login"));
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken([FromBody] string token)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(ApiResponse<bool>.SuccessResponse(isValid, isValid ? "Token is valid" : "Token is invalid"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred during token validation"));
        }
    }

    [HttpGet("user-info")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetUserInfo()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(ApiResponse<UserInfo>.ErrorResponse("User not authenticated"));
            }

            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(ApiResponse<UserInfo>.ErrorResponse("User not found"));
            }

            var userInfo = new UserInfo
            {
                Username = user.Username,
                Role = user.Role,
                Name = user.Username
            };

            return Ok(ApiResponse<UserInfo>.SuccessResponse(userInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return StatusCode(500, ApiResponse<UserInfo>.ErrorResponse("An error occurred while retrieving user information"));
        }
    }

    // Temporary endpoint to generate password hashes - REMOVE IN PRODUCTION
    [HttpGet("generate-hash/{password}")]
    public ActionResult<string> GenerateHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
        return Ok(new { password, hash });
    }
}
