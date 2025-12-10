using HDScheduler.API.Models;
using HDScheduler.API.Data;
using Dapper;

namespace HDScheduler.API.Services.AI
{
    public interface IAnalyticsDashboardService
    {
        Task<UsageMetrics> GetUsageMetricsAsync(DateTime startDate, DateTime endDate);
        Task<CostAnalytics> GetCostAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
        Task<UsageTrends> GetUsageTrendsAsync(int days);
        Task<SystemHealth> GetSystemHealthAsync();
        Task<FeatureUsage> GetFeatureUsageAsync();
        Task<CostProjection> GetCostProjectionAsync(int days);
    }

    public class AnalyticsDashboardService : IAnalyticsDashboardService
    {
        private readonly DapperContext _context;
        private readonly IAIRepository _aiRepository;
        private readonly ILogger<AnalyticsDashboardService> _logger;

        public AnalyticsDashboardService(
            DapperContext context,
            IAIRepository aiRepository,
            ILogger<AnalyticsDashboardService> logger)
        {
            _context = context;
            _aiRepository = aiRepository;
            _logger = logger;
        }

        public async Task<UsageMetrics> GetUsageMetricsAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();

            var query = @"SELECT 
                COUNT(*) as TotalRequests,
                SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as SuccessfulRequests,
                SUM(CASE WHEN Success = 0 THEN 1 ELSE 0 END) as FailedRequests,
                SUM(InputTokens) as TotalInputTokens,
                SUM(OutputTokens) as TotalOutputTokens,
                SUM(Cost) as TotalCost,
                AVG(CAST(ProcessingTimeMs AS FLOAT)) as AverageResponseTime
                FROM AIUsageLogs
                WHERE Timestamp >= @StartDate AND Timestamp < @EndDate";

            var stats = await connection.QueryFirstOrDefaultAsync<dynamic>(query,
                new { StartDate = startDate, EndDate = endDate });

            int total = stats?.TotalRequests ?? 0;
            int successful = stats?.SuccessfulRequests ?? 0;

            return new UsageMetrics
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = total,
                SuccessfulRequests = successful,
                FailedRequests = stats?.FailedRequests ?? 0,
                SuccessRate = total > 0 ? (successful * 100.0 / total) : 0,
                TotalInputTokens = stats?.TotalInputTokens ?? 0,
                TotalOutputTokens = stats?.TotalOutputTokens ?? 0,
                TotalCost = stats?.TotalCost ?? 0,
                AverageResponseTime = stats?.AverageResponseTime ?? 0
            };
        }

        public async Task<CostAnalytics> GetCostAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();

            var costQuery = @"SELECT 
                SUM(Cost) as TotalCost,
                SUM(InputTokens * 0.0005 / 1000) as InputCost,
                SUM(OutputTokens * 0.0015 / 1000) as OutputCost
                FROM AIUsageLogs
                WHERE Timestamp >= @StartDate AND Timestamp < @EndDate";

            var costs = await connection.QueryFirstOrDefaultAsync<dynamic>(costQuery,
                new { StartDate = startDate, EndDate = endDate });

            var dailyQuery = @"SELECT 
                CAST(Timestamp AS DATE) as Date,
                SUM(Cost) as Cost,
                COUNT(*) as RequestCount
                FROM AIUsageLogs
                WHERE Timestamp >= @StartDate AND Timestamp < @EndDate
                GROUP BY CAST(Timestamp AS DATE)
                ORDER BY Date";

            var dailyCosts = await connection.QueryAsync<DailyCostData>(dailyQuery,
                new { StartDate = startDate, EndDate = endDate });

            decimal totalCost = costs?.TotalCost ?? 0;
            int daysDiff = (int)(endDate - startDate).TotalDays;
            decimal avgDailyCost = daysDiff > 0 ? totalCost / daysDiff : totalCost;
            decimal projectedMonthlyCost = avgDailyCost * 30;

            return new CostAnalytics
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalCost = totalCost,
                InputCost = costs?.InputCost ?? 0,
                OutputCost = costs?.OutputCost ?? 0,
                AverageDailyCost = avgDailyCost,
                ProjectedMonthlyCost = projectedMonthlyCost,
                DailyCosts = dailyCosts.ToList()
            };
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            using var connection = _context.CreateConnection();

            var query = @"SELECT 
                AVG(CAST(ProcessingTimeMs AS FLOAT)) as AverageResponseTime,
                MIN(ProcessingTimeMs) as FastestRequestTime,
                MAX(ProcessingTimeMs) as SlowestRequestTime,
                COUNT(*) as TotalRequests
                FROM AIUsageLogs
                WHERE Timestamp >= DATEADD(day, -7, GETDATE())";

            var stats = await connection.QueryFirstOrDefaultAsync<dynamic>(query);

            return new PerformanceMetrics
            {
                AverageResponseTime = stats?.AverageResponseTime ?? 0,
                MedianResponseTime = stats?.AverageResponseTime ?? 0,
                P95ResponseTime = (stats?.AverageResponseTime ?? 0) * 1.5,
                P99ResponseTime = (stats?.AverageResponseTime ?? 0) * 2.0,
                TotalRequests = stats?.TotalRequests ?? 0,
                FastestRequestTime = stats?.FastestRequestTime ?? 0,
                SlowestRequestTime = stats?.SlowestRequestTime ?? 0,
                MeasuredAt = DateTime.UtcNow
            };
        }

        public async Task<UsageTrends> GetUsageTrendsAsync(int days)
        {
            using var connection = _context.CreateConnection();
            var startDate = DateTime.Today.AddDays(-days);

            var query = @"SELECT 
                CAST(Timestamp AS DATE) as Date,
                COUNT(*) as RequestCount,
                SUM(Cost) as Cost,
                SUM(InputTokens + OutputTokens) as TokensUsed
                FROM AIUsageLogs
                WHERE Timestamp >= @StartDate
                GROUP BY CAST(Timestamp AS DATE)
                ORDER BY Date";

            var trends = await connection.QueryAsync<TrendData>(query, new { StartDate = startDate });

            var trendList = trends.ToList();
            string direction = "Stable";
            double growthRate = 0;

            if (trendList.Count > 1)
            {
                var firstHalf = trendList.Take(trendList.Count / 2).Average(t => t.RequestCount);
                var secondHalf = trendList.Skip(trendList.Count / 2).Average(t => t.RequestCount);
                growthRate = firstHalf > 0 ? ((secondHalf - firstHalf) / firstHalf) * 100 : 0;
                direction = growthRate > 10 ? "Growing" : growthRate < -10 ? "Declining" : "Stable";
            }

            return new UsageTrends
            {
                Days = days,
                Trends = trendList,
                TrendDirection = direction,
                GrowthRate = growthRate
            };
        }

        public async Task<SystemHealth> GetSystemHealthAsync()
        {
            var settings = await _aiRepository.GetSettingsAsync();
            var todayStart = DateTime.Today;

            using var connection = _context.CreateConnection();

            var todayQuery = @"SELECT 
                COUNT(*) as RequestCount,
                SUM(Cost) as DailyCost
                FROM AIUsageLogs
                WHERE Timestamp >= @TodayStart";

            var todayStats = await connection.QueryFirstOrDefaultAsync<dynamic>(todayQuery,
                new { TodayStart = todayStart });

            decimal currentCost = todayStats?.DailyCost ?? 0;
            decimal costLimit = settings?.DailyCostLimit ?? 0;
            double utilization = costLimit > 0 ? (double)(currentCost / costLimit) * 100 : 0;

            var warnings = new List<string>();
            string status = "Healthy";

            if (settings == null || string.IsNullOrEmpty(settings.EncryptedApiKey))
            {
                status = "Not Configured";
                warnings.Add("AI API key not configured");
            }
            else if (!settings.AIEnabled)
            {
                status = "Disabled";
                warnings.Add("AI features are currently disabled");
            }
            else if (utilization > 90)
            {
                status = "Warning";
                warnings.Add($"Cost utilization at {utilization:F1}% - approaching daily limit");
            }
            else if (utilization > 100)
            {
                status = "Critical";
                warnings.Add("Daily cost limit exceeded");
            }

            return new SystemHealth
            {
                Status = status,
                AIEnabled = settings?.AIEnabled ?? false,
                APIKeyConfigured = !string.IsNullOrEmpty(settings?.EncryptedApiKey),
                DailyCostLimit = costLimit,
                CurrentDailyCost = currentCost,
                CostUtilization = utilization,
                TodaysRequests = todayStats?.RequestCount ?? 0,
                Warnings = warnings,
                CheckedAt = DateTime.UtcNow
            };
        }

        public async Task<FeatureUsage> GetFeatureUsageAsync()
        {
            using var connection = _context.CreateConnection();

            var featureQuery = @"SELECT 
                RequestType,
                COUNT(*) as Count
                FROM AIUsageLogs
                WHERE Timestamp >= DATEADD(day, -30, GETDATE())
                GROUP BY RequestType";

            var features = await connection.QueryAsync<dynamic>(featureQuery);

            var promptQuery = @"SELECT TOP 10 
                PromptText,
                UsageCount
                FROM SavedPrompts
                WHERE IsDeleted = 0
                ORDER BY UsageCount DESC";

            var prompts = await connection.QueryAsync<dynamic>(promptQuery);

            var popularQueries = prompts.ToDictionary(
                p => (string)p.PromptText,
                p => (int)p.UsageCount
            );

            return new FeatureUsage
            {
                NaturalQueryCount = features.FirstOrDefault(f => f.RequestType == "NaturalQuery")?.Count ?? 0,
                RiskAssessmentCount = features.FirstOrDefault(f => f.RequestType == "RiskAssessment")?.Count ?? 0,
                ReportGenerationCount = features.FirstOrDefault(f => f.RequestType == "ReportGeneration")?.Count ?? 0,
                SavedPromptsCount = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM SavedPrompts WHERE IsDeleted = 0"),
                PopularQueries = popularQueries,
                MeasuredAt = DateTime.UtcNow
            };
        }

        public async Task<CostProjection> GetCostProjectionAsync(int days)
        {
            using var connection = _context.CreateConnection();
            var last30Days = DateTime.Today.AddDays(-30);

            var avgQuery = @"SELECT 
                AVG(CAST(DailyCost AS FLOAT)) as AvgDailyCost
                FROM (
                    SELECT CAST(Timestamp AS DATE) as Date, SUM(Cost) as DailyCost
                    FROM AIUsageLogs
                    WHERE Timestamp >= @StartDate
                    GROUP BY CAST(Timestamp AS DATE)
                ) as DailyCosts";

            var avgCost = await connection.QueryFirstOrDefaultAsync<decimal>(avgQuery,
                new { StartDate = last30Days });

            var projections = new List<ProjectionData>();
            decimal cumulativeCost = 0;

            for (int i = 0; i < days; i++)
            {
                var projectionDate = DateTime.Today.AddDays(i);
                decimal estimatedCost = avgCost * (1 + (i * 0.01m)); // 1% growth per day
                cumulativeCost += estimatedCost;

                projections.Add(new ProjectionData
                {
                    Date = projectionDate,
                    EstimatedCost = estimatedCost,
                    CumulativeCost = cumulativeCost
                });
            }

            var settings = await _aiRepository.GetSettingsAsync();
            decimal monthlyProjection = avgCost * 30;
            decimal dailyLimit = settings?.DailyCostLimit ?? 0;

            string recommendation = monthlyProjection > (dailyLimit * 30)
                ? $"Projected monthly cost (${monthlyProjection:F2}) may exceed budget. Consider increasing daily limit or optimizing usage."
                : $"Projected monthly cost (${monthlyProjection:F2}) is within budget.";

            return new CostProjection
            {
                ProjectionDate = DateTime.Today,
                ProjectionDays = days,
                CurrentDailyCost = avgCost,
                ProjectedDailyCost = avgCost * 1.3m, // 30% growth estimate
                ProjectedTotalCost = cumulativeCost,
                DailyProjections = projections,
                Recommendation = recommendation
            };
        }
    }
}
