using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HDLogController : ControllerBase
{
    private readonly IHDLogRepository _hdLogRepository;
    private readonly ILogger<HDLogController> _logger;

    public HDLogController(IHDLogRepository hdLogRepository, ILogger<HDLogController> logger)
    {
        _hdLogRepository = hdLogRepository;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All can view HD logs
    public async Task<ActionResult<ApiResponse<HDLog>>> GetById(int id)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(id);
            if (hdLog == null)
            {
                return NotFound(ApiResponse<HDLog>.ErrorResponse("HD Log not found"));
            }
            return Ok(ApiResponse<HDLog>.SuccessResponse(hdLog));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HD Log");
            return StatusCode(500, ApiResponse<HDLog>.ErrorResponse("Error retrieving HD Log"));
        }
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All can view patient HD logs
    public async Task<ActionResult<ApiResponse<IEnumerable<HDLog>>>> GetByPatientId(int patientId)
    {
        try
        {
            var hdLogs = await _hdLogRepository.GetByPatientIdAsync(patientId);
            return Ok(ApiResponse<IEnumerable<HDLog>>.SuccessResponse(hdLogs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HD Logs for patient");
            return StatusCode(500, ApiResponse<IEnumerable<HDLog>>.ErrorResponse("Error retrieving HD Logs"));
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HDLog>>>> GetByDate(DateTime date)
    {
        try
        {
            var hdLogs = await _hdLogRepository.GetByDateAsync(date);
            return Ok(ApiResponse<IEnumerable<HDLog>>.SuccessResponse(hdLogs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HD Logs for date");
            return StatusCode(500, ApiResponse<IEnumerable<HDLog>>.ErrorResponse("Error retrieving HD Logs"));
        }
    }

    [HttpGet("daterange")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HDLog>>>> GetByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var hdLogs = await _hdLogRepository.GetByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<HDLog>>.SuccessResponse(hdLogs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HD Logs for date range");
            return StatusCode(500, ApiResponse<IEnumerable<HDLog>>.ErrorResponse("Error retrieving HD Logs"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor and Nurse can create HD logs
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] HDLog hdLog)
    {
        try
        {
            var username = User.Identity?.Name ?? "System";
            hdLog.CreatedBy = username;
            
            var id = await _hdLogRepository.CreateAsync(hdLog);
            return Ok(ApiResponse<int>.SuccessResponse(id, "HD Log created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HD Log");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error creating HD Log"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor and Nurse can update HD logs (HOD is read-only)
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] HDLog hdLog)
    {
        try
        {
            hdLog.LogID = id;
            var success = await _hdLogRepository.UpdateAsync(hdLog);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("HD Log not found"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(true, "HD Log updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating HD Log");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating HD Log"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete HD logs
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var success = await _hdLogRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("HD Log not found"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(true, "HD Log deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting HD Log");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting HD Log"));
        }
    }

    // Intra-Dialytic Records endpoints
    [HttpGet("{hdLogId}/monitoring")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All can view monitoring records
    public async Task<ActionResult<ApiResponse<IEnumerable<IntraDialyticRecord>>>> GetMonitoringRecords(int hdLogId)
    {
        try
        {
            var records = await _hdLogRepository.GetIntraDialyticRecordsAsync(hdLogId);
            return Ok(ApiResponse<IEnumerable<IntraDialyticRecord>>.SuccessResponse(records));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring records");
            return StatusCode(500, ApiResponse<IEnumerable<IntraDialyticRecord>>.ErrorResponse("Error retrieving monitoring records"));
        }
    }

    [HttpPost("{hdLogId}/monitoring")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Technician")] // Technician CAN add monitoring records
    public async Task<ActionResult<ApiResponse<int>>> AddMonitoringRecord(int hdLogId, [FromBody] IntraDialyticRecord record)
    {
        try
        {
            record.HDLogID = hdLogId;
            var id = await _hdLogRepository.AddIntraDialyticRecordAsync(record);
            return Ok(ApiResponse<int>.SuccessResponse(id, "Monitoring record added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding monitoring record");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error adding monitoring record"));
        }
    }

    [HttpPut("monitoring/{monitoringId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Technician")] // Technician CAN update monitoring records
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMonitoringRecord(int monitoringId, [FromBody] IntraDialyticRecord record)
    {
        try
        {
            record.MonitoringID = monitoringId;
            var success = await _hdLogRepository.UpdateIntraDialyticRecordAsync(record);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Monitoring record not found"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Monitoring record updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating monitoring record");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating monitoring record"));
        }
    }

    [HttpDelete("monitoring/{monitoringId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor/Nurse can delete monitoring (NOT Technician)
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMonitoringRecord(int monitoringId)
    {
        try
        {
            var success = await _hdLogRepository.DeleteIntraDialyticRecordAsync(monitoringId);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Monitoring record not found"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Monitoring record deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting monitoring record");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting monitoring record"));
        }
    }

    // Post-Dialysis Medications endpoints
    [HttpGet("{hdLogId}/medications")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All can view medications
    public async Task<ActionResult<ApiResponse<IEnumerable<PostDialysisMedication>>>> GetMedications(int hdLogId)
    {
        try
        {
            var medications = await _hdLogRepository.GetMedicationsAsync(hdLogId);
            return Ok(ApiResponse<IEnumerable<PostDialysisMedication>>.SuccessResponse(medications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medications");
            return StatusCode(500, ApiResponse<IEnumerable<PostDialysisMedication>>.ErrorResponse("Error retrieving medications"));
        }
    }

    [HttpPost("{hdLogId}/medications")]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor/Nurse can add medications (NOT Technician)
    public async Task<ActionResult<ApiResponse<int>>> AddMedication(int hdLogId, [FromBody] PostDialysisMedication medication)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            medication.HDLogID = hdLogId;
            medication.AdministeredBy = userId;
            
            var id = await _hdLogRepository.AddMedicationAsync(medication);
            return Ok(ApiResponse<int>.SuccessResponse(id, "Medication added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medication");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Error adding medication"));
        }
    }

    [HttpDelete("medications/{medicationId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor/Nurse can delete medications
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedication(int medicationId)
    {
        try
        {
            var success = await _hdLogRepository.DeleteMedicationAsync(medicationId);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Medication not found"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Medication deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medication");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting medication"));
        }
    }

    // ==================== SESSION PHASE MANAGEMENT ====================

    [HttpGet("{hdLogId}/phase-status")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<SessionPhaseStatusDTO>>> GetPhaseStatus(int hdLogId)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<SessionPhaseStatusDTO>.ErrorResponse("Session not found"));

            var status = new SessionPhaseStatusDTO
            {
                SessionPhase = hdLog.SessionPhase,
                IsPreDialysisLocked = hdLog.IsPreDialysisLocked,
                IsIntraDialysisLocked = hdLog.IsIntraDialysisLocked,
                PreDialysisCompletedAt = hdLog.PreDialysisCompletedAt,
                IntraDialysisStartedAt = hdLog.IntraDialysisStartedAt,
                PostDialysisStartedAt = hdLog.PostDialysisStartedAt
            };

            return Ok(ApiResponse<SessionPhaseStatusDTO>.SuccessResponse(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting phase status for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<SessionPhaseStatusDTO>.ErrorResponse("Error retrieving phase status"));
        }
    }

    // ==================== PHASE 1: PRE-DIALYSIS ====================

    [HttpPut("{hdLogId}/save-pre-dialysis")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<HDLog>>> SavePreDialysis(int hdLogId, [FromBody] PreDialysisDTO dto)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<HDLog>.ErrorResponse("Session not found"));

            if (hdLog.IsPreDialysisLocked)
                return BadRequest(ApiResponse<HDLog>.ErrorResponse("Pre-dialysis phase is locked"));

            // Update ONLY pre-dialysis fields
            hdLog.PreWeight = dto.PreWeight;
            hdLog.PreSBP = dto.PreSBP;
            hdLog.PreDBP = dto.PreDBP;
            hdLog.PreHR = dto.PreHR;
            hdLog.PreTemp = dto.PreTemp;
            hdLog.AccessSite = dto.AccessSite;
            hdLog.PreAssessmentNotes = dto.PreAssessmentNotes;

            // Save to database
            await _hdLogRepository.UpdateAsync(hdLog);

            return Ok(ApiResponse<HDLog>.SuccessResponse(hdLog, "Pre-dialysis data saved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving pre-dialysis data for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<HDLog>.ErrorResponse("Error saving data"));
        }
    }

    [HttpPost("{hdLogId}/complete-pre-dialysis")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<string>>> CompletePreDialysis(int hdLogId)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Session not found"));

            if (hdLog.IsPreDialysisLocked)
                return BadRequest(ApiResponse<string>.ErrorResponse("Pre-dialysis phase already completed"));

            // Validate required fields are filled
            if (!hdLog.PreWeight.HasValue || !hdLog.PreSBP.HasValue || !hdLog.PreDBP.HasValue)
                return BadRequest(ApiResponse<string>.ErrorResponse("Please save all required pre-dialysis fields first"));

            // Lock phase and transition
            hdLog.SessionPhase = "INTRA_DIALYSIS";
            hdLog.IsPreDialysisLocked = true;
            hdLog.PreDialysisCompletedAt = DateTime.Now;
            hdLog.IntraDialysisStartedAt = DateTime.Now;
            hdLog.Status = "Active";
            
            if (!hdLog.SessionStartTime.HasValue)
                hdLog.SessionStartTime = DateTime.Now;

            await _hdLogRepository.UpdateAsync(hdLog);

            return Ok(ApiResponse<string>.SuccessResponse("Pre-dialysis completed. Treatment started."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing pre-dialysis for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error completing pre-dialysis"));
        }
    }

    // ==================== PHASE 2: INTRA-DIALYSIS ====================

    [HttpPost("{hdLogId}/start-post-dialysis")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<string>>> StartPostDialysis(int hdLogId)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Session not found"));

            if (hdLog.SessionPhase != "INTRA_DIALYSIS")
                return BadRequest(ApiResponse<string>.ErrorResponse("Must be in treatment phase"));

            if (hdLog.IsIntraDialysisLocked)
                return BadRequest(ApiResponse<string>.ErrorResponse("Intra-dialysis phase already completed"));

            // Lock phase and transition
            hdLog.SessionPhase = "POST_DIALYSIS";
            hdLog.IsIntraDialysisLocked = true;
            hdLog.PostDialysisStartedAt = DateTime.Now;

            await _hdLogRepository.UpdateAsync(hdLog);

            return Ok(ApiResponse<string>.SuccessResponse("Treatment ended. Post-dialysis started."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting post-dialysis for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error starting post-dialysis"));
        }
    }

    // ==================== PHASE 3: POST-DIALYSIS ====================

    [HttpPut("{hdLogId}/save-post-dialysis")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<HDLog>>> SavePostDialysis(int hdLogId, [FromBody] PostDialysisDTO dto)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<HDLog>.ErrorResponse("Session not found"));

            if (hdLog.SessionPhase != "POST_DIALYSIS")
                return BadRequest(ApiResponse<HDLog>.ErrorResponse("Session not in post-dialysis phase"));

            // Update ONLY post-dialysis fields
            hdLog.PostDialysisWeight = dto.PostDialysisWeight;
            hdLog.PostWeight = dto.PostDialysisWeight; // Also update legacy field
            hdLog.PostDialysisSBP = dto.PostDialysisSBP;
            hdLog.PostDialysisDBP = dto.PostDialysisDBP;
            hdLog.PostDialysisHR = dto.PostDialysisHR;
            hdLog.AccessBleedingTime = dto.AccessBleedingTime;
            hdLog.TotalFluidRemoved = dto.TotalFluidRemoved;
            hdLog.PostAccessStatus = dto.PostAccessStatus;
            hdLog.MedicationsAdministered = dto.MedicationsAdministered;
            hdLog.DischargeNotes = dto.DischargeNotes;
            
            // Calculate weight loss
            if (hdLog.PreWeight.HasValue)
            {
                hdLog.WeightLoss = hdLog.PreWeight.Value - dto.PostDialysisWeight;
            }

            // Save to database (but don't complete yet)
            await _hdLogRepository.UpdateAsync(hdLog);

            return Ok(ApiResponse<HDLog>.SuccessResponse(hdLog, "Post-dialysis data saved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving post-dialysis data for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<HDLog>.ErrorResponse("Error saving data"));
        }
    }

    [HttpPost("{hdLogId}/complete-post-dialysis")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<string>>> CompletePostDialysis(int hdLogId)
    {
        try
        {
            var hdLog = await _hdLogRepository.GetByIdAsync(hdLogId);
            if (hdLog == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Session not found"));

            if (hdLog.SessionPhase != "POST_DIALYSIS")
                return BadRequest(ApiResponse<string>.ErrorResponse("Session must be in post-dialysis phase"));

            // Validate required fields are filled
            if (!hdLog.PostDialysisWeight.HasValue || !hdLog.PostDialysisSBP.HasValue || 
                !hdLog.PostDialysisDBP.HasValue || string.IsNullOrEmpty(hdLog.PostAccessStatus))
                return BadRequest(ApiResponse<string>.ErrorResponse("Please save all required post-dialysis fields first"));

            // Complete session
            hdLog.SessionPhase = "COMPLETED";
            hdLog.Status = "Completed";
            hdLog.IsDischarged = true;
            hdLog.ActualEndTime = DateTime.Now;

            await _hdLogRepository.UpdateAsync(hdLog);

            return Ok(ApiResponse<string>.SuccessResponse("Session completed. Patient discharged."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session for HDLog {HDLogId}", hdLogId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error completing session"));
        }
    }
}
