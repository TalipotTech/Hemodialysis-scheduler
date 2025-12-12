using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Repositories;
using HDScheduler.API.Services;
using HDScheduler.API.DTOs;
using HDScheduler.API.Models;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IHDCycleService _hdCycleService;
    private readonly IBedAssignmentService _bedAssignmentService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(
        IHDScheduleRepository scheduleRepository,
        IPatientRepository patientRepository,
        IHDCycleService hdCycleService,
        IBedAssignmentService bedAssignmentService,
        ILogger<ReservationController> logger)
    {
        _scheduleRepository = scheduleRepository;
        _patientRepository = patientRepository;
        _hdCycleService = hdCycleService;
        _bedAssignmentService = bedAssignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get reservation statistics showing reserved vs active patients based on HD cycles
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetReservationStatistics([FromQuery] string? date = null)
    {
        try
        {
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var allSchedules = await _scheduleRepository.GetAllAsync();
            var allPatients = await _patientRepository.GetAllAsync();

            // Get today's schedules (Active patients - currently undergoing treatment)
            var todaySchedules = allSchedules.Where(s => 
                s.SessionDate.Date == targetDate && 
                !s.IsDischarged && 
                !s.IsMovedToHistory).ToList();

            // Get future schedules (Reserved patients - scheduled for future based on HD cycle)
            var futureSchedules = allSchedules.Where(s => 
                s.SessionDate.Date > targetDate && 
                !s.IsDischarged && 
                !s.IsMovedToHistory).ToList();

            // Active patients count (today)
            var activePatientIds = todaySchedules.Select(s => s.PatientID).Distinct().ToList();
            var activePatients = activePatientIds.Count;

            // Reserved patients count (future sessions)
            var reservedPatientIds = futureSchedules.Select(s => s.PatientID).Distinct().ToList();
            var reservedPatients = reservedPatientIds.Count;

            // Get HD cycle breakdown for active patients
            var activePatientsWithCycles = new List<object>();
            foreach (var patientId in activePatientIds)
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient != null)
                {
                    var patientSchedules = todaySchedules.Where(s => s.PatientID == patientId).ToList();
                    activePatientsWithCycles.Add(new
                    {
                        patientId = patient.PatientID,
                        name = patient.Name,
                        mrn = patient.MRN,
                        hdCycle = patient.HDCycle,
                        todaysSessions = patientSchedules.Count,
                        status = "Active"
                    });
                }
            }

            // Get HD cycle breakdown for reserved patients
            var reservedPatientsWithCycles = new List<object>();
            foreach (var patientId in reservedPatientIds)
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient != null)
                {
                    var patientSchedules = futureSchedules.Where(s => s.PatientID == patientId).ToList();
                    var nextSession = patientSchedules.OrderBy(s => s.SessionDate).FirstOrDefault();
                    
                    reservedPatientsWithCycles.Add(new
                    {
                        patientId = patient.PatientID,
                        name = patient.Name,
                        mrn = patient.MRN,
                        hdCycle = patient.HDCycle,
                        futureSessionsCount = patientSchedules.Count,
                        nextSessionDate = nextSession?.SessionDate.ToString("yyyy-MM-dd"),
                        status = "Reserved"
                    });
                }
            }

            // Count by HD cycle type
            var cycleCounts = new Dictionary<string, object>();
            var allActivePatients = await Task.WhenAll(activePatientIds.Select(id => _patientRepository.GetByIdAsync(id)));
            var allReservedPatients = await Task.WhenAll(reservedPatientIds.Select(id => _patientRepository.GetByIdAsync(id)));

            var groupedByCycle = allActivePatients.Where(p => p != null)
                .GroupBy(p => p!.HDCycle ?? "Not Set")
                .Select(g => new
                {
                    hdCycle = g.Key,
                    activeCount = g.Count(),
                    reservedCount = allReservedPatients.Count(p => p != null && (p.HDCycle ?? "Not Set") == g.Key)
                });

            var statistics = new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                summary = new
                {
                    totalActive = activePatients,
                    totalReserved = reservedPatients,
                    totalPatients = activePatients + reservedPatients,
                    activeSessions = todaySchedules.Count,
                    futureSessions = futureSchedules.Count
                },
                byCycle = groupedByCycle,
                activePatients = activePatientsWithCycles,
                reservedPatients = reservedPatientsWithCycles
            };

            return Ok(ApiResponse<object>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation statistics");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reservation statistics"));
        }
    }

    /// <summary>
    /// Auto-schedule next session for a patient based on their HD cycle
    /// </summary>
    [HttpPost("auto-schedule-next/{scheduleId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> AutoScheduleNextSession(int scheduleId)
    {
        try
        {
            // Get the current session
            var currentSession = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (currentSession == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));
            }

            // Get patient details
            var patient = await _patientRepository.GetByIdAsync(currentSession.PatientID);
            if (patient == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Patient not found"));
            }

            if (string.IsNullOrEmpty(patient.HDCycle))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Patient does not have an HD cycle defined"));
            }

            // Calculate next dialysis date
            var nextDate = _hdCycleService.CalculateNextDialysisDate(patient.HDCycle, currentSession.SessionDate);
            if (!nextDate.HasValue)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Unable to calculate next date for HD cycle: {patient.HDCycle}"));
            }

            // Check if session already exists for that date
            var existingSession = await _scheduleRepository.GetByPatientAndDateAsync(patient.PatientID, nextDate.Value);
            if (existingSession != null)
            {
                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    message = "Session already exists for next date",
                    existingSessionId = existingSession.ScheduleID,
                    sessionDate = nextDate.Value.ToString("yyyy-MM-dd")
                }));
            }

            // Create new session based on current session template
            var newSession = new HDSchedule
            {
                PatientID = patient.PatientID,
                SessionDate = nextDate.Value,
                SlotID = currentSession.SlotID, // Keep same time slot
                BedNumber = currentSession.BedNumber, // Keep same bed if possible
                
                // Copy treatment parameters
                DryWeight = patient.DryWeight ?? currentSession.DryWeight,
                HDStartDate = patient.HDStartDate,
                HDCycle = patient.HDCycle,
                HDFrequency = patient.HDFrequency,
                
                DialyserType = patient.DialyserType ?? currentSession.DialyserType,
                DialyserModel = patient.DialyserModel ?? currentSession.DialyserModel,
                PrescribedDuration = patient.PrescribedDuration ?? currentSession.PrescribedDuration,
                PrescribedBFR = patient.PrescribedBFR ?? currentSession.PrescribedBFR,
                DialysatePrescription = patient.DialysatePrescription ?? currentSession.DialysatePrescription,
                
                // Copy staff assignments
                AssignedDoctor = currentSession.AssignedDoctor,
                AssignedNurse = currentSession.AssignedNurse,
                
                // Mark as pre-scheduled and auto-generated
                SessionStatus = "Pre-Scheduled",
                IsAutoGenerated = true,
                ParentScheduleID = scheduleId,
                
                IsDischarged = false,
                IsMovedToHistory = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var newScheduleId = await _scheduleRepository.CreateAsync(newSession);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "Next session scheduled successfully",
                scheduleId = newScheduleId,
                sessionDate = nextDate.Value.ToString("yyyy-MM-dd"),
                hdCycle = patient.HDCycle,
                patientName = patient.Name
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-scheduling next session");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while scheduling the next session"));
        }
    }

    /// <summary>
    /// Generate multiple future sessions for a patient based on their HD cycle
    /// </summary>
    [HttpPost("generate-schedule/{patientId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> GenerateScheduleForPatient(
        int patientId, 
        [FromQuery] int daysAhead = 30,
        [FromQuery] int? slotId = null,
        [FromQuery] int? bedNumber = null)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Patient not found"));
            }

            if (string.IsNullOrEmpty(patient.HDCycle))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Patient does not have an HD cycle defined"));
            }

            // Get patient's last session
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var lastSession = allSchedules
                .Where(s => s.PatientID == patientId)
                .OrderByDescending(s => s.SessionDate)
                .FirstOrDefault();

            DateTime startDate = lastSession?.SessionDate ?? DateTime.Today;
            
            // Get upcoming dates based on HD cycle
            var upcomingDates = _hdCycleService.GetUpcomingDialysisDates(patient.HDCycle, startDate, daysAhead);

            var createdSessions = new List<object>();
            var skippedDates = new List<string>();

            foreach (var date in upcomingDates)
            {
                // Check if session already exists
                var existing = await _scheduleRepository.GetByPatientAndDateAsync(patientId, date);
                if (existing != null)
                {
                    skippedDates.Add(date.ToString("yyyy-MM-dd"));
                    continue;
                }

                // Create new session
                var newSession = new HDSchedule
                {
                    PatientID = patientId,
                    SessionDate = date,
                    SlotID = slotId ?? lastSession?.SlotID,
                    BedNumber = bedNumber ?? lastSession?.BedNumber,
                    
                    DryWeight = patient.DryWeight,
                    HDStartDate = patient.HDStartDate,
                    HDCycle = patient.HDCycle,
                    HDFrequency = patient.HDFrequency,
                    
                    DialyserType = patient.DialyserType,
                    DialyserModel = patient.DialyserModel,
                    PrescribedDuration = patient.PrescribedDuration,
                    PrescribedBFR = patient.PrescribedBFR,
                    DialysatePrescription = patient.DialysatePrescription,
                    
                    AssignedDoctor = lastSession?.AssignedDoctor,
                    AssignedNurse = lastSession?.AssignedNurse,
                    
                    SessionStatus = "Pre-Scheduled",
                    IsAutoGenerated = true,
                    ParentScheduleID = lastSession?.ScheduleID,
                    
                    IsDischarged = false,
                    IsMovedToHistory = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var newScheduleId = await _scheduleRepository.CreateAsync(newSession);
                
                createdSessions.Add(new
                {
                    scheduleId = newScheduleId,
                    sessionDate = date.ToString("yyyy-MM-dd")
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = $"Generated {createdSessions.Count} future sessions",
                patientId = patientId,
                patientName = patient.Name,
                hdCycle = patient.HDCycle,
                daysAhead = daysAhead,
                sessionsCreated = createdSessions.Count,
                sessionsDuplicated = skippedDates.Count,
                createdSessions = createdSessions,
                skippedDates = skippedDates
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schedule for patient");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while generating the schedule"));
        }
    }

    /// <summary>
    /// Get all patients with their reservation status (Active or Reserved)
    /// </summary>
    [HttpGet("patients-status")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetPatientsWithReservationStatus([FromQuery] string? date = null)
    {
        try
        {
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var allPatients = await _patientRepository.GetAllAsync();
            var allSchedules = await _scheduleRepository.GetAllAsync();

            var patientsWithStatus = new List<object>();

            foreach (var patient in allPatients.Where(p => p.IsActive))
            {
                var todaySchedule = allSchedules.FirstOrDefault(s => 
                    s.PatientID == patient.PatientID && 
                    s.SessionDate.Date == targetDate && 
                    !s.IsDischarged && 
                    !s.IsMovedToHistory);

                var futureSchedules = allSchedules.Where(s => 
                    s.PatientID == patient.PatientID && 
                    s.SessionDate.Date > targetDate && 
                    s.SessionStatus == "Pre-Scheduled" && // Only pre-scheduled sessions
                    !s.IsDischarged && 
                    !s.IsMovedToHistory)
                    .OrderBy(s => s.SessionDate)
                    .ToList();

                var lastSession = allSchedules.Where(s => s.PatientID == patient.PatientID)
                    .OrderByDescending(s => s.SessionDate)
                    .FirstOrDefault();

                DateTime? nextExpectedDate = null;
                string nextExpectedDay = null;
                if (!string.IsNullOrEmpty(patient.HDCycle) && lastSession != null)
                {
                    nextExpectedDate = _hdCycleService.CalculateNextDialysisDate(patient.HDCycle, lastSession.SessionDate);
                    if (nextExpectedDate.HasValue)
                    {
                        nextExpectedDay = nextExpectedDate.Value.ToString("dddd, MMM dd"); // "Monday, Dec 13"
                    }
                }

                string status = "Inactive";
                if (todaySchedule != null && todaySchedule.SessionStatus == "Active")
                {
                    status = "Active";
                }
                else if (todaySchedule != null && todaySchedule.SessionStatus == "Pre-Scheduled")
                {
                    status = "Reserved"; // Has session today but not yet activated
                }
                else if (futureSchedules.Any())
                {
                    status = "Reserved";
                }

                var nextSession = futureSchedules.FirstOrDefault();

                patientsWithStatus.Add(new
                {
                    patientId = patient.PatientID,
                    name = patient.Name,
                    mrn = patient.MRN,
                    age = patient.Age,
                    gender = patient.Gender,
                    hdCycle = patient.HDCycle,
                    hdFrequency = patient.HDFrequency,
                    status = status,
                    todaySession = todaySchedule != null ? new
                    {
                        scheduleId = todaySchedule.ScheduleID,
                        slotId = todaySchedule.SlotID,
                        bedNumber = todaySchedule.BedNumber,
                        sessionStatus = todaySchedule.SessionStatus
                    } : null,
                    futureSessionsCount = futureSchedules.Count,
                    nextScheduledDate = nextSession?.SessionDate.ToString("yyyy-MM-dd"),
                    nextScheduledDay = nextSession?.SessionDate.ToString("dddd, MMM dd"), // "Monday, Dec 13"
                    nextExpectedDate = nextExpectedDate?.ToString("yyyy-MM-dd"),
                    nextExpectedDay = nextExpectedDay,
                    lastSessionDate = lastSession?.SessionDate.ToString("yyyy-MM-dd")
                });
            }

            var summary = new
            {
                totalPatients = patientsWithStatus.Count,
                activeToday = patientsWithStatus.Count(p => ((dynamic)p).status == "Active"),
                reserved = patientsWithStatus.Count(p => ((dynamic)p).status == "Reserved"),
                inactive = patientsWithStatus.Count(p => ((dynamic)p).status == "Inactive")
            };

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                summary = summary,
                patients = patientsWithStatus
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients with reservation status");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// Activate a reserved patient for TODAY - moves patient from Reserved to Active
    /// Finds today's pre-scheduled session and changes status from "Pre-Scheduled" to "Active"
    /// Bed color changes from purple to red in the schedule grid
    /// </summary>
    [HttpPost("activate/{patientId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateReservedPatient(int patientId)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Patient not found"));
            }

            // Find today's pre-scheduled session for this patient
            var todaySession = await _scheduleRepository.GetByPatientAndDateAsync(patientId, DateTime.Today);
            
            if (todaySession == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No pre-scheduled session found for {patient.Name} today ({DateTime.Today:yyyy-MM-dd})"));
            }

            if (todaySession.SessionStatus == "Active")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Patient {patient.Name} is already active today"));
            }

            if (todaySession.SessionStatus != "Pre-Scheduled")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Cannot activate session with status: {todaySession.SessionStatus}"));
            }

            // Change status from Pre-Scheduled to Active
            todaySession.SessionStatus = "Active";
            todaySession.UpdatedAt = DateTime.Now;
            
            // If no bed assigned yet, assign one now
            if (!todaySession.BedNumber.HasValue && todaySession.SlotID.HasValue)
            {
                var availableBed = await _bedAssignmentService.GetNextAvailableBedAsync(todaySession.SlotID.Value, DateTime.Today);
                if (availableBed.HasValue)
                {
                    todaySession.BedNumber = availableBed.Value;
                }
            }

            await _scheduleRepository.UpdateAsync(todaySession);

            _logger.LogInformation("✅ Patient {PatientName} (ID: {PatientId}) activated for today. Session {ScheduleId} changed from Pre-Scheduled to Active. Bed: {Bed}",
                patient.Name, patientId, todaySession.ScheduleID, todaySession.BedNumber?.ToString() ?? "Not Assigned");

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = $"Patient {patient.Name} activated successfully",
                patientId = patient.PatientID,
                patientName = patient.Name,
                scheduleId = todaySession.ScheduleID,
                sessionDate = todaySession.SessionDate.ToString("yyyy-MM-dd"),
                slotId = todaySession.SlotID,
                bedNumber = todaySession.BedNumber,
                sessionStatus = todaySession.SessionStatus
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating reserved patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while activating the patient"));
        }
    }

    /// <summary>
    /// Mark a pre-scheduled session as "Missed" when patient doesn't show up
    /// </summary>
    [HttpPost("mark-missed/{patientId}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> MarkSessionAsMissed(int patientId, [FromQuery] string? date = null)
    {
        try
        {
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Patient not found"));
            }

            var session = await _scheduleRepository.GetByPatientAndDateAsync(patientId, targetDate);
            
            if (session == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No session found for {patient.Name} on {targetDate:yyyy-MM-dd}"));
            }

            if (session.SessionStatus == "Completed")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Cannot mark completed session as missed"));
            }

            // Change status to Missed
            session.SessionStatus = "Missed";
            session.UpdatedAt = DateTime.Now;
            
            await _scheduleRepository.UpdateAsync(session);

            _logger.LogInformation("⚠️ Session marked as MISSED for patient {PatientName} (ID: {PatientId}) on {Date}",
                patient.Name, patientId, targetDate.ToString("yyyy-MM-dd"));

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = $"Session marked as missed for {patient.Name}",
                patientId = patient.PatientID,
                patientName = patient.Name,
                scheduleId = session.ScheduleID,
                sessionDate = session.SessionDate.ToString("yyyy-MM-dd"),
                sessionStatus = session.SessionStatus
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking session as missed for patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }
}
