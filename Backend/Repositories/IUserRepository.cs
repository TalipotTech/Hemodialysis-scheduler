using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int userId);
    Task<List<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> DeleteAsync(int userId);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    Task<bool> ToggleActiveStatusAsync(int userId, bool isActive);
}
