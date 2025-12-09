using Dapper;
using HDScheduler.API.Data;
using HDScheduler.API.DTOs;
using HDScheduler.API.Models;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HDScheduler.API.Services.AI
{
    public interface IAIRepository
    {
        Task<AISettings?> GetSettingsAsync();
        Task<int> CreateSettingsAsync(AISettings settings);
        Task UpdateSettingsAsync(AISettings settings);
        Task<int> LogUsageAsync(AIUsageLog log);
        Task<List<AIUsageLog>> GetUsageLogsAsync(DateTime fromDate);
        Task<Patient?> GetPatientAsync(int patientId);
        Task<List<SlotAvailability>> GetAvailableSlotsAsync(DateTime date);
        Task<List<SavedPrompt>> GetSavedPromptsAsync();
        Task<int> SavePromptAsync(string promptText, string? category);
        Task IncrementPromptUsageAsync(int promptId);
        Task DeletePromptAsync(int promptId);
    }
    
    public class AIRepository : IAIRepository
    {
        private readonly DapperContext _context;
        
        public AIRepository(DapperContext context)
        {
            _context = context;
        }
        
        public async Task<AISettings?> GetSettingsAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AISettings>(
                "SELECT TOP 1 * FROM AISettings ORDER BY Id");
        }
        
        public async Task<int> CreateSettingsAsync(AISettings settings)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                INSERT INTO AISettings (AIEnabled, AIProvider, DailyCostLimit, MonthlyCostLimit,
                    EnableSchedulingRecommendations, EnableNaturalLanguageQueries, EnablePredictiveAnalytics,
                    CurrentDailyCost, CurrentMonthlyCost, TodayRequestCount, MonthRequestCount,
                    LastDailyReset, LastMonthlyReset, EncryptedApiKey, LastUpdated, UpdatedBy)
                VALUES (@AIEnabled, @AIProvider, @DailyCostLimit, @MonthlyCostLimit,
                    @EnableSchedulingRecommendations, @EnableNaturalLanguageQueries, @EnablePredictiveAnalytics,
                    @CurrentDailyCost, @CurrentMonthlyCost, @TodayRequestCount, @MonthRequestCount,
                    @LastDailyReset, @LastMonthlyReset, @EncryptedApiKey, @LastUpdated, @UpdatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, settings);
        }
        
        public async Task UpdateSettingsAsync(AISettings settings)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                UPDATE AISettings SET
                    AIEnabled = @AIEnabled,
                    AIProvider = @AIProvider,
                    DailyCostLimit = @DailyCostLimit,
                    MonthlyCostLimit = @MonthlyCostLimit,
                    EnableSchedulingRecommendations = @EnableSchedulingRecommendations,
                    EnableNaturalLanguageQueries = @EnableNaturalLanguageQueries,
                    EnablePredictiveAnalytics = @EnablePredictiveAnalytics,
                    CurrentDailyCost = @CurrentDailyCost,
                    CurrentMonthlyCost = @CurrentMonthlyCost,
                    TodayRequestCount = @TodayRequestCount,
                    MonthRequestCount = @MonthRequestCount,
                    LastDailyReset = @LastDailyReset,
                    LastMonthlyReset = @LastMonthlyReset,
                    EncryptedApiKey = @EncryptedApiKey,
                    LastUpdated = @LastUpdated,
                    UpdatedBy = @UpdatedBy
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, settings);
        }
        
        public async Task<int> LogUsageAsync(AIUsageLog log)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                INSERT INTO AIUsageLogs (Timestamp, Provider, RequestType, InputTokens, OutputTokens,
                    TotalTokens, Cost, ProcessingTimeMs, Success, ErrorMessage, UserId, Metadata)
                VALUES (@Timestamp, @Provider, @RequestType, @InputTokens, @OutputTokens,
                    @TotalTokens, @Cost, @ProcessingTimeMs, @Success, @ErrorMessage, @UserId, @Metadata);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, log);
        }
        
        public async Task<List<AIUsageLog>> GetUsageLogsAsync(DateTime fromDate)
        {
            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<AIUsageLog>(
                "SELECT * FROM AIUsageLogs WHERE Timestamp >= @FromDate ORDER BY Timestamp DESC",
                new { FromDate = fromDate })).ToList();
        }
        
        public async Task<Patient?> GetPatientAsync(int patientId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Patient>(
                "SELECT * FROM Patients WHERE PatientID = @PatientId",
                new { PatientId = patientId });
        }
        
        public async Task<List<SlotAvailability>> GetAvailableSlotsAsync(DateTime date)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT 
                    s.SlotID as SlotId,
                    CONVERT(VARCHAR(5), s.StartTime, 108) + ' - ' + CONVERT(VARCHAR(5), s.EndTime, 108) as TimeDescription,
                    10 - ISNULL(COUNT(ba.BedNumber), 0) as AvailableBeds,
                    10 as TotalBeds
                FROM Slots s
                LEFT JOIN BedAssignments ba ON s.SlotID = ba.SlotID 
                    AND CAST(ba.AssignmentDate AS DATE) = @Date
                    AND ba.DischargedAt IS NULL
                GROUP BY s.SlotID, s.StartTime, s.EndTime
                ORDER BY s.SlotID";
            
            return (await connection.QueryAsync<SlotAvailability>(sql, new { Date = date.Date })).ToList();
        }

        public async Task<List<SavedPrompt>> GetSavedPromptsAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT Id, PromptText, Category, UsageCount, LastUsedAt, CreatedAt
                FROM SavedPrompts
                WHERE IsDeleted = 0
                ORDER BY UsageCount DESC, LastUsedAt DESC";
            
            return (await connection.QueryAsync<SavedPrompt>(sql)).ToList();
        }

        public async Task<int> SavePromptAsync(string promptText, string? category)
        {
            using var connection = _context.CreateConnection();
            
            // Check if prompt already exists
            var existingPrompt = await connection.QueryFirstOrDefaultAsync<SavedPrompt>(
                "SELECT * FROM SavedPrompts WHERE PromptText = @PromptText AND IsDeleted = 0",
                new { PromptText = promptText });
            
            if (existingPrompt != null)
            {
                return existingPrompt.Id;
            }
            
            var sql = @"
                INSERT INTO SavedPrompts (PromptText, Category, UsageCount, CreatedAt, LastUsedAt, IsDeleted)
                VALUES (@PromptText, @Category, 0, GETDATE(), NULL, 0);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, new { PromptText = promptText, Category = category });
        }

        public async Task IncrementPromptUsageAsync(int promptId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                UPDATE SavedPrompts 
                SET UsageCount = UsageCount + 1,
                    LastUsedAt = GETDATE()
                WHERE Id = @PromptId";
            
            await connection.ExecuteAsync(sql, new { PromptId = promptId });
        }

        public async Task DeletePromptAsync(int promptId)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE SavedPrompts SET IsDeleted = 1 WHERE Id = @PromptId";
            await connection.ExecuteAsync(sql, new { PromptId = promptId });
        }
    }
}
