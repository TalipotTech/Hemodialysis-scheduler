using HDScheduler.API.Data;
using HDScheduler.API.Models;
using Dapper;
using System.Data;

namespace HDScheduler.API.Repositories;

public interface IPatientActivityRepository
{
    Task<int> CreateActivityAsync(PatientActivityLog activity);
    Task<List<PatientActivityLog>> GetPatientActivitiesAsync(int patientId);
    Task<List<PatientHistoryTimeline>> GetPatientHistoryTimelineAsync(int patientId);
}

public class PatientActivityRepository : IPatientActivityRepository
{
    private readonly DapperContext _context;

    public PatientActivityRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<int> CreateActivityAsync(PatientActivityLog activity)
    {
        var query = @"
            INSERT INTO PatientActivityLog (PatientID, ScheduleID, ActivityDate, ActivityType, Reason, Details, RecordedBy, OldDateTime, NewDateTime, CreatedAt)
            VALUES (@PatientID, @ScheduleID, @ActivityDate, @ActivityType, @Reason, @Details, @RecordedBy, @OldDateTime, @NewDateTime, @CreatedAt);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(query, activity);
    }

    public async Task<List<PatientActivityLog>> GetPatientActivitiesAsync(int patientId)
    {
        var query = @"
            SELECT * FROM PatientActivityLog 
            WHERE PatientID = @PatientID 
            ORDER BY ActivityDate DESC, CreatedAt DESC";

        using var connection = _context.CreateConnection();
        var activities = await connection.QueryAsync<PatientActivityLog>(query, new { PatientID = patientId });
        return activities.ToList();
    }

    public async Task<List<PatientHistoryTimeline>> GetPatientHistoryTimelineAsync(int patientId)
    {
        var query = @"
            -- Combine completed sessions and patient activities into one timeline
            SELECT 
                COALESCE(h.SessionDate, a.ActivityDate) as Date,
                CASE 
                    WHEN h.ScheduleID IS NOT NULL THEN 'SESSION_COMPLETED'
                    ELSE a.ActivityType
                END as EventType,
                COALESCE(h.SessionStatus, a.ActivityType) as Status,
                COALESCE(a.Details, 'Session completed') as Details,
                a.Reason,
                s.SlotName,
                h.BedNumber,
                a.RecordedBy
            FROM (
                -- Get completed sessions from HDSchedule
                SELECT 
                    ScheduleID,
                    SessionDate,
                    SessionStatus,
                    BedNumber
                FROM HDSchedule
                WHERE PatientID = @PatientID 
                    AND SessionStatus IN ('Completed', 'Discharged')
            ) h
            FULL OUTER JOIN (
                -- Get all patient activities
                SELECT 
                    ScheduleID,
                    ActivityDate,
                    ActivityType,
                    Reason,
                    Details,
                    RecordedBy
                FROM PatientActivityLog
                WHERE PatientID = @PatientID
            ) a ON h.ScheduleID = a.ScheduleID
            LEFT JOIN Slots s ON h.ScheduleID = s.SlotID
            ORDER BY Date DESC";

        using var connection = _context.CreateConnection();
        var timeline = await connection.QueryAsync<PatientHistoryTimeline>(query, new { PatientID = patientId });
        return timeline.ToList();
    }
}
