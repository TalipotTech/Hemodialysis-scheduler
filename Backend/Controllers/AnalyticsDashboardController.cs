using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsDashboardController : ControllerBase
    {
        private readonly IAnalyticsDashboardService _analyticsService;
        private readonly ILogger<AnalyticsDashboardController> _logger;

        public AnalyticsDashboardController(
            IAnalyticsDashboardService analyticsService,
            ILogger<AnalyticsDashboardController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get overall AI usage metrics
        /// </summary>
        [HttpGet("usage-metrics")]
        public async Task<IActionResult> GetUsageMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today;
                
                var metrics = await _analyticsService.GetUsageMetricsAsync(start, end);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage metrics");
                return StatusCode(500, new { error = "Failed to retrieve usage metrics" });
            }
        }

        /// <summary>
        /// Get AI cost analytics
        /// </summary>
        [HttpGet("cost-analytics")]
        public async Task<IActionResult> GetCostAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today;
                
                var analytics = await _analyticsService.GetCostAnalyticsAsync(start, end);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost analytics");
                return StatusCode(500, new { error = "Failed to retrieve cost analytics" });
            }
        }

        /// <summary>
        /// Get AI performance metrics
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics()
        {
            try
            {
                var performance = await _analyticsService.GetPerformanceMetricsAsync();
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics");
                return StatusCode(500, new { error = "Failed to retrieve performance metrics" });
            }
        }

        /// <summary>
        /// Get usage trends over time
        /// </summary>
        [HttpGet("trends")]
        public async Task<IActionResult> GetUsageTrends([FromQuery] int days = 30)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest(new { error = "Days must be between 1 and 365" });
                }

                var trends = await _analyticsService.GetUsageTrendsAsync(days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage trends");
                return StatusCode(500, new { error = "Failed to retrieve usage trends" });
            }
        }

        /// <summary>
        /// Get AI system health status
        /// </summary>
        [HttpGet("system-health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var health = await _analyticsService.GetSystemHealthAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health");
                return StatusCode(500, new { error = "Failed to retrieve system health" });
            }
        }

        /// <summary>
        /// Get feature usage statistics
        /// </summary>
        [HttpGet("feature-usage")]
        public async Task<IActionResult> GetFeatureUsage()
        {
            try
            {
                var usage = await _analyticsService.GetFeatureUsageAsync();
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature usage");
                return StatusCode(500, new { error = "Failed to retrieve feature usage" });
            }
        }

        /// <summary>
        /// Get cost projections
        /// </summary>
        [HttpGet("cost-projection")]
        public async Task<IActionResult> GetCostProjection([FromQuery] int days = 30)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest(new { error = "Days must be between 1 and 365" });
                }

                var projection = await _analyticsService.GetCostProjectionAsync(days);
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost projection");
                return StatusCode(500, new { error = "Failed to calculate cost projection" });
            }
        }
    }
}
