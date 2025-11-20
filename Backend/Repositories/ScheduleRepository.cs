using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.Models;

namespace HDScheduler.API.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly DapperContext _context;

    public ScheduleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<Slot>> GetAllSlotsAsync()
    {
        var query = "SELECT * FROM Slots ORDER BY SlotID";
        using var connection = _context.CreateConnection();
        var slots = await connection.QueryAsync<Slot>(query);
        return slots.ToList();
    }

    public async Task<Slot?> GetSlotByIdAsync(int slotId)
    {
        var query = "SELECT * FROM Slots WHERE SlotID = @SlotID";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Slot>(query, new { SlotID = slotId });
    }

    public async Task<List<BedAssignment>> GetAssignmentsByDateAsync(DateTime date)
    {
        var query = @"SELECT * FROM BedAssignments 
                     WHERE CAST(AssignmentDate AS DATE) = CAST(@Date AS DATE) 
                     AND IsActive = 1";
        using var connection = _context.CreateConnection();
        var assignments = await connection.QueryAsync<BedAssignment>(query, new { Date = date });
        return assignments.ToList();
    }

    public async Task<List<BedAssignment>> GetActiveAssignmentsBySlotAsync(int slotId, DateTime date)
    {
        var query = @"SELECT * FROM BedAssignments 
                     WHERE SlotID = @SlotID 
                     AND CAST(AssignmentDate AS DATE) = CAST(@Date AS DATE)
                     AND IsActive = 1";
        using var connection = _context.CreateConnection();
        var assignments = await connection.QueryAsync<BedAssignment>(query, new { SlotID = slotId, Date = date });
        return assignments.ToList();
    }

    public async Task<int> CreateAssignmentAsync(BedAssignment assignment)
    {
        var query = @"INSERT INTO BedAssignments 
                     (PatientID, SlotID, BedNumber, AssignmentDate, IsActive, CreatedAt)
                     VALUES 
                     (@PatientID, @SlotID, @BedNumber, @AssignmentDate, 1, datetime('now'));
                     SELECT last_insert_rowid()";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, assignment);
    }

    public async Task<bool> DischargeAssignmentAsync(int patientId)
    {
        var query = @"UPDATE BedAssignments 
                     SET IsActive = 0, DischargedAt = datetime('now') 
                     WHERE PatientID = @PatientID AND IsActive = 1";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { PatientID = patientId });
        return affected > 0;
    }

    public async Task<BedAvailability> GetBedAvailabilityAsync(int slotId, DateTime date)
    {
        var slotQuery = "SELECT * FROM Slots WHERE SlotID = @SlotID";
        var assignmentsQuery = @"SELECT BedNumber FROM BedAssignments 
                                WHERE SlotID = @SlotID 
                                AND CAST(AssignmentDate AS DATE) = CAST(@Date AS DATE)
                                AND IsActive = 1";
        
        using var connection = _context.CreateConnection();
        var slot = await connection.QueryFirstOrDefaultAsync<Slot>(slotQuery, new { SlotID = slotId });
        
        if (slot == null)
        {
            return new BedAvailability 
            { 
                SlotID = slotId, 
                SlotName = "Unknown", 
                TotalBeds = 0, 
                OccupiedBeds = 0, 
                AvailableBeds = 0 
            };
        }

        var occupiedBeds = await connection.QueryAsync<int>(assignmentsQuery, new { SlotID = slotId, Date = date });
        var occupiedBedsList = occupiedBeds.ToList();
        var allBeds = Enumerable.Range(1, slot.BedCapacity).ToList();
        var availableBeds = allBeds.Except(occupiedBedsList).ToList();

        return new BedAvailability
        {
            SlotID = slot.SlotID,
            SlotName = slot.SlotName,
            TotalBeds = slot.BedCapacity,
            OccupiedBeds = occupiedBedsList.Count,
            AvailableBeds = availableBeds.Count,
            AvailableBedNumbers = availableBeds
        };
    }
}
