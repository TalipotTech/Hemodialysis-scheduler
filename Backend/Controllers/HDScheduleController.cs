using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;
using HDScheduler.API.Services;
using HDScheduler.API.Data;
using Backend.DTOs;
using Dapper;
using System.Security.Claims;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HDScheduleController : ControllerBase
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientActivityRepository _activityRepository;
    private readonly EquipmentUsageService _equipmentUsageService;
    private readonly IRecurringSessionService _recurringSessionService;
    private readonly ILogger<HDScheduleController> _logger;
    private readonly DapperContext _context;

    public HDScheduleController(
        IHDScheduleRepository scheduleRepository,
        IPatientRepository patientRepository,
        IPatientActivityRepository activityRepository,
        EquipmentUsageService equipmentUsageService,
        IRecurringSessionService recurringSessionService,
        ILogger<HDScheduleController> logger,
        DapperContext context)
    {
        _scheduleRepository = scheduleRepository;
        _patientRepository = patientRepository;
        _activityRepository = activityRepository;
        _equipmentUsageService = equipmentUsageService;
        _recurringSessionService = recurringSessionService;
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    // [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view schedules
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetAllSchedules()
    {
        try
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HD schedules");
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving schedules"));
        }
    }

    [HttpGet("debug/today")]
    [AllowAnonymous] // Temporary debug endpoint - remove in production
    public async Task<ActionResult> GetTodaySchedulesDebug()
    {
        try
        {
            var schedules = await _scheduleRepository.GetTodaySchedulesAsync();
            var result = schedules.Select(s => new {
                s.ScheduleID,
                s.PatientName,
                s.SessionDate,
                s.SlotID,
                s.BedNumber,
                s.IsDischarged,
                Status = s.IsDischarged ? "DISCHARGED" : "ACTIVE"
            }).ToList();
            
            return Ok(new {
                totalSchedules = result.Count,
                activeCount = result.Count(r => !r.IsDischarged),
                dischargedCount = result.Count(r => r.IsDischarged),
                schedules = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view active schedules
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetActiveSchedules()
    {
        try
        {
            var schedules = await _scheduleRepository.GetActiveAsync();
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active schedules");
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving active schedules"));
        }
    }

    [HttpGet("slot/{slotId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetSlotSchedule(int slotId, [FromQuery] DateTime? date, [FromQuery] int? excludeScheduleId)
    {
        try
        {
            var targetDate = date ?? DateTime.Today;
            var schedules = await _scheduleRepository.GetBySlotAndDateAsync(slotId, targetDate);
            
            _logger.LogInformation($"GetSlotSchedule: Slot={slotId}, Date={targetDate:yyyy-MM-dd}, ExcludeScheduleId={excludeScheduleId}");
            
            // Get actual bed capacity from Slots table
            using var connection = _context.CreateConnection();
            var maxBeds = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT MaxBeds FROM Slots WHERE SlotID = @SlotID",
                new { SlotID = slotId });
            
            // Default to 10 if not found
            if (maxBeds == 0) maxBeds = 10;
            
            _logger.LogInformation($"GetSlotSchedule for Slot {slotId}, Date {targetDate:yyyy-MM-dd}: Found {schedules.Count} schedules, MaxBeds={maxBeds}");
            
            // Create bed availability map using actual capacity
            var beds = new List<object>();
            for (int bedNum = 1; bedNum <= maxBeds; bedNum++)
            {
                var occupiedSchedule = schedules.FirstOrDefault(s => 
                    s.BedNumber == bedNum && 
                    !s.IsDischarged && 
                    !s.IsMovedToHistory &&
                    (!excludeScheduleId.HasValue || s.ScheduleID != excludeScheduleId.Value)); // Exclude current session when editing
                
                string bedStatus = "available";
                if (occupiedSchedule != null)
                {
                    // Determine status based on session date and discharge status
                    if (occupiedSchedule.SessionDate.Date < DateTime.Today)
                    {
                        bedStatus = "completed"; // Past session
                    }
                    else if (occupiedSchedule.SessionDate.Date > DateTime.Today)
                    {
                        bedStatus = "pre-scheduled"; // Future session
                    }
                    else if (occupiedSchedule.SessionStatus == "Pre-Scheduled")
                    {
                        bedStatus = "pre-scheduled"; // Today but not yet activated
                    }
                    else
                    {
                        bedStatus = "occupied"; // Active session today
                    }
                    
                    _logger.LogInformation($"  Bed {bedNum}: {bedStatus} - Patient: {occupiedSchedule.PatientName} (ScheduleID: {occupiedSchedule.ScheduleID})");
                }
                
                beds.Add(new
                {
                    bedNumber = bedNum,
                    status = bedStatus,
                    patient = occupiedSchedule != null ? new { name = occupiedSchedule.PatientName, id = occupiedSchedule.PatientID } : null
                });
            }
            
            var occupiedCount = beds.Count(b => ((dynamic)b).status != "available");
            var availableCount = beds.Count(b => ((dynamic)b).status == "available");
            _logger.LogInformation($"GetSlotSchedule result: {occupiedCount} occupied/pre-scheduled, {availableCount} available");
            
            var result = new { beds, maxBeds };
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving slot schedule for slot {SlotId}", slotId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving slot schedule"));
        }
    }

    [HttpGet("today")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view today's schedule
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetTodaySchedules()
    {
        try
        {
            var schedules = await _scheduleRepository.GetTodaySchedulesAsync();
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's schedules");
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving today's schedules"));
        }
    }

    [HttpGet("history")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view history
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetHistorySessions()
    {
        try
        {
            var schedules = await _scheduleRepository.GetHistorySessionsAsync();
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history sessions");
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving history sessions"));
        }
    }

    [HttpGet("future-scheduled")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view bed schedule
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetFutureScheduledSessions()
    {
        try
        {
            var schedules = await _scheduleRepository.GetFutureScheduledSessionsAsync();
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules, 
                $"Retrieved {schedules.Count} future scheduled sessions"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving future scheduled sessions");
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving future scheduled sessions"));
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view individual schedule
    public async Task<ActionResult<ApiResponse<HDSchedule>>> GetSchedule(int id)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            
            if (schedule == null)
            {
                return NotFound(ApiResponse<HDSchedule>.ErrorResponse("Schedule not found"));
            }

            return Ok(ApiResponse<HDSchedule>.SuccessResponse(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<HDSchedule>.ErrorResponse("An error occurred while retrieving the schedule"));
        }
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All roles can view patient schedules
    public async Task<ActionResult<ApiResponse<List<HDSchedule>>>> GetSchedulesByPatient(int patientId)
    {
        try
        {
            var schedules = await _scheduleRepository.GetByPatientIdAsync(patientId);
            return Ok(ApiResponse<List<HDSchedule>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<List<HDSchedule>>.ErrorResponse("An error occurred while retrieving patient schedules"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor and Nurse can create HD sessions
    public async Task<ActionResult<ApiResponse<int>>> CreateSchedule([FromBody] CreateHDScheduleRequest request)
    {
        try
        {
            // Log model state errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<int>.ErrorResponse($"Validation failed: {string.Join(", ", errors)}"));
            }

            // TODO: Prevent duplicate patient scheduling - Check if patient already has a session on this date
            var existingSchedules = await _scheduleRepository.GetByPatientIdAsync(request.PatientID);
            var sessionOnSameDate = existingSchedules.FirstOrDefault(s => 
                s.SessionDate.Date == request.SessionDate.Date && 
                !s.IsDischarged && 
                !s.IsMovedToHistory);
            
            if (sessionOnSameDate != null)
            {
                _logger.LogWarning("Duplicate session attempt: Patient {PatientID} already has session {ScheduleID} on {Date}",
                    request.PatientID, sessionOnSameDate.ScheduleID, request.SessionDate.Date);
                return BadRequest(ApiResponse<int>.ErrorResponse(
                    $"Patient already has an HD session scheduled on {request.SessionDate:yyyy-MM-dd}. Each patient can only have ONE session per day. Existing session ID: {sessionOnSameDate.ScheduleID}"
                ));
            }
            
            _logger.LogInformation("Creating new HD session for Patient {PatientID} on {Date}, Slot {SlotID}, Bed {BedNumber}",
                request.PatientID, request.SessionDate.Date, request.SlotID, request.BedNumber);

            var username = User.Identity?.Name ?? "System";
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";

            var schedule = new HDSchedule
            {
                PatientID = request.PatientID,
                SessionDate = request.SessionDate,
                DryWeight = request.DryWeight,
                HDStartDate = request.HDStartDate,
                HDCycle = request.HDCycle,
                HDFrequency = request.HDFrequency,
                SessionsPerWeek = request.SessionsPerWeek,
                WeightGain = request.WeightGain,
                DialyserType = request.DialyserType,
                DialyserModel = request.DialyserModel,
                DialyserReuseCount = request.DialyserReuseCount,
                BloodTubingReuse = request.BloodTubingReuse,
                HDUnitNumber = request.HDUnitNumber,
                PrescribedDuration = request.PrescribedDuration,
                UFGoal = request.UFGoal,
                DialysatePrescription = request.DialysatePrescription,
                PrescribedBFR = request.PrescribedBFR,
                AnticoagulationType = request.AnticoagulationType,
                HeparinDose = request.HeparinDose,
                SyringeType = request.SyringeType,
                BolusDose = request.BolusDose,
                HeparinInfusionRate = request.HeparinInfusionRate,
                AccessType = request.AccessType,
                AccessLocation = request.AccessLocation,
                BloodPressure = request.BloodPressure,
                Symptoms = request.Symptoms,
                BloodTestDone = request.BloodTestDone,
                SlotID = request.SlotID,
                BedNumber = request.BedNumber,
                AssignedDoctor = request.AssignedDoctor,
                AssignedNurse = request.AssignedNurse,
                // HDTreatmentSession fields
                StartTime = !string.IsNullOrEmpty(request.StartTime) && TimeSpan.TryParse(request.StartTime, out var startTime) ? startTime : null,
                PreWeight = request.PreWeight,
                PreBPSitting = request.PreBPSitting,
                PreTemperature = request.PreTemperature,
                AccessBleedingTime = !string.IsNullOrEmpty(request.AccessBleedingTime) && int.TryParse(request.AccessBleedingTime, out var bleedingTime) ? bleedingTime : null,
                AccessStatus = request.AccessStatus,
                Complications = request.Complications,
                // Post-Dialysis Assessment fields - vitals only (medications/alerts in separate tables)
                PostWeight = request.PostWeight,
                PostSBP = request.PostSBP,
                PostDBP = request.PostDBP,
                PostHR = request.PostHR,
                PostAccessStatus = request.PostAccessStatus,
                TotalFluidRemoved = request.TotalFluidRemoved,
                // Status
                CreatedByStaffName = username,
                CreatedByStaffRole = role,
                IsDischarged = false
            };

            var scheduleId = await _scheduleRepository.CreateAsync(schedule);
            
            // Create HDLog entry with basic session data
            if (request.PreWeight.HasValue)
            {
                var hdLog = new HDLog
                {
                    PatientID = request.PatientID,
                    ScheduleID = scheduleId,
                    SessionDate = request.SessionDate,
                    PreWeight = request.PreWeight,
                    BloodPressurePre = request.PreBPSitting,
                    Temperature = request.PreTemperature,
                    Notes = request.Complications,
                    CreatedBy = username
                };
                await _scheduleRepository.CreateHDLogAsync(hdLog);
            }
            
            // Create IntraDialytic record if monitoring data provided
            if (!string.IsNullOrEmpty(request.MonitoringTime))
            {
                await _scheduleRepository.CreateIntraDialyticRecordAsync(new
                {
                    PatientID = request.PatientID,
                    ScheduleID = scheduleId,
                    SessionDate = request.SessionDate,
                    TimeRecorded = request.MonitoringTime,
                    BloodPressure = request.BloodPressure,
                    PulseRate = request.HeartRate,
                    Temperature = request.PreTemperature,
                    UFVolume = request.TotalUFAchieved,
                    VenousPressure = request.VenousPressure,
                    Notes = request.Interventions
                });
            }
            
            // Create medication record if provided
            if (!string.IsNullOrEmpty(request.MedicationName))
            {
                await _scheduleRepository.CreatePostDialysisMedicationAsync(new
                {
                    PatientID = request.PatientID,
                    ScheduleID = scheduleId,
                    SessionDate = request.SessionDate,
                    MedicationName = request.MedicationName,
                    Dosage = request.Dose,
                    Route = request.Route,
                    AdministeredBy = username,
                    AdministeredAt = request.AdministeredAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            
            _logger.LogInformation("HD Schedule created successfully with ID {ScheduleId} by {Username}", scheduleId, username);
            
            // Generate recurring sessions if HD Cycle is specified (until discharge - 90 days ahead)
            _logger.LogInformation("Checking HD Cycle for recurring sessions: HDCycle={HDCycle}", request.HDCycle);
            if (!string.IsNullOrEmpty(request.HDCycle))
            {
                try
                {
                    _logger.LogInformation("🔄 Generating recurring sessions for HDCycle={HDCycle}, ScheduleId={ScheduleId} (90 days ahead)", 
                        request.HDCycle, scheduleId);
                    schedule.ScheduleID = scheduleId; // Set the ID for the base session
                    var recurringSessionIds = await _recurringSessionService.GenerateRecurringSessions(schedule, daysAhead: 90);
                    _logger.LogInformation("✅ Created {Count} recurring sessions for schedule {ScheduleId} covering next 90 days", 
                        recurringSessionIds.Count, scheduleId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to create recurring sessions for schedule {ScheduleId}", scheduleId);
                    // Don't fail the main request if recurring creation fails
                }
            }
            else
            {
                _logger.LogInformation("⏭️ No HD Cycle specified, skipping recurring session generation");
            }
            
            return CreatedAtAction(nameof(GetSchedule), new { id = scheduleId }, 
                ApiResponse<int>.SuccessResponse(scheduleId, "HD Schedule created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HD schedule for patient {PatientId}", request.PatientID);
            return StatusCode(500, ApiResponse<int>.ErrorResponse($"An error occurred while creating the schedule: {ex.Message}"));
        }
    }

    [HttpPatch("{id}/auto-save")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Technician")] // All staff can auto-save during treatment
    public async Task<ActionResult<ApiResponse<bool>>> AutoSaveSchedule(int id, [FromBody] System.Text.Json.JsonElement updates)
    {
        try
        {
            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                _logger.LogWarning("Auto-save failed: Schedule {ScheduleId} not found", id);
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            _logger.LogInformation("Auto-save request for schedule {ScheduleId}: IsDischarged={IsDischarged}, IsMovedToHistory={IsMovedToHistory}, SessionDate={SessionDate}, SessionStatus={SessionStatus}", 
                id, existingSchedule.IsDischarged, existingSchedule.IsMovedToHistory, existingSchedule.SessionDate, existingSchedule.SessionStatus);

            // Allow editing of all sessions for late documentation/corrections
            // Log historical edits for audit purposes
            if (existingSchedule.IsMovedToHistory)
            {
                _logger.LogInformation("Auto-save on historical session {ScheduleId} - allowing for documentation purposes", id);
            }
            
            if (existingSchedule.IsDischarged)
            {
                _logger.LogInformation("Auto-save on discharged session {ScheduleId} - allowing for documentation purposes", id);
            }

            // Convert JsonElement to Dictionary with proper type conversion
            var updateDict = new Dictionary<string, object>();
            foreach (var prop in updates.EnumerateObject())
            {
                object? value = prop.Value.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => prop.Value.GetString(),
                    System.Text.Json.JsonValueKind.Number => ConvertJsonNumber(prop.Value),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => prop.Value.ToString()
                };
                
                if (value != null)
                {
                    updateDict[prop.Name] = value;
                }
            }

            _logger.LogInformation("Auto-saving schedule {ScheduleId} with fields: {Fields}", id, string.Join(", ", updateDict.Keys));
            var result = await _scheduleRepository.PartialUpdateAsync(id, updateDict);
            
            if (result)
            {
                _logger.LogInformation("HD Schedule {ScheduleId} auto-saved successfully with values: {Values}", 
                    id, string.Join(", ", updateDict.Select(kvp => $"{kvp.Key}={kvp.Value}")));
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Data saved successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to save data"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-saving schedule {ScheduleId}", id);
            // Temporarily return detailed error for debugging
            var errorDetails = $"{ex.Message}";
            if (ex.InnerException != null)
            {
                errorDetails += $" Inner: {ex.InnerException.Message}";
            }
            return StatusCode(500, ApiResponse<string>.ErrorResponse(errorDetails));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")] // Only Doctor and Nurse can update prescriptions
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSchedule(int id, [FromBody] UpdateHDScheduleRequest request)
    {
        try
        {
            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            // TODO: Prevent duplicate patient scheduling - Check if date is being changed
            if (existingSchedule.SessionDate.Date != request.SessionDate.Date)
            {
                // Validate that patient doesn't already have a session on the new date
                var patientSchedules = await _scheduleRepository.GetByPatientIdAsync(existingSchedule.PatientID);
                var conflictingSession = patientSchedules.FirstOrDefault(s => 
                    s.ScheduleID != id && // Exclude current session being updated
                    s.SessionDate.Date == request.SessionDate.Date && 
                    !s.IsDischarged && 
                    !s.IsMovedToHistory);
                
                if (conflictingSession != null)
                {
                    _logger.LogWarning("Update failed: Patient {PatientID} already has session {ScheduleID} on {Date}",
                        existingSchedule.PatientID, conflictingSession.ScheduleID, request.SessionDate.Date);
                    return BadRequest(ApiResponse<bool>.ErrorResponse(
                        $"Patient already has an HD session scheduled on {request.SessionDate:yyyy-MM-dd}. Each patient can only have ONE session per day. Existing session ID: {conflictingSession.ScheduleID}"
                    ));
                }
            }

            existingSchedule.SessionDate = request.SessionDate;
            existingSchedule.DryWeight = request.DryWeight;
            existingSchedule.HDStartDate = request.HDStartDate;
            existingSchedule.HDCycle = request.HDCycle;
            existingSchedule.WeightGain = request.WeightGain;
            existingSchedule.DialyserType = request.DialyserType;
            existingSchedule.DialyserModel = request.DialyserModel;
            existingSchedule.DialyserReuseCount = request.DialyserReuseCount;
            existingSchedule.BloodTubingReuse = request.BloodTubingReuse;
            existingSchedule.HDUnitNumber = request.HDUnitNumber;
            existingSchedule.PrescribedDuration = request.PrescribedDuration;
            existingSchedule.UFGoal = request.UFGoal;
            existingSchedule.DialysatePrescription = request.DialysatePrescription;
            existingSchedule.PrescribedBFR = request.PrescribedBFR;
            existingSchedule.AnticoagulationType = request.AnticoagulationType;
            existingSchedule.HeparinDose = request.HeparinDose;
            existingSchedule.SyringeType = request.SyringeType;
            existingSchedule.BolusDose = request.BolusDose;
            existingSchedule.HeparinInfusionRate = request.HeparinInfusionRate;
            existingSchedule.AccessType = request.AccessType;
            existingSchedule.AccessLocation = request.AccessLocation;
            existingSchedule.BloodPressure = request.BloodPressure;
            existingSchedule.Symptoms = request.Symptoms;
            existingSchedule.BloodTestDone = request.BloodTestDone;
            existingSchedule.SlotID = request.SlotID;
            existingSchedule.BedNumber = request.BedNumber;
            existingSchedule.AssignedDoctor = request.AssignedDoctor;
            existingSchedule.AssignedNurse = request.AssignedNurse;
            // HDTreatmentSession fields
            existingSchedule.StartTime = !string.IsNullOrEmpty(request.StartTime) && TimeSpan.TryParse(request.StartTime, out var startTime) ? startTime : null;
            existingSchedule.PreWeight = request.PreWeight;
            existingSchedule.PreBPSitting = request.PreBPSitting;
            existingSchedule.PreTemperature = request.PreTemperature;
            existingSchedule.AccessBleedingTime = !string.IsNullOrEmpty(request.AccessBleedingTime) && int.TryParse(request.AccessBleedingTime, out var bleedingTime) ? bleedingTime : null;
            existingSchedule.AccessStatus = request.AccessStatus;
            existingSchedule.Complications = request.Complications;
            // Post-Dialysis Assessment fields (vitals only - medications/alerts in separate tables)
            existingSchedule.PostWeight = request.PostWeight;
            existingSchedule.PostSBP = request.PostSBP;
            existingSchedule.PostDBP = request.PostDBP;
            existingSchedule.PostHR = request.PostHR;
            existingSchedule.PostAccessStatus = request.PostAccessStatus;
            existingSchedule.TotalFluidRemoved = request.TotalFluidRemoved;
            // Status
            existingSchedule.IsDischarged = request.IsDischarged;

            var result = await _scheduleRepository.UpdateAsync(existingSchedule);
            
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Schedule updated successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update schedule"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while updating the schedule"));
        }
    }

    [HttpPut("{id}/discharge")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> DischargeSchedule(int id, [FromQuery] bool autoScheduleNext = true)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Schedule not found"));
            }

            var result = await _scheduleRepository.DischargeAsync(id);
            
            if (!result)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to discharge patient"));
            }

            object? nextSessionInfo = null;

            // Auto-schedule next session based on HD cycle if enabled
            if (autoScheduleNext)
            {
                try
                {
                    var patient = await _patientRepository.GetByIdAsync(schedule.PatientID);
                    if (patient != null && !string.IsNullOrEmpty(patient.HDCycle))
                    {
                        // Calculate next dialysis date
                        var nextDate = CalculateNextDialysisDate(patient.HDCycle, schedule.SessionDate);
                        
                        if (nextDate.HasValue)
                        {
                            // Check if session already exists for that date
                            var existingSession = await _scheduleRepository.GetByPatientAndDateAsync(patient.PatientID, nextDate.Value);
                            
                            if (existingSession == null)
                            {
                                // Create new session
                                var newSession = new HDSchedule
                                {
                                    PatientID = patient.PatientID,
                                    SessionDate = nextDate.Value,
                                    SlotID = schedule.SlotID,
                                    BedNumber = schedule.BedNumber,
                                    
                                    DryWeight = patient.DryWeight ?? schedule.DryWeight,
                                    HDStartDate = patient.HDStartDate,
                                    HDCycle = patient.HDCycle,
                                    HDFrequency = patient.HDFrequency,
                                    
                                    DialyserType = patient.DialyserType ?? schedule.DialyserType,
                                    DialyserModel = patient.DialyserModel ?? schedule.DialyserModel,
                                    PrescribedDuration = patient.PrescribedDuration ?? schedule.PrescribedDuration,
                                    PrescribedBFR = patient.PrescribedBFR ?? schedule.PrescribedBFR,
                                    DialysatePrescription = patient.DialysatePrescription ?? schedule.DialysatePrescription,
                                    
                                    AssignedDoctor = schedule.AssignedDoctor,
                                    AssignedNurse = schedule.AssignedNurse,
                                    
                                    SessionStatus = "Pre-Scheduled",
                                    IsAutoGenerated = true,
                                    ParentScheduleID = id,
                                    
                                    IsDischarged = false,
                                    IsMovedToHistory = false,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };

                                var newScheduleId = await _scheduleRepository.CreateAsync(newSession);
                                
                                nextSessionInfo = new
                                {
                                    scheduled = true,
                                    scheduleId = newScheduleId,
                                    sessionDate = nextDate.Value.ToString("yyyy-MM-dd"),
                                    hdCycle = patient.HDCycle,
                                    message = $"Next session automatically scheduled for {nextDate.Value:MMM dd, yyyy}"
                                };
                                
                                _logger.LogInformation($"Auto-scheduled next session for patient {patient.PatientID} on {nextDate.Value:yyyy-MM-dd}");
                            }
                            else
                            {
                                nextSessionInfo = new
                                {
                                    scheduled = false,
                                    message = "Session already exists for next date",
                                    existingSessionId = existingSession.ScheduleID,
                                    sessionDate = nextDate.Value.ToString("yyyy-MM-dd")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auto-scheduling next session for schedule {ScheduleId}", id);
                    // Don't fail the discharge if auto-scheduling fails
                }
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                discharged = true,
                message = "Patient discharged successfully",
                nextSession = nextSessionInfo
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discharging schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while discharging the patient"));
        }
    }

    [HttpPut("{id}/activate")]
    [AllowAnonymous] // Temporarily allow anonymous for debugging
    public async Task<ActionResult<ApiResponse<bool>>> ActivateSession(int id)
    {
        try
        {
            _logger.LogInformation($"Activating session {id}");
            
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            // Update session status to indicate it's active (in treatment)
            // Only update SessionStatus - don't update StartTime as it may not exist in database
            var updates = new Dictionary<string, object>
            {
                { "SessionStatus", "In Progress" }
            };
            
            var result = await _scheduleRepository.PartialUpdateAsync(id, updates);
            
            if (result)
            {
                _logger.LogInformation($"✅ Session {id} activated successfully");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Session activated successfully"));
            }
            else
            {
                _logger.LogWarning($"⚠️ Failed to update session {id}");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to activate session"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating session {ScheduleId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
        }
    }

    // Helper method to calculate next dialysis date
    private DateTime? CalculateNextDialysisDate(string hdCycle, DateTime lastSessionDate)
    {
        if (string.IsNullOrEmpty(hdCycle))
            return null;

        var cycle = hdCycle.Trim().ToLower();
        
        // Handle "Every X days" pattern
        if (cycle.Contains("every") && cycle.Contains("day"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(cycle, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int days))
            {
                return lastSessionDate.AddDays(days);
            }
        }
        
        // Handle specific patterns
        if (cycle == "daily" || cycle == "everyday")
            return lastSessionDate.AddDays(1);
        else if (cycle.Contains("alternate") || cycle == "every 2 days")
            return lastSessionDate.AddDays(2);
        else if (cycle.Contains("3x/week") || cycle.Contains("3 times"))
            return lastSessionDate.AddDays(2); // Approximate
        else if (cycle.Contains("2x/week") || cycle.Contains("2 times"))
            return lastSessionDate.AddDays(3); // Approximate
        
        return null;
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSchedule(int id)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            var result = await _scheduleRepository.DeleteAsync(id);
            
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Schedule deleted successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete schedule"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the schedule"));
        }
    }

    // ==================== EQUIPMENT USAGE ALERT ENDPOINTS ====================

    /// <summary>
    /// Get suggested equipment counts for a new session (auto-increment from last session)
    /// </summary>
    [HttpGet("patient/{patientId}/suggested-equipment-counts")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetSuggestedEquipmentCounts(int patientId)
    {
        try
        {
            var (dialyserCount, bloodTubingCount) = await _scheduleRepository.GetLatestEquipmentCountsAsync(patientId);
            
            var result = new
            {
                dialyserReuseCount = dialyserCount,
                bloodTubingReuse = bloodTubingCount,
                message = dialyserCount == 1 && bloodTubingCount == 1 
                    ? "First session for this patient - starting with count 1"
                    : $"Auto-incremented from previous session (Dialyser: {dialyserCount-1} → {dialyserCount}, Blood Tubing: {bloodTubingCount-1} → {bloodTubingCount})"
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggested equipment counts for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while getting equipment counts"));
        }
    }

    /// <summary>
    /// Get equipment usage status for a patient's latest session
    /// Returns warnings for Dialyser (max 7 uses) and Blood Tubing (max 12 uses)
    /// </summary>
    [HttpGet("patient/{patientId}/equipment-status")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<EquipmentUsageStatus>>>> GetEquipmentUsageStatus(int patientId)
    {
        try
        {
            var statuses = await _equipmentUsageService.CheckEquipmentUsageAsync(patientId);
            return Ok(ApiResponse<List<EquipmentUsageStatus>>.SuccessResponse(statuses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking equipment usage for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<List<EquipmentUsageStatus>>.ErrorResponse(
                "An error occurred while checking equipment usage"));
        }
    }

    /// <summary>
    /// Get equipment usage status for a specific schedule
    /// </summary>
    [HttpGet("{scheduleId}/equipment-status")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<EquipmentUsageStatus>>>> GetScheduleEquipmentStatus(int scheduleId)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(ApiResponse<List<EquipmentUsageStatus>>.ErrorResponse("Schedule not found"));
            }

            var statuses = await _equipmentUsageService.CheckEquipmentUsageAsync(schedule.PatientID, scheduleId);
            return Ok(ApiResponse<List<EquipmentUsageStatus>>.SuccessResponse(statuses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking equipment usage for schedule {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<List<EquipmentUsageStatus>>.ErrorResponse(
                "An error occurred while checking equipment usage"));
        }
    }

    /// <summary>
    /// Get all unacknowledged equipment alerts for a patient
    /// </summary>
    [HttpGet("patient/{patientId}/equipment-alerts")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<EquipmentUsageAlert>>>> GetPatientEquipmentAlerts(int patientId)
    {
        try
        {
            var alerts = await _equipmentUsageService.GetUnacknowledgedAlertsAsync(patientId);
            return Ok(ApiResponse<List<EquipmentUsageAlert>>.SuccessResponse(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment alerts for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<List<EquipmentUsageAlert>>.ErrorResponse(
                "An error occurred while retrieving equipment alerts"));
        }
    }

    /// <summary>
    /// Check and create equipment alerts for a schedule (called after creating/updating session)
    /// </summary>
    [HttpPost("{scheduleId}/check-equipment-alerts")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<List<EquipmentUsageAlert>>>> CheckAndCreateEquipmentAlerts(int scheduleId)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(ApiResponse<List<EquipmentUsageAlert>>.ErrorResponse("Schedule not found"));
            }

            var alerts = await _equipmentUsageService.CheckAndCreateAlertsForScheduleAsync(
                schedule.PatientID, scheduleId);
            
            if (alerts.Any())
            {
                _logger.LogWarning("Equipment usage alerts created for schedule {ScheduleId}: {AlertCount} alert(s)", 
                    scheduleId, alerts.Count);
            }

            return Ok(ApiResponse<List<EquipmentUsageAlert>>.SuccessResponse(alerts, 
                alerts.Any() ? "Equipment usage alerts generated" : "No alerts needed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking equipment alerts for schedule {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<List<EquipmentUsageAlert>>.ErrorResponse(
                "An error occurred while checking equipment alerts"));
        }
    }

    /// <summary>
    /// Acknowledge an equipment usage alert (mark as seen by staff)
    /// </summary>
    [HttpPut("equipment-alerts/{alertId}/acknowledge")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> AcknowledgeEquipmentAlert(
        int alertId, [FromBody] AcknowledgeAlertRequest request)
    {
        try
        {
            var result = await _equipmentUsageService.AcknowledgeAlertAsync(alertId, request.AcknowledgedBy);
            
            if (result)
            {
                _logger.LogInformation("Equipment alert {AlertId} acknowledged by {User}", 
                    alertId, request.AcknowledgedBy);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Alert acknowledged successfully"));
            }
            
            return NotFound(ApiResponse<bool>.ErrorResponse("Alert not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging equipment alert {AlertId}", alertId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "An error occurred while acknowledging the alert"));
        }
    }

    private object ConvertJsonNumber(System.Text.Json.JsonElement element)
    {
        // Try to get as int32 first (most common for our use case)
        if (element.TryGetInt32(out var intValue))
            return intValue;
        
        // Try decimal for precise numbers
        if (element.TryGetDecimal(out var decimalValue))
            return decimalValue;
        
        // Fall back to double
        if (element.TryGetDouble(out var doubleValue))
            return doubleValue;
        
        // Last resort - parse as string
        return element.ToString();
    }

    // ==================== INTRA-DIALYTIC MONITORING ENDPOINTS ====================

    /// <summary>
    /// Get all monitoring records for a specific HD session
    /// </summary>
    [HttpGet("{scheduleId}/monitoring")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetMonitoringRecords(int scheduleId)
    {
        try
        {
            var records = await _scheduleRepository.GetIntraDialyticRecordsAsync(scheduleId);
            return Ok(ApiResponse<List<object>>.SuccessResponse(
                records.ToList<object>(), 
                $"Retrieved {records.Count()} monitoring records"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring records for schedule {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                "An error occurred while retrieving monitoring records"));
        }
    }

    /// <summary>
    /// Add a new monitoring record during HD session
    /// </summary>
    [HttpPost("monitoring")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<int>>> AddMonitoringRecord([FromBody] IntraDialyticRecordDTO record)
    {
        try
        {
            // Get the staff ID from the authenticated user's claims
            var staffIdClaim = User.FindFirst("StaffID")?.Value;
            if (int.TryParse(staffIdClaim, out int staffId))
            {
                record.RecordedBy = staffId;
            }
            else
            {
                // Fallback: try to get from NameIdentifier claim
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    record.RecordedBy = userId;
                }
            }
            
            var recordId = await _scheduleRepository.CreateIntraDialyticRecordAsync(record);
            
            _logger.LogInformation("Monitoring record created with ID {RecordId} by staff {StaffId}", recordId, record.RecordedBy);
            return Ok(ApiResponse<int>.SuccessResponse(recordId, "Monitoring record saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating monitoring record");
            return StatusCode(500, ApiResponse<int>.ErrorResponse(
                "An error occurred while saving the monitoring record"));
        }
    }

    /// <summary>
    /// Add post-dialysis medication for a session
    /// </summary>
    [HttpPost("{id}/medications")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<int>>> AddPostDialysisMedication(int id, [FromBody] PostDialysisMedicationDTO medication)
    {
        try
        {
            medication.ScheduleID = id;
            var medicationId = await _scheduleRepository.CreatePostDialysisMedicationAsync(medication);
            
            _logger.LogInformation("Medication added for schedule {ScheduleId}: {MedicationName}", id, medication.MedicationName);
            return Ok(ApiResponse<int>.SuccessResponse(medicationId, "Medication saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medication for schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<int>.ErrorResponse("An error occurred while saving the medication"));
        }
    }

    /// <summary>
    /// Get post-dialysis medications for a session
    /// </summary>
    [HttpGet("{id}/medications")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetPostDialysisMedications(int id)
    {
        try
        {
            var medications = await _scheduleRepository.GetPostDialysisMedicationsByScheduleIdAsync(id);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(medications, "Medications retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medications for schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<object>>.ErrorResponse("An error occurred while retrieving medications"));
        }
    }

    /// <summary>
    /// Add treatment alert for a session
    /// </summary>
    [HttpPost("{id}/alerts")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<int>>> AddTreatmentAlert(int id, [FromBody] TreatmentAlertDTO alert)
    {
        try
        {
            alert.ScheduleID = id;
            var alertId = await _scheduleRepository.CreateTreatmentAlertAsync(alert);
            
            _logger.LogInformation("Alert added for schedule {ScheduleId}: {AlertType}", id, alert.AlertType);
            return Ok(ApiResponse<int>.SuccessResponse(alertId, "Alert saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding alert for schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<int>.ErrorResponse("An error occurred while saving the alert"));
        }
    }

    /// <summary>
    /// Get treatment alerts for a session
    /// </summary>
    [HttpGet("{id}/alerts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetTreatmentAlerts(int id)
    {
        try
        {
            var alerts = await _scheduleRepository.GetTreatmentAlertsByScheduleIdAsync(id);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(alerts, "Alerts retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<object>>.ErrorResponse("An error occurred while retrieving alerts"));
        }
    }

    /// <summary>
    /// Force discharge a patient session (mark as complete)
    /// </summary>
    [HttpPost("force-discharge/{scheduleId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> ForceDischargeSession(int scheduleId)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            if (schedule.IsDischarged)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Session is already discharged"));
            }

            // Mark session as discharged
            var result = await _scheduleRepository.DischargeAsync(scheduleId);
            
            if (result)
            {
                _logger.LogInformation("Session {ScheduleId} force discharged successfully", scheduleId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Session discharged successfully"));
            }
            else
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to discharge session"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force discharging session {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while discharging the session"));
        }
    }

    /// <summary>
    /// Delete a monitoring record
    /// </summary>
    [HttpDelete("monitoring/{recordId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMonitoringRecord(int recordId)
    {
        try
        {
            var result = await _scheduleRepository.DeleteIntraDialyticRecordAsync(recordId);
            
            if (result)
            {
                _logger.LogInformation("Monitoring record {RecordId} deleted", recordId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Monitoring record deleted successfully"));
            }
            
            return NotFound(ApiResponse<bool>.ErrorResponse("Monitoring record not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting monitoring record {RecordId}", recordId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "An error occurred while deleting the monitoring record"));
        }
    }
    
    /// <summary>
    /// Change bed assignment for a session (allows staff to reassign beds flexibly)
    /// </summary>
    [HttpPatch("{scheduleId}/change-bed")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeBed(int scheduleId, [FromBody] ChangeBedRequest request)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }
            
            _logger.LogInformation("🔄 Changing bed for Schedule {ScheduleId} from Bed {OldBed} to Bed {NewBed}",
                scheduleId, schedule.BedNumber, request.NewBedNumber);
            
            // Update bed assignment
            schedule.BedNumber = request.NewBedNumber;
            schedule.UpdatedAt = DateTime.Now;
            
            var result = await _scheduleRepository.UpdateAsync(schedule);
            
            if (result)
            {
                _logger.LogInformation("✅ Bed changed successfully for Schedule {ScheduleId}", scheduleId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, $"Bed changed to Bed {request.NewBedNumber} successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update bed assignment"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing bed for schedule {ScheduleId}", scheduleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while changing the bed assignment"));
        }
    }

    // ==================== MISSED APPOINTMENT MANAGEMENT ====================

    /// <summary>
    /// Auto-detect possible no-shows (sessions that should have started but haven't been activated)
    /// </summary>
    [HttpGet("possible-no-shows")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<PossibleNoShowDTO>>>> GetPossibleNoShows([FromQuery] int minutesThreshold = 60)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            var query = @"
                SELECT 
                    s.ScheduleID,
                    s.PatientID,
                    p.Name AS PatientName,
                    s.SessionDate,
                    s.SlotID,
                    slot.SlotName,
                    slot.StartTime AS SlotStartTime,
                    DATEDIFF(MINUTE, 
                        DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), 
                            CAST(s.SessionDate AS DATETIME)), 
                        GETDATE()) AS MinutesLate
                FROM HDSchedule s
                INNER JOIN Patients p ON s.PatientID = p.PatientID
                LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
                WHERE CAST(s.SessionDate AS DATE) = CAST(GETDATE() AS DATE)
                    AND s.SessionStatus = 'Pre-Scheduled'
                    AND s.IsDischarged = 0
                    AND s.IsMovedToHistory = 0
                    AND ISNULL(s.IsMissed, 0) = 0
                    AND slot.StartTime IS NOT NULL
                    AND DATEDIFF(MINUTE, 
                        DATEADD(SECOND, DATEDIFF(SECOND, '00:00:00', slot.StartTime), 
                            CAST(s.SessionDate AS DATETIME)), 
                        GETDATE()) > @MinutesThreshold
                ORDER BY MinutesLate DESC";

            var possibleNoShows = await connection.QueryAsync<PossibleNoShowDTO>(query, new { MinutesThreshold = minutesThreshold });
            
            foreach (var noShow in possibleNoShows)
            {
                noShow.IsAutoDetected = true;
            }

            return Ok(ApiResponse<List<PossibleNoShowDTO>>.SuccessResponse(possibleNoShows.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting possible no-shows");
            return StatusCode(500, ApiResponse<List<PossibleNoShowDTO>>.ErrorResponse("An error occurred while detecting no-shows"));
        }
    }

    /// <summary>
    /// Update a session's time slot (for rescheduling late patients)
    /// </summary>
    [HttpPut("{scheduleId}/slot")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSessionSlot(int scheduleId, [FromBody] UpdateSlotDTO request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check if schedule exists and can be rescheduled
            var schedule = await connection.QueryFirstOrDefaultAsync<HDSchedule>(
                "SELECT * FROM HDSchedule WHERE ScheduleID = @ScheduleID", 
                new { ScheduleID = scheduleId });

            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            if (schedule.SessionStatus == "Completed" || schedule.IsDischarged == true)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot reschedule completed or discharged sessions"));
            }

            // Verify the new slot exists
            var slotExists = await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Slots WHERE SlotID = @SlotID) THEN 1 ELSE 0 END",
                new { SlotID = request.SlotId });

            if (!slotExists)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid slot ID"));
            }

            // Update the slot
            var updated = await connection.ExecuteAsync(
                "UPDATE HDSchedule SET SlotID = @SlotID WHERE ScheduleID = @ScheduleID",
                new { SlotID = request.SlotId, ScheduleID = scheduleId });

            if (updated > 0)
            {
                _logger.LogInformation($"Session {scheduleId} rescheduled to slot {request.SlotId}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Session rescheduled successfully"));
            }
            else
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update session slot"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session slot");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while rescheduling the session"));
        }
    }

    /// <summary>
    /// Reschedule a session to a new date
    /// </summary>
    [HttpPut("{scheduleId}/reschedule")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> RescheduleSession(int scheduleId, [FromBody] RescheduleSessionDTO request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            // Check if schedule exists and can be rescheduled
            var schedule = await connection.QueryFirstOrDefaultAsync<HDSchedule>(
                "SELECT * FROM HDSchedule WHERE ScheduleID = @ScheduleID", 
                new { ScheduleID = scheduleId });

            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            if (schedule.SessionStatus == "Completed" || schedule.IsDischarged == true)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot reschedule completed or discharged sessions"));
            }

            // Parse the new date
            if (!DateTime.TryParse(request.NewDate, out DateTime newDate))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid date format. Expected YYYY-MM-DD"));
            }

            // Update the session date
            var updated = await connection.ExecuteAsync(
                "UPDATE HDSchedule SET SessionDate = @NewDate, UpdatedAt = GETDATE() WHERE ScheduleID = @ScheduleID",
                new { NewDate = newDate.Date, ScheduleID = scheduleId });

            if (updated > 0)
            {
                _logger.LogInformation($"Session {scheduleId} rescheduled from {schedule.SessionDate:yyyy-MM-dd} to {newDate:yyyy-MM-dd}");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Session rescheduled successfully"));
            }
            else
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update session date"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling session");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while rescheduling the session"));
        }
    }

    /// <summary>
    /// Mark a session as missed (no-show)
    /// </summary>
    [HttpPost("mark-missed")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkMissedAppointment([FromBody] MarkMissedAppointmentDTO request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            using var connection = _context.CreateConnection();
            
            // Check if schedule exists and is eligible to be marked as missed
            var schedule = await connection.QueryFirstOrDefaultAsync<HDSchedule>(
                "SELECT * FROM HDSchedule WHERE ScheduleID = @ScheduleID", 
                new { request.ScheduleID });

            if (schedule == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Schedule not found"));
            }

            if (schedule.SessionStatus == "Active" || schedule.SessionStatus == "Completed")
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot mark an active or completed session as missed"));
            }

            // Mark as missed
            var updateQuery = @"
                UPDATE HDSchedule
                SET IsMissed = 1,
                    MissedReason = @MissedReason,
                    MissedNotes = @MissedNotes,
                    MissedDateTime = GETDATE(),
                    MissedMarkedByUserID = @UserId,
                    SessionStatus = 'Missed',
                    UpdatedAt = GETDATE()
                WHERE ScheduleID = @ScheduleID";

            await connection.ExecuteAsync(updateQuery, new
            {
                request.ScheduleID,
                request.MissedReason,
                request.MissedNotes,
                UserId = userId
            });

            // Record the missed appointment in patient activity log
            var activity = new PatientActivityLog
            {
                PatientID = schedule.PatientID,
                ScheduleID = request.ScheduleID,
                ActivityDate = schedule.SessionDate,
                ActivityType = "MISSED",
                Reason = request.MissedReason ?? "No-Show",
                Details = request.MissedNotes ?? "Patient did not arrive for scheduled session",
                RecordedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            try
            {
                await _activityRepository.CreateActivityAsync(activity);
                _logger.LogInformation("Missed appointment recorded in patient activity log for PatientID {PatientID}", schedule.PatientID);
            }
            catch (Exception activityEx)
            {
                _logger.LogWarning(activityEx, "Failed to record missed appointment in activity log, but session was marked as missed");
            }

            _logger.LogInformation("Session {ScheduleID} marked as missed by user {UserId}. Reason: {Reason}", 
                request.ScheduleID, userId, request.MissedReason);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment marked as missed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking appointment as missed");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while marking the appointment as missed"));
        }
    }

    /// <summary>
    /// Get all missed appointments for a patient
    /// </summary>
    [HttpGet("patient/{patientId}/missed-appointments")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<MissedAppointmentDetailsDTO>>>> GetPatientMissedAppointments(int patientId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            var query = @"
                SELECT 
                    s.ScheduleID,
                    s.PatientID,
                    p.Name AS PatientName,
                    s.SessionDate,
                    s.SlotID,
                    slot.SlotName,
                    s.IsMissed,
                    s.MissedReason,
                    s.MissedNotes,
                    s.MissedDateTime,
                    u.Username AS MissedMarkedBy,
                    CASE WHEN s.MissedResolvedDateTime IS NOT NULL THEN 1 ELSE 0 END AS IsResolved,
                    s.MissedResolvedDateTime,
                    s.MissedResolutionNotes
                FROM HDSchedule s
                INNER JOIN Patients p ON s.PatientID = p.PatientID
                LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
                LEFT JOIN Users u ON s.MissedMarkedByUserID = u.UserID
                WHERE s.PatientID = @PatientId 
                    AND s.IsMissed = 1
                ORDER BY s.SessionDate DESC";

            var missedAppointments = await connection.QueryAsync<MissedAppointmentDetailsDTO>(query, new { PatientId = patientId });

            return Ok(ApiResponse<List<MissedAppointmentDetailsDTO>>.SuccessResponse(missedAppointments.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving missed appointments for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<List<MissedAppointmentDetailsDTO>>.ErrorResponse("An error occurred while retrieving missed appointments"));
        }
    }

    /// <summary>
    /// Resolve a missed appointment (allow patient to schedule again)
    /// </summary>
    [HttpPost("resolve-missed")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveMissedAppointment([FromBody] ResolveMissedAppointmentDTO request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            using var connection = _context.CreateConnection();
            
            var updateQuery = @"
                UPDATE HDSchedule
                SET MissedResolvedDateTime = GETDATE(),
                    MissedResolvedByUserID = @UserId,
                    MissedResolutionNotes = @ResolutionNotes,
                    UpdatedAt = GETDATE()
                WHERE ScheduleID = @ScheduleID AND IsMissed = 1";

            var rowsAffected = await connection.ExecuteAsync(updateQuery, new
            {
                request.ScheduleID,
                request.ResolutionNotes,
                UserId = userId
            });

            if (rowsAffected == 0)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Missed appointment not found or already resolved"));
            }

            _logger.LogInformation("Missed appointment {ScheduleID} resolved by user {UserId}", request.ScheduleID, userId);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Missed appointment resolved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving missed appointment");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while resolving the missed appointment"));
        }
    }

    /// <summary>
    /// Get all unresolved missed appointments (blocking scheduling)
    /// </summary>
    [HttpGet("patient/{patientId}/unresolved-missed")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<List<MissedAppointmentDetailsDTO>>>> GetUnresolvedMissedAppointments(int patientId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            
            var query = @"
                SELECT 
                    s.ScheduleID,
                    s.PatientID,
                    p.Name AS PatientName,
                    s.SessionDate,
                    s.SlotID,
                    slot.SlotName,
                    s.IsMissed,
                    s.MissedReason,
                    s.MissedNotes,
                    s.MissedDateTime,
                    u.Username AS MissedMarkedBy
                FROM HDSchedule s
                INNER JOIN Patients p ON s.PatientID = p.PatientID
                LEFT JOIN Slots slot ON s.SlotID = slot.SlotID
                LEFT JOIN Users u ON s.MissedMarkedByUserID = u.UserID
                WHERE s.PatientID = @PatientId 
                    AND s.IsMissed = 1
                    AND s.MissedResolvedDateTime IS NULL
                ORDER BY s.SessionDate DESC";

            var unresolvedMissed = await connection.QueryAsync<MissedAppointmentDetailsDTO>(query, new { PatientId = patientId });

            return Ok(ApiResponse<List<MissedAppointmentDetailsDTO>>.SuccessResponse(unresolvedMissed.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unresolved missed appointments for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<List<MissedAppointmentDetailsDTO>>.ErrorResponse("An error occurred while retrieving unresolved missed appointments"));
        }
    }
}

public class ChangeBedRequest
{
    public int NewBedNumber { get; set; }
}
