using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FormAutocompleteController : ControllerBase
    {
        private readonly IFormAutocompleteService _autocompleteService;
        private readonly ILogger<FormAutocompleteController> _logger;

        public FormAutocompleteController(
            IFormAutocompleteService autocompleteService,
            ILogger<FormAutocompleteController> logger)
        {
            _autocompleteService = autocompleteService;
            _logger = logger;
        }

        /// <summary>
        /// Get AI-powered autocomplete predictions for HD session form
        /// </summary>
        [HttpPost("predict-session")]
        public async Task<IActionResult> PredictSessionData([FromBody] SessionAutocompleteRequest request)
        {
            try
            {
                var predictions = await _autocompleteService.PredictSessionDataAsync(request);
                return Ok(predictions);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting session data for patient {PatientId}", request.PatientId);
                return StatusCode(500, new { error = "Failed to generate predictions" });
            }
        }

        /// <summary>
        /// Get prediction for a single form field
        /// </summary>
        [HttpGet("predict-field/{patientId}/{fieldName}")]
        public async Task<IActionResult> PredictField(int patientId, string fieldName)
        {
            try
            {
                var prediction = await _autocompleteService.PredictFieldValueAsync(patientId, fieldName, null);
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting field {FieldName} for patient {PatientId}", fieldName, patientId);
                return StatusCode(500, new { error = "Failed to predict field value" });
            }
        }

        /// <summary>
        /// Get cached autocomplete value for quick retrieval
        /// </summary>
        [HttpGet("cache/{patientId}/{fieldName}")]
        public async Task<IActionResult> GetCachedValue(int patientId, string fieldName)
        {
            try
            {
                var cached = await _autocompleteService.GetCachedAutocompleteAsync(patientId, fieldName);
                
                if (cached == null)
                {
                    return NotFound(new { message = "No cached value found" });
                }

                return Ok(new { fieldName, value = cached, source = "cache" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached value");
                return StatusCode(500, new { error = "Failed to retrieve cached value" });
            }
        }
    }
}
