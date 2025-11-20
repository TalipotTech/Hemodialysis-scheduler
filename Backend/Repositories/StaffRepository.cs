using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly DapperContext _context;

    public StaffRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<Staff>> GetAllAsync()
    {
        var query = @"SELECT s.*, sl.SlotName as AssignedSlotName 
                     FROM Staff s
                     LEFT JOIN Slots sl ON s.AssignedSlot = sl.SlotID
                     ORDER BY s.Role, s.Name";
        using var connection = _context.CreateConnection();
        var staff = await connection.QueryAsync<Staff>(query);
        return staff.ToList();
    }

    public async Task<List<Staff>> GetActiveAsync()
    {
        var query = "SELECT * FROM Staff WHERE IsActive = 1 ORDER BY Role, Name";
        using var connection = _context.CreateConnection();
        var staff = await connection.QueryAsync<Staff>(query);
        return staff.ToList();
    }

    public async Task<Staff?> GetByIdAsync(int staffId)
    {
        var query = "SELECT * FROM Staff WHERE StaffID = @StaffID";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Staff>(query, new { StaffID = staffId });
    }

    public async Task<List<Staff>> GetByRoleAsync(string role)
    {
        var query = "SELECT * FROM Staff WHERE Role = @Role AND IsActive = 1 ORDER BY Name";
        using var connection = _context.CreateConnection();
        var staff = await connection.QueryAsync<Staff>(query, new { Role = role });
        return staff.ToList();
    }

    public async Task<List<Staff>> GetBySlotAsync(int slotId)
    {
        var query = "SELECT * FROM Staff WHERE AssignedSlot = @SlotID AND IsActive = 1 ORDER BY Role, Name";
        using var connection = _context.CreateConnection();
        var staff = await connection.QueryAsync<Staff>(query, new { SlotID = slotId });
        return staff.ToList();
    }

    public async Task<int> CreateAsync(Staff staff)
    {
        var query = @"INSERT INTO Staff (Name, Role, ContactNumber, StaffSpecialization, AssignedSlot, IsActive, CreatedAt)
                     VALUES (@Name, @Role, @ContactNumber, @StaffSpecialization, @AssignedSlot, @IsActive, @CreatedAt);
                     SELECT last_insert_rowid()";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, staff);
    }

    public async Task<bool> UpdateAsync(Staff staff)
    {
        var query = @"UPDATE Staff SET 
                     Name = @Name,
                     Role = @Role,
                     ContactNumber = @ContactNumber,
                     StaffSpecialization = @StaffSpecialization,
                     AssignedSlot = @AssignedSlot,
                     IsActive = @IsActive
                     WHERE StaffID = @StaffID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, staff);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int staffId)
    {
        var query = "DELETE FROM Staff WHERE StaffID = @StaffID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { StaffID = staffId });
        return affected > 0;
    }

    public async Task<bool> AssignToSlotAsync(int staffId, int? slotId)
    {
        var query = "UPDATE Staff SET AssignedSlot = @SlotID WHERE StaffID = @StaffID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { StaffID = staffId, SlotID = slotId });
        return affected > 0;
    }

    public async Task<bool> ToggleActiveStatusAsync(int staffId, bool isActive)
    {
        var query = "UPDATE Staff SET IsActive = @IsActive WHERE StaffID = @StaffID";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { StaffID = staffId, IsActive = isActive });
        return affected > 0;
    }
}
