using HDScheduler.API.Models;

namespace HDScheduler.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(string username, string password);
    string GenerateJwtToken(User user);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> ValidateTokenAsync(string token);
}
