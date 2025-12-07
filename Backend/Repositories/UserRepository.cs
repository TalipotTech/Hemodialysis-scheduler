using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var query = "SELECT * FROM Users WHERE Username = @Username";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        var query = "SELECT * FROM Users WHERE UserID = @UserID AND IsActive = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { UserID = userId });
    }

    public async Task<List<User>> GetAllAsync()
    {
        var query = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY Role, Username";
        using var connection = _context.CreateConnection();
        var users = await connection.QueryAsync<User>(query);
        return users.ToList();
    }

    public async Task<int> CreateAsync(User user)
    {
        var query = @"INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
                     VALUES (@Username, @PasswordHash, @Role, @IsActive, GETUTCDATE());
                     SELECT CAST(SCOPE_IDENTITY() AS INT)";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var query = @"UPDATE Users SET 
                     Username = @Username, 
                     Role = @Role, 
                     IsActive = @IsActive 
                     WHERE UserID = @UserID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, user);
        return affected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        var query = "UPDATE Users SET LastLogin = GETUTCDATE() WHERE UserID = @UserID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { UserID = userId });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var query = "DELETE FROM Users WHERE UserID = @UserID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { UserID = userId });
        return affected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { UserID = userId, PasswordHash = newPasswordHash });
        return affected > 0;
    }

    public async Task<bool> ToggleActiveStatusAsync(int userId, bool isActive)
    {
        var query = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { UserID = userId, IsActive = isActive });
        return affected > 0;
    }
}
