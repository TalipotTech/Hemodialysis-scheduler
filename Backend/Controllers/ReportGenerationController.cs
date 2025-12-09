using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Services.AI;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportGenerationController : ControllerBase
    {
        private readonly IReportGenerationService _reportService;
        private readonly ILogger<ReportGenerationController> _logger;

        public ReportGenerationController(
            IReportGenerationService reportService,
            ILogger<ReportGenerationController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a daily summary report
        /// </summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GenerateDailyReport([FromQuery] DateTime? date)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;
                var report = await _reportService.GenerateDailyReportAsync(reportDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily report");
                return StatusCode(500, new { error = "Failed to generate daily report" });
            }
        }

        /// <summary>
        /// Generate a weekly summary report
        /// </summary>
        [HttpGet("weekly")]
        public async Task<IActionResult> GenerateWeeklyReport([FromQuery] DateTime? startDate)
        {
            try
            {
                var weekStart = startDate ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var report = await _reportService.GenerateWeeklyReportAsync(weekStart);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly report");
                return StatusCode(500, new { error = "Failed to generate weekly report" });
            }
        }

        /// <summary>
        /// Generate a patient-specific treatment summary
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GeneratePatientReport(
            int patientId, 
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today;
                
                var report = await _reportService.GeneratePatientReportAsync(patientId, start, end);
                return Ok(report);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating patient report");
                return StatusCode(500, new { error = "Failed to generate patient report" });
            }
        }

        /// <summary>
        /// Generate a custom report with AI analysis
        /// </summary>
        [HttpPost("custom")]
        public async Task<IActionResult> GenerateCustomReport([FromBody] CustomReportRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ReportType))
                {
                    return BadRequest(new { error = "Report type is required" });
                }

                var report = await _reportService.GenerateCustomReportAsync(
                    request.ReportType,
                    request.Parameters);
                
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, new { error = "Failed to generate custom report" });
            }
        }

        /// <summary>
        /// Get list of available report templates
        /// </summary>
        [HttpGet("templates")]
        public IActionResult GetReportTemplates()
        {
            try
            {
                var templates = _reportService.GetAvailableTemplates();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report templates");
                return StatusCode(500, new { error = "Failed to retrieve templates" });
            }
        }

        /// <summary>
        /// Export report to PDF format
        /// </summary>
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromBody] ReportExportRequest request)
        {
            try
            {
                var pdfBytes = await _reportService.ExportToPdfAsync(request.ReportContent, request.Title);
                return File(pdfBytes, "application/pdf", $"{request.Title}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to PDF");
                return StatusCode(500, new { error = "Failed to export PDF" });
            }
        }
    }
}
