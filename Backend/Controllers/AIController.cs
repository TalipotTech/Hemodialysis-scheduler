using HDScheduler.API.DTOs;
using HDScheduler.API.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HDScheduler.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;
        
        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get AI scheduling recommendation for a patient
        /// </summary>
        [HttpPost("schedule/recommend")]
        public async Task<ActionResult<AIScheduleRecommendation>> GetSchedulingRecommendation(
            [FromBody] AIScheduleRecommendationRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var recommendation = await _aiService.GetSchedulingRecommendationAsync(request, userId);
                return Ok(recommendation);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "AI operation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI scheduling recommendation");
                return StatusCode(500, new { error = "Failed to generate recommendation" });
            }
        }
        
        /// <summary>
        /// Get AI settings
        /// </summary>
        [HttpGet("settings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AISettingsDto>> GetSettings()
        {
            try
            {
                var settings = await _aiService.GetSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI settings");
                return StatusCode(500, new { error = "Failed to retrieve settings" });
            }
        }
        
        /// <summary>
        /// Update AI settings
        /// </summary>
        [HttpPut("settings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AISettingsDto>> UpdateSettings(
            [FromBody] UpdateAISettingsDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var settings = await _aiService.UpdateSettingsAsync(dto, userId);
                return Ok(settings);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid AI settings update");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI settings");
                return StatusCode(500, new { error = "Failed to update settings" });
            }
        }
        
        /// <summary>
        /// Get AI usage statistics
        /// </summary>
        [HttpGet("usage/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AIUsageStatsDto>> GetUsageStats()
        {
            try
            {
                var stats = await _aiService.GetUsageStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI usage stats");
                return StatusCode(500, new { error = "Failed to retrieve statistics" });
            }
        }
        
        /// <summary>
        /// Check if AI is currently enabled and available
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetStatus()
        {
            try
            {
                var isEnabled = await _aiService.IsAIEnabledAsync();
                var canUse = await _aiService.CheckCostLimitAsync();
                
                return Ok(new
                {
                    enabled = isEnabled,
                    available = isEnabled && canUse,
                    message = !isEnabled ? "AI features are disabled" :
                              !canUse ? "AI cost limit reached" :
                              "AI features are available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking AI status");
                return StatusCode(500, new { error = "Failed to check status" });
            }
        }
    }
}
