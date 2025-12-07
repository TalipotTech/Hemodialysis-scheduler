using HDScheduler.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace HDScheduler.API;

public class DatabaseInitializer
{
    private readonly DapperContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(DapperContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeSlotsAsync()
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check if Slots table has data
            var count = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM Slots");
            
            if (count == 0)
            {
                _logger.LogInformation("Initializing default time slots...");
                
                var insertQuery = @"
                    INSERT INTO Slots (SlotID, SlotName, StartTime, EndTime, MaxBeds, IsActive) 
                    VALUES 
                        (1, 'Morning Shift', '06:00', '10:00', 10, 1),
                        (2, 'Afternoon Shift', '11:00', '15:00', 10, 1),
                        (3, 'Evening Shift', '16:00', '20:00', 10, 1),
                        (4, 'Night Shift', '21:00', '01:00', 10, 1)";
                
                await connection.ExecuteAsync(insertQuery);
                _logger.LogInformation("✓ Default time slots initialized: Morning, Afternoon, Evening, Night (10 beds each)");
            }
            else
            {
                _logger.LogInformation($"Slots table already has {count} slot(s)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing slots");
        }
    }
}
