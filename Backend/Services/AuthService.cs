using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;

namespace HDScheduler.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IAuditLogRepository auditLogRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            // Log failed login attempt
            if (user != null)
            {
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserID = user.UserID,
                    Username = username,
                    Action = "LOGIN_FAILED",
                    EntityType = "User",
                    EntityID = user.UserID,
                    CreatedAt = DateTime.Now
                });
            }
            return null;
        }

        // Update last login
        await _userRepository.UpdateLastLoginAsync(user.UserID);

        // Log successful login
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserID = user.UserID,
            Username = user.Username,
            Action = "LOGIN",
            EntityType = "User",
            EntityID = user.UserID,
            CreatedAt = DateTime.Now
        });

        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Username = user.Username,
                Role = user.Role,
                Name = user.Username // In a real app, you'd have a Name field in User table
            },
            ExpiresIn = 1800 // 30 minutes
        };
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "HDSchedulerAPI";
        var audience = jwtSettings["Audience"] ?? "HDSchedulerClient";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Using BCrypt for password verification
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
