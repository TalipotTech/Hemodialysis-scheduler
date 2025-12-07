using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminFixController : ControllerBase
{
    private readonly DapperContext _context;
    
    public AdminFixController(DapperContext context)
    {
        _context = context;
    }
    
    [HttpPost("fix-session-dates")]
    public async Task<IActionResult> FixSessionDates()
    {
        using var connection = _context.CreateConnection();
        
        // Check how many need fixing
        var countQuery = @"
            SELECT COUNT(*) 
            FROM HDSchedule h
            INNER JOIN Patients p ON h.PatientID = p.PatientID
            WHERE p.HDStartDate IS NOT NULL 
                AND date(h.SessionDate) != date(p.HDStartDate)";
        
        var count = await connection.ExecuteScalarAsync<int>(countQuery);
        
        if (count == 0)
        {
            return Ok(new { message = "No sessions need fixing", count = 0 });
        }
        
        // Update the dates
        var updateQuery = @"
            UPDATE HDSchedule
            SET SessionDate = (
                SELECT HDStartDate 
                FROM Patients 
                WHERE Patients.PatientID = HDSchedule.PatientID
            ),
            UpdatedAt = GETUTCDATE()
            WHERE PatientID IN (
                SELECT PatientID 
                FROM Patients 
                WHERE HDStartDate IS NOT NULL
            )
            AND date(SessionDate) != date((
                SELECT HDStartDate 
                FROM Patients 
                WHERE Patients.PatientID = HDSchedule.PatientID
            ))";
        
        var rowsAffected = await connection.ExecuteAsync(updateQuery);
        
        return Ok(new { 
            message = "Session dates fixed successfully", 
            count = rowsAffected 
        });
    }
}
