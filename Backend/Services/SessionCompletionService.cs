using HDScheduler.API.Repositories;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Services;

/// <summary>
/// Background service that automatically marks sessions as "Ready-For-Discharge" 
/// when the prescribed treatment duration has elapsed, and auto-discharges patients
/// after 5 hours from treatment start time
/// </summary>
public class SessionCompletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCompletionService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
    private const int AUTO_DISCHARGE_HOURS = 5; // Auto-discharge after 5 hours

    public SessionCompletionService(
        IServiceProvider serviceProvider,
        ILogger<SessionCompletionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Completion Service started (Auto-discharge after {Hours} hours)", AUTO_DISCHARGE_HOURS);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndUpdateCompletedSessionsAsync();
                await AutoDischargeExpiredSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Session Completion Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Session Completion Service stopped");
    }

    private async Task CheckAndUpdateCompletedSessionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DapperContext>();

        using var connection = context.CreateConnection();

        // Find active sessions where prescribed duration has elapsed
        var query = @"
            SELECT ScheduleID, PatientID, TreatmentStartTime, PrescribedDuration
            FROM HDSchedule
            WHERE SessionStatus = 'Active'
              AND TreatmentStartTime IS NOT NULL
              AND PrescribedDuration IS NOT NULL
              AND datetime('now') >= datetime(TreatmentStartTime, '+' || CAST(PrescribedDuration AS TEXT) || ' hours')
              AND IsDischarged = 0";

        var completedSessions = (await connection.QueryAsync<CompletedSessionDto>(query)).ToList();

        foreach (var session in completedSessions)
        {
            // Calculate completion time
            var completionTime = DateTime.Parse(session.TreatmentStartTime)
                .AddHours((double)session.PrescribedDuration);

            // Update session status to Ready-For-Discharge
            var updateQuery = @"
                UPDATE HDSchedule
                SET SessionStatus = 'Ready-For-Discharge',
                    TreatmentCompletionTime = @CompletionTime,
                    UpdatedAt = datetime('now')
                WHERE ScheduleID = @ScheduleID";

            await connection.ExecuteAsync(updateQuery, new
            {
                ScheduleID = session.ScheduleID,
                CompletionTime = completionTime.ToString("yyyy-MM-dd HH:mm:ss")
            });

            _logger.LogInformation(
                "Session {ScheduleID} for Patient {PatientID} marked as Ready-For-Discharge at {CompletionTime}",
                session.ScheduleID,
                session.PatientID,
                completionTime);
        }

        if (completedSessions.Count > 0)
        {
            _logger.LogInformation(
                "Marked {Count} session(s) as Ready-For-Discharge",
                completedSessions.Count);
        }
    }

    private async Task AutoDischargeExpiredSessionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DapperContext>();

        using var connection = context.CreateConnection();

        // Find active sessions where 5 hours have passed since treatment start
        var query = @"
            SELECT ScheduleID, PatientID, TreatmentStartTime, SessionDate, SlotID, BedNumber
            FROM HDSchedule
            WHERE TreatmentStartTime IS NOT NULL
              AND IsDischarged = 0
              AND IsMovedToHistory = 0
              AND datetime('now') >= datetime(TreatmentStartTime, '+' || @AutoDischargeHours || ' hours')";

        var expiredSessions = (await connection.QueryAsync<AutoDischargeSessionDto>(query, new
        {
            AutoDischargeHours = AUTO_DISCHARGE_HOURS
        })).ToList();

        foreach (var session in expiredSessions)
        {
            try
            {
                // Calculate discharge time (5 hours after start)
                var dischargeTime = DateTime.Parse(session.TreatmentStartTime)
                    .AddHours(AUTO_DISCHARGE_HOURS);

                // Update HDSchedule: Mark as discharged and moved to history
                var updateScheduleQuery = @"
                    UPDATE HDSchedule
                    SET IsMovedToHistory = 1,
                        IsDischarged = 1,
                        SessionStatus = 'Discharged',
                        DischargeTime = @DischargeTime,
                        UpdatedAt = datetime('now')
                    WHERE ScheduleID = @ScheduleID";

                await connection.ExecuteAsync(updateScheduleQuery, new
                {
                    ScheduleID = session.ScheduleID,
                    DischargeTime = dischargeTime.ToString("yyyy-MM-dd HH:mm:ss")
                });

                // AUTO-INCREMENT: Update equipment counters and total dialysis count
                // Also check if equipment needs reset (reached max usage) and increment purchase counter
                var updateEquipmentQuery = @"
                    UPDATE Patients
                    SET DialyserCount = CASE 
                        WHEN DialyserCount >= 7 THEN 1  -- Reset to 1 when max reached
                        ELSE DialyserCount + 1 
                    END,
                    BloodTubingCount = CASE 
                        WHEN BloodTubingCount >= 12 THEN 1  -- Reset to 1 when max reached
                        ELSE BloodTubingCount + 1 
                    END,
                    DialysersPurchased = CASE 
                        WHEN DialyserCount >= 7 THEN DialysersPurchased + 1  -- Increment when purchasing new
                        ELSE DialysersPurchased 
                    END,
                    BloodTubingPurchased = CASE 
                        WHEN BloodTubingCount >= 12 THEN BloodTubingPurchased + 1  -- Increment when purchasing new
                        ELSE BloodTubingPurchased 
                    END,
                    TotalDialysisCompleted = TotalDialysisCompleted + 1,
                    UpdatedAt = datetime('now')
                    WHERE PatientID = @PatientID";

                await connection.ExecuteAsync(updateEquipmentQuery, new
                {
                    PatientID = session.PatientID
                });

                // Release bed in BedAssignments
                var releaseBedQuery = @"
                    UPDATE BedAssignments
                    SET IsActive = 0,
                        DischargedAt = datetime('now')
                    WHERE PatientID = @PatientID
                      AND SlotID = @SlotID
                      AND BedNumber = @BedNumber
                      AND date(AssignmentDate) = date(@SessionDate)
                      AND IsActive = 1";

                await connection.ExecuteAsync(releaseBedQuery, new
                {
                    PatientID = session.PatientID,
                    SlotID = session.SlotID,
                    BedNumber = session.BedNumber,
                    SessionDate = session.SessionDate
                });

                _logger.LogInformation(
                    "Auto-discharged Patient {PatientID} from Schedule {ScheduleID} after {Hours} hours. " +
                    "Session started: {StartTime}, Auto-discharged: {DischargeTime}",
                    session.PatientID,
                    session.ScheduleID,
                    AUTO_DISCHARGE_HOURS,
                    session.TreatmentStartTime,
                    dischargeTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to auto-discharge Schedule {ScheduleID} for Patient {PatientID}",
                    session.ScheduleID,
                    session.PatientID);
            }
        }

        if (expiredSessions.Count > 0)
        {
            _logger.LogInformation(
                "Auto-discharged {Count} patient(s) after {Hours} hours",
                expiredSessions.Count,
                AUTO_DISCHARGE_HOURS);
        }
    }
}

// DTO for query results
internal class CompletedSessionDto
{
    public int ScheduleID { get; set; }
    public int PatientID { get; set; }
    public string TreatmentStartTime { get; set; } = string.Empty;
    public decimal PrescribedDuration { get; set; }
}

internal class AutoDischargeSessionDto
{
    public int ScheduleID { get; set; }
    public int PatientID { get; set; }
    public string TreatmentStartTime { get; set; } = string.Empty;
    public string SessionDate { get; set; } = string.Empty;
    public int? SlotID { get; set; }
    public int? BedNumber { get; set; }
}
