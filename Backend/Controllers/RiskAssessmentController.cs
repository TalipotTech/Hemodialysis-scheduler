using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RiskAssessmentController : ControllerBase
    {
        private readonly IRiskAssessmentService _riskService;
        private readonly ILogger<RiskAssessmentController> _logger;

        public RiskAssessmentController(
            IRiskAssessmentService riskService,
            ILogger<RiskAssessmentController> logger)
        {
            _riskService = riskService;
            _logger = logger;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientRiskAssessment(int patientId)
        {
            try
            {
                var assessment = await _riskService.AssessPatientRiskAsync(patientId);
                return Ok(assessment);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing patient risk for ID: {PatientId}", patientId);
                return StatusCode(500, new { error = "Failed to assess patient risk" });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetBatchRiskAssessment([FromBody] BatchRiskAssessmentRequest request)
        {
            try
            {
                if (request.PatientIds == null || request.PatientIds.Length == 0)
                {
                    return BadRequest(new { error = "Patient IDs are required" });
                }

                var assessments = await _riskService.BatchAssessRiskAsync(request.PatientIds);
                return Ok(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch risk assessment");
                return StatusCode(500, new { error = "Failed to assess risks" });
            }
        }

        [HttpGet("high-risk")]
        public async Task<IActionResult> GetHighRiskPatients([FromQuery] int threshold = 60)
        {
            try
            {
                if (threshold < 0 || threshold > 100)
                {
                    return BadRequest(new { error = "Threshold must be between 0 and 100" });
                }

                var patients = await _riskService.GetHighRiskPatientsAsync(threshold);
                return Ok(new
                {
                    threshold,
                    count = patients.Count,
                    patients
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving high-risk patients");
                return StatusCode(500, new { error = "Failed to retrieve high-risk patients" });
            }
        }

        [HttpPost("factors/{patientId}")]
        public async Task<IActionResult> AnalyzeRiskFactors(int patientId, [FromBody] RiskFactorsRequest? request)
        {
            try
            {
                var analysis = await _riskService.AnalyzeRiskFactorsAsync(patientId, request?.CustomFactors);
                return Ok(analysis);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing risk factors for patient: {PatientId}", patientId);
                return StatusCode(500, new { error = "Failed to analyze risk factors" });
            }
        }
    }
}
