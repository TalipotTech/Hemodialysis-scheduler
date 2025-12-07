using HDScheduler.API.Models;
using Microsoft.Data.SqlClient;

namespace HDScheduler.API.Services;

/// <summary>
/// Service for managing equipment usage tracking and alerts
/// </summary>
public class EquipmentUsageService
{
    private readonly string _connectionString;

    public EquipmentUsageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string is required");
    }

    /// <summary>
    /// Check equipment usage for a patient and return status with alerts
    /// </summary>
    public async Task<List<EquipmentUsageStatus>> CheckEquipmentUsageAsync(int patientId, int? scheduleId = null)
    {
        var statuses = new List<EquipmentUsageStatus>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Get latest equipment usage counts
        int dialyserCount = 0;
        int bloodTubingCount = 0;

        if (scheduleId.HasValue)
        {
            // Get from specific schedule
            var query = @"
                SELECT DialyserReuseCount, BloodTubingReuse
                FROM HDSchedules
                WHERE ScheduleID = @ScheduleID AND PatientID = @PatientID";
            
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ScheduleID", scheduleId.Value);
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dialyserCount = reader.GetInt32(0);
                bloodTubingCount = reader.GetInt32(1);
            }
        }
        else
        {
            // Get from latest schedule for patient
            var query = @"
                SELECT TOP 1 DialyserReuseCount, BloodTubingReuse
                FROM HDSchedules
                WHERE PatientID = @PatientID
                ORDER BY SessionDate DESC, CreatedAt DESC";
            
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dialyserCount = reader.GetInt32(0);
                bloodTubingCount = reader.GetInt32(1);
            }
        }

        // Check Dialyser status
        statuses.Add(CreateEquipmentStatus(
            "Dialyser",
            dialyserCount,
            EquipmentUsageLimits.DIALYSER_MAX_REUSE,
            EquipmentUsageLimits.DIALYSER_WARNING_THRESHOLD
        ));

        // Check Blood Tubing status
        statuses.Add(CreateEquipmentStatus(
            "Blood Tubing",
            bloodTubingCount,
            EquipmentUsageLimits.BLOOD_TUBING_MAX_REUSE,
            EquipmentUsageLimits.BLOOD_TUBING_WARNING_THRESHOLD
        ));

        return statuses;
    }

    /// <summary>
    /// Create equipment usage status with appropriate alert level
    /// </summary>
    private EquipmentUsageStatus CreateEquipmentStatus(
        string equipmentType, 
        int currentCount, 
        int maxLimit, 
        int warningThreshold)
    {
        var status = new EquipmentUsageStatus
        {
            EquipmentType = equipmentType,
            CurrentUsageCount = currentCount,
            MaxUsageLimit = maxLimit,
            RemainingUses = Math.Max(0, maxLimit - currentCount),
            UsagePercentage = Math.Round((double)currentCount / maxLimit * 100, 1)
        };

        // Determine status and message
        if (currentCount >= maxLimit)
        {
            status.Status = "Expired";
            status.RequiresReplacement = true;
            status.Message = $"⚠️ CRITICAL: {equipmentType} has reached maximum usage limit ({maxLimit} times). MUST be replaced before next session!";
        }
        else if (currentCount > maxLimit)
        {
            status.Status = "Expired";
            status.RequiresReplacement = true;
            status.Message = $"⛔ DANGER: {equipmentType} has EXCEEDED maximum usage limit! Current: {currentCount}, Max: {maxLimit}. Replace immediately!";
        }
        else if (currentCount >= warningThreshold)
        {
            status.Status = "Critical";
            status.RequiresReplacement = false;
            status.Message = $"⚠️ WARNING: {equipmentType} is nearing maximum usage. Current: {currentCount}/{maxLimit}. {status.RemainingUses} use(s) remaining. Please prepare replacement.";
        }
        else if (currentCount >= (warningThreshold - 1))
        {
            status.Status = "Warning";
            status.RequiresReplacement = false;
            status.Message = $"ℹ️ NOTICE: {equipmentType} usage at {currentCount}/{maxLimit}. {status.RemainingUses} use(s) remaining.";
        }
        else
        {
            status.Status = "OK";
            status.RequiresReplacement = false;
            status.Message = $"✓ {equipmentType} usage is normal ({currentCount}/{maxLimit}).";
        }

        return status;
    }

    /// <summary>
    /// Log equipment usage alert to database
    /// </summary>
    public async Task<int> LogEquipmentAlertAsync(EquipmentUsageAlert alert)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO EquipmentUsageAlerts 
            (PatientID, ScheduleID, EquipmentType, CurrentUsageCount, MaxUsageLimit, 
             Severity, AlertMessage, IsAcknowledged, CreatedAt)
            VALUES 
            (@PatientID, @ScheduleID, @EquipmentType, @CurrentUsageCount, @MaxUsageLimit,
             @Severity, @AlertMessage, @IsAcknowledged, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@PatientID", alert.PatientID);
        cmd.Parameters.AddWithValue("@ScheduleID", alert.ScheduleID ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EquipmentType", alert.EquipmentType);
        cmd.Parameters.AddWithValue("@CurrentUsageCount", alert.CurrentUsageCount);
        cmd.Parameters.AddWithValue("@MaxUsageLimit", alert.MaxUsageLimit);
        cmd.Parameters.AddWithValue("@Severity", alert.Severity);
        cmd.Parameters.AddWithValue("@AlertMessage", alert.AlertMessage);
        cmd.Parameters.AddWithValue("@IsAcknowledged", alert.IsAcknowledged);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Get all unacknowledged alerts for a patient
    /// </summary>
    public async Task<List<EquipmentUsageAlert>> GetUnacknowledgedAlertsAsync(int patientId)
    {
        var alerts = new List<EquipmentUsageAlert>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT AlertID, PatientID, ScheduleID, EquipmentType, CurrentUsageCount, 
                   MaxUsageLimit, Severity, AlertMessage, IsAcknowledged, CreatedAt
            FROM EquipmentUsageAlerts
            WHERE PatientID = @PatientID AND IsAcknowledged = 0
            ORDER BY CreatedAt DESC";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@PatientID", patientId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            alerts.Add(new EquipmentUsageAlert
            {
                AlertID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                ScheduleID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                EquipmentType = reader.GetString(3),
                CurrentUsageCount = reader.GetInt32(4),
                MaxUsageLimit = reader.GetInt32(5),
                Severity = reader.GetString(6),
                AlertMessage = reader.GetString(7),
                IsAcknowledged = reader.GetBoolean(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }

        return alerts;
    }

    /// <summary>
    /// Acknowledge an alert
    /// </summary>
    public async Task<bool> AcknowledgeAlertAsync(int alertId, string acknowledgedBy)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE EquipmentUsageAlerts
            SET IsAcknowledged = 1,
                AcknowledgedBy = @AcknowledgedBy,
                AcknowledgedAt = @AcknowledgedAt
            WHERE AlertID = @AlertID";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@AlertID", alertId);
        cmd.Parameters.AddWithValue("@AcknowledgedBy", acknowledgedBy);
        cmd.Parameters.AddWithValue("@AcknowledgedAt", DateTime.UtcNow);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Automatically check and create alerts for a schedule
    /// </summary>
    public async Task<List<EquipmentUsageAlert>> CheckAndCreateAlertsForScheduleAsync(int patientId, int scheduleId)
    {
        var createdAlerts = new List<EquipmentUsageAlert>();
        var statuses = await CheckEquipmentUsageAsync(patientId, scheduleId);

        foreach (var status in statuses)
        {
            // Only create alerts for Warning, Critical, or Expired status
            if (status.Status == "Warning" || status.Status == "Critical" || status.Status == "Expired")
            {
                var alert = new EquipmentUsageAlert
                {
                    PatientID = patientId,
                    ScheduleID = scheduleId,
                    EquipmentType = status.EquipmentType,
                    CurrentUsageCount = status.CurrentUsageCount,
                    MaxUsageLimit = status.MaxUsageLimit,
                    Severity = status.Status,
                    AlertMessage = status.Message ?? string.Empty,
                    IsAcknowledged = false,
                    CreatedAt = DateTime.UtcNow
                };

                alert.AlertID = await LogEquipmentAlertAsync(alert);
                createdAlerts.Add(alert);
            }
        }

        return createdAlerts;
    }
}
