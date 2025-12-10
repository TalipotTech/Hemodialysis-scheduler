using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureSuggestionController : ControllerBase
    {
        private readonly IFeatureSuggestionService _featureService;
        private readonly ILogger<FeatureSuggestionController> _logger;

        public FeatureSuggestionController(
            IFeatureSuggestionService featureService,
            ILogger<FeatureSuggestionController> logger)
        {
            _featureService = featureService;
            _logger = logger;
        }

        /// <summary>
        /// Analyze system and generate AI-powered feature suggestions for developers
        /// </summary>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeAndSuggest([FromBody] FeatureAnalysisRequest request)
        {
            try
            {
                var analysis = await _featureService.AnalyzeAndSuggestFeaturesAsync(request);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating feature suggestions");
                return StatusCode(500, new { error = "Failed to generate feature suggestions" });
            }
        }

        /// <summary>
        /// Get all pending feature suggestions
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingSuggestions([FromQuery] string? category = null)
        {
            try
            {
                var suggestions = await _featureService.GetPendingSuggestionsAsync(category);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending suggestions");
                return StatusCode(500, new { error = "Failed to retrieve suggestions" });
            }
        }

        /// <summary>
        /// Update feature suggestion status (mark as implemented or reviewed)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, 
            [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var updated = await _featureService.UpdateSuggestionStatusAsync(
                    id, 
                    request.IsImplemented, 
                    request.DeveloperNotes);
                
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating suggestion status");
                return StatusCode(500, new { error = "Failed to update status" });
            }
        }

        /// <summary>
        /// Get feature suggestion statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _featureService.GetSuggestionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats");
                return StatusCode(500, new { error = "Failed to retrieve stats" });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public bool IsImplemented { get; set; }
        public string? DeveloperNotes { get; set; }
    }
}
