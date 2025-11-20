using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientHistoryController : ControllerBase
{
    private readonly IPatientHistoryRepository _historyRepository;
    private readonly ILogger<PatientHistoryController> _logger;

    public PatientHistoryController(IPatientHistoryRepository historyRepository, ILogger<PatientHistoryController> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    [HttpGet("{patientId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<PatientTreatmentHistory>>> GetPatientHistory(int patientId)
    {
        try
        {
            var history = await _historyRepository.GetPatientHistoryAsync(patientId);
            
            if (history == null)
            {
                return NotFound(ApiResponse<PatientTreatmentHistory>.ErrorResponse("Patient not found"));
            }

            return Ok(ApiResponse<PatientTreatmentHistory>.SuccessResponse(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient history for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<PatientTreatmentHistory>.ErrorResponse("An error occurred while retrieving patient history"));
        }
    }

    [HttpGet("session/{scheduleId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<DetailedSessionView>>> GetSessionDetails(int scheduleId)
    {
        try
        {
            var session = await _historyRepository.GetSessionDetailsAsync(scheduleId);
            
            if (session == null)
            {
                return NotFound(ApiResponse<DetailedSessionView>.ErrorResponse("Session not found"));
            }

            return Ok(ApiResponse<DetailedSessionView>.SuccessResponse(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session details for schedule {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<DetailedSessionView>.ErrorResponse("An error occurred while retrieving session details"));
        }
    }

    [HttpGet("{patientId}/trends")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<PatientVitalTrends>>> GetVitalTrends(int patientId, [FromQuery] int? lastNSessions = null)
    {
        try
        {
            var trends = await _historyRepository.GetVitalTrendsAsync(patientId, lastNSessions);
            
            if (trends == null)
            {
                return NotFound(ApiResponse<PatientVitalTrends>.ErrorResponse("No data found"));
            }

            return Ok(ApiResponse<PatientVitalTrends>.SuccessResponse(trends));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vital trends for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<PatientVitalTrends>.ErrorResponse("An error occurred while retrieving vital trends"));
        }
    }

    [HttpGet("{patientId}/statistics")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<TreatmentStatistics>>> GetTreatmentStatistics(int patientId)
    {
        try
        {
            var statistics = await _historyRepository.GetTreatmentStatisticsAsync(patientId);
            
            if (statistics == null)
            {
                return NotFound(ApiResponse<TreatmentStatistics>.ErrorResponse("No statistics available"));
            }

            return Ok(ApiResponse<TreatmentStatistics>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving treatment statistics for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<TreatmentStatistics>.ErrorResponse("An error occurred while retrieving treatment statistics"));
        }
    }
}
