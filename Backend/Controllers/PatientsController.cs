using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;
using HDScheduler.API.Services;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly IRecurringSessionService _recurringSessionService;
    private readonly IBedAssignmentService _bedAssignmentService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientRepository patientRepository,
        IHDScheduleRepository scheduleRepository,
        IRecurringSessionService recurringSessionService,
        IBedAssignmentService bedAssignmentService,
        ILogger<PatientsController> logger)
    {
        _patientRepository = patientRepository;
        _scheduleRepository = scheduleRepository;
        _recurringSessionService = recurringSessionService;
        _bedAssignmentService = bedAssignmentService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    // [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<Patient>>>> GetAllPatients()
    {
        try
        {
            var patients = await _patientRepository.GetAllAsync();
            return Ok(ApiResponse<List<Patient>>.SuccessResponse(patients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            return StatusCode(500, ApiResponse<List<Patient>>.ErrorResponse("An error occurred while retrieving patients"));
        }
    }

    [HttpGet("active")]
    [AllowAnonymous]
    // [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<Patient>>>> GetActivePatients()
    {
        try
        {
            var patients = await _patientRepository.GetActiveAsync();
            return Ok(ApiResponse<List<Patient>>.SuccessResponse(patients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active patients");
            return StatusCode(500, ApiResponse<List<Patient>>.ErrorResponse("An error occurred while retrieving active patients"));
        }
    }

    [HttpGet("all-including-inactive")]
    [AllowAnonymous]
    // [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<Patient>>>> GetAllIncludingInactive()
    {
        try
        {
            var patients = await _patientRepository.GetAllIncludingInactiveAsync();
            return Ok(ApiResponse<List<Patient>>.SuccessResponse(patients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all patients");
            return StatusCode(500, ApiResponse<List<Patient>>.ErrorResponse("An error occurred while retrieving all patients"));
        }
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<List<Patient>>>> SearchPatients([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiResponse<List<Patient>>.ErrorResponse("Search term is required"));
            }

            var patients = await _patientRepository.SearchAsync(q);
            return Ok(ApiResponse<List<Patient>>.SuccessResponse(patients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients");
            return StatusCode(500, ApiResponse<List<Patient>>.ErrorResponse("An error occurred while searching patients"));
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<Patient>>> GetPatient(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            
            if (patient == null)
            {
                return NotFound(ApiResponse<Patient>.ErrorResponse("Patient not found"));
            }

            return Ok(ApiResponse<Patient>.SuccessResponse(patient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient {PatientId}", id);
            return StatusCode(500, ApiResponse<Patient>.ErrorResponse("An error occurred while retrieving the patient"));
        }
    }

    [HttpGet("{id}/with-sessions")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<PatientWithLatestSession>>> GetPatientWithSessions(int id)
    {
        try
        {
            var patient = await _patientRepository.GetPatientWithLatestSessionAsync(id);
            
            if (patient == null)
            {
                return NotFound(ApiResponse<PatientWithLatestSession>.ErrorResponse("Patient not found"));
            }

            return Ok(ApiResponse<PatientWithLatestSession>.SuccessResponse(patient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient with sessions {PatientId}", id);
            return StatusCode(500, ApiResponse<PatientWithLatestSession>.ErrorResponse("An error occurred while retrieving the patient"));
        }
    }

    [HttpPost]
    [AllowAnonymous]  // Temporarily allow anonymous access for testing
    // [Authorize(Roles = "Admin,Doctor,Nurse")]  // Uncomment this line to re-enable auth
    public async Task<ActionResult<ApiResponse<int>>> CreatePatient([FromBody] CreatePatientRequest request)
    {
        try
        {
            // Check if MRN already exists
            if (!string.IsNullOrEmpty(request.MRN))
            {
                var existingPatient = await _patientRepository.GetByMRNAsync(request.MRN);
                if (existingPatient != null)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse($"A patient with MRN '{request.MRN}' already exists. Please use a different MRN."));
                }
            }

            var patient = new Patient
            {
                MRN = request.MRN,
                Name = request.Name,
                Age = request.Age,
                Gender = request.Gender,
                ContactNumber = request.ContactNumber,
                EmergencyContact = request.EmergencyContact,
                Address = request.Address,
                GuardianName = request.GuardianName,
                HDCycle = request.HDCycle,
                HDFrequency = request.HDFrequency,
                DryWeight = request.DryWeight,
                HDStartDate = request.HDStartDate,
                DialyserType = request.DialyserType,
                DialyserModel = request.DialyserModel,
                PrescribedDuration = request.PrescribedDuration,
                PrescribedBFR = request.PrescribedBFR,
                DialysatePrescription = request.DialysatePrescription,
                DialyserCount = request.DialyserCount ?? 0,
                BloodTubingCount = request.BloodTubingCount ?? 0,
                TotalDialysisCompleted = request.TotalDialysisCompleted ?? 0,
                IsActive = true
            };

            var patientId = await _patientRepository.CreateAsync(patient);
            
            // If patient has HD cycle, automatically generate recurring sessions with slot/bed assignment
            if (!string.IsNullOrEmpty(request.HDCycle) && request.HDStartDate.HasValue)
            {
                try
                {
                    // Validate preferred slot
                    int preferredSlot = request.PreferredSlotID ?? 1; // Default to Morning if not specified
                    if (preferredSlot < 1 || preferredSlot > 4)
                    {
                        preferredSlot = 1; // Fallback to Morning
                    }
                    
                    _logger.LogInformation("🔄 Auto-generating recurring sessions for patient {PatientName} with HDCycle={HDCycle}, PreferredSlot={Slot}",
                        request.Name, request.HDCycle, preferredSlot);
                    
                    // Calculate session dates based on HD Cycle for next 4 weeks (28 days)
                    var sessionDates = CalculateSessionDates(request.HDCycle, request.HDStartDate.Value, 28);
                    var createdSessions = new List<int>();
                    
                    foreach (var sessionDate in sessionDates)
                    {
                        // Smart bed assignment for each date
                        var assignedBed = await _bedAssignmentService.GetNextAvailableBedAsync(preferredSlot, sessionDate);
                        
                        if (!assignedBed.HasValue)
                        {
                            _logger.LogWarning("⚠️ No available beds for {Date} in Slot {Slot}, skipping", sessionDate, preferredSlot);
                            continue;
                        }
                        
                        // Determine session status: "Active" if today, "Pre-Scheduled" if future
                        var sessionStatus = sessionDate.Date == DateTime.Today ? "Active" : "Pre-Scheduled";
                        
                        var schedule = new HDSchedule
                        {
                            PatientID = patientId,
                            SessionDate = sessionDate,
                            HDCycle = request.HDCycle,
                            DryWeight = request.DryWeight,
                            HDStartDate = request.HDStartDate,
                            DialyserType = request.DialyserType,
                            DialyserModel = request.DialyserModel,
                            PrescribedDuration = request.PrescribedDuration,
                            PrescribedBFR = request.PrescribedBFR,
                            DialysatePrescription = request.DialysatePrescription,
                            SlotID = preferredSlot,  // Assigned based on user preference
                            BedNumber = assignedBed.Value,  // Smart bed assignment with spacing
                            SessionStatus = sessionStatus,
                            IsAutoGenerated = true,
                            IsDischarged = false,
                            CreatedByStaffName = "System",
                            CreatedByStaffRole = "Auto-Scheduler",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        
                        var scheduleId = await _scheduleRepository.CreateAsync(schedule);
                        createdSessions.Add(scheduleId);
                    }
                    
                    _logger.LogInformation("✅ Auto-generated {Count} recurring sessions for patient {PatientName}",
                        createdSessions.Count, request.Name);
                    
                    return CreatedAtAction(nameof(GetPatient), new { id = patientId }, 
                        ApiResponse<int>.SuccessResponse(patientId, 
                            $"Patient created successfully with {createdSessions.Count} pre-scheduled sessions"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to auto-generate recurring sessions for patient {PatientName}", request.Name);
                    // Don't fail patient creation if recurring session generation fails
                    return CreatedAtAction(nameof(GetPatient), new { id = patientId }, 
                        ApiResponse<int>.SuccessResponse(patientId, 
                            "Patient created successfully, but recurring session generation failed. Please create sessions manually."));
                }
            }
            
            return CreatedAtAction(nameof(GetPatient), new { id = patientId }, 
                ApiResponse<int>.SuccessResponse(patientId, "Patient created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            
            // Check if it's a constraint error (duplicate MRN)
            if (ex.Message.Contains("UNIQUE constraint failed: Patients.MRN"))
            {
                return BadRequest(ApiResponse<int>.ErrorResponse($"A patient with MRN '{request.MRN}' already exists. Please use a different MRN."));
            }
            
            return StatusCode(500, ApiResponse<int>.ErrorResponse("An error occurred while creating the patient"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePatient(int id, [FromBody] CreatePatientRequest request)
    {
        try
        {
            var existingPatient = await _patientRepository.GetByIdAsync(id);
            if (existingPatient == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Patient not found"));
            }

            existingPatient.MRN = request.MRN;
            existingPatient.Name = request.Name;
            existingPatient.Age = request.Age;
            existingPatient.Gender = request.Gender;
            existingPatient.ContactNumber = request.ContactNumber;
            existingPatient.EmergencyContact = request.EmergencyContact;
            existingPatient.Address = request.Address;
            existingPatient.GuardianName = request.GuardianName;
            existingPatient.HDCycle = request.HDCycle;
            existingPatient.HDFrequency = request.HDFrequency;
            existingPatient.DryWeight = request.DryWeight;
            existingPatient.HDStartDate = request.HDStartDate;
            existingPatient.DialyserType = request.DialyserType;
            existingPatient.DialyserModel = request.DialyserModel;
            existingPatient.PrescribedDuration = request.PrescribedDuration;
            existingPatient.PrescribedBFR = request.PrescribedBFR;
            existingPatient.DialysatePrescription = request.DialysatePrescription;
            existingPatient.DialyserCount = request.DialyserCount ?? existingPatient.DialyserCount;
            existingPatient.BloodTubingCount = request.BloodTubingCount ?? existingPatient.BloodTubingCount;
            existingPatient.TotalDialysisCompleted = request.TotalDialysisCompleted ?? existingPatient.TotalDialysisCompleted;

            var result = await _patientRepository.UpdateAsync(existingPatient);
            
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Patient updated successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to update patient"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {PatientId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while updating the patient"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePatient(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Patient not found"));
            }

            var result = await _patientRepository.DeleteAsync(id);
            
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Patient deleted successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete patient"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient {PatientId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the patient"));
        }
    }

    [HttpPut("{id}/discharge")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<bool>>> DischargePatient(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Patient not found"));
            }

            // Mark patient as inactive/discharged
            patient.IsActive = false;
            var result = await _patientRepository.UpdateAsync(patient);
            
            if (result)
            {
                _logger.LogInformation("Patient {PatientId} ({PatientName}) manually discharged", patient.PatientID, patient.Name);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Patient discharged successfully"));
            }
            
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to discharge patient"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discharging patient {PatientId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while discharging the patient"));
        }
    }

    [HttpGet("{id}/equipment-status")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetEquipmentStatus(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Patient not found"));
            }

            // Equipment usage limits
            const int DIALYSER_MAX = 12; // Maximum 12 uses
            const int BLOOD_TUBING_MAX = 1; // Single use only

            // Calculate dialyser status
            var dialyserPercentage = (patient.DialyserCount * 100.0) / DIALYSER_MAX;
            var dialyserRemaining = DIALYSER_MAX - patient.DialyserCount;
            string dialyserStatus = "OK";
            string dialyserMessage = "";
            bool dialyserRequiresReplacement = false;

            if (patient.DialyserCount >= DIALYSER_MAX)
            {
                dialyserStatus = "Expired";
                dialyserMessage = "URGENT: Dialyser has reached maximum usage limit. Replacement required immediately.";
                dialyserRequiresReplacement = true;
            }
            else if (dialyserPercentage >= 90)
            {
                dialyserStatus = "Critical";
                dialyserMessage = $"WARNING: Dialyser usage at {dialyserPercentage:F1}%. Replace soon ({dialyserRemaining} use(s) remaining).";
                dialyserRequiresReplacement = true;
            }
            else if (dialyserPercentage >= 80)
            {
                dialyserStatus = "Warning";
                dialyserMessage = $"Dialyser usage at {dialyserPercentage:F1}%. Consider replacement soon ({dialyserRemaining} use(s) remaining).";
            }

            // Calculate blood tubing status
            var bloodTubingPercentage = (patient.BloodTubingCount * 100.0) / BLOOD_TUBING_MAX;
            var bloodTubingRemaining = BLOOD_TUBING_MAX - patient.BloodTubingCount;
            string bloodTubingStatus = "OK";
            string bloodTubingMessage = "";
            bool bloodTubingRequiresReplacement = false;

            if (patient.BloodTubingCount >= BLOOD_TUBING_MAX)
            {
                bloodTubingStatus = "Expired";
                bloodTubingMessage = "URGENT: Blood tubing has reached usage limit. Replacement required.";
                bloodTubingRequiresReplacement = true;
            }
            else if (bloodTubingPercentage >= 80)
            {
                bloodTubingStatus = "Warning";
                bloodTubingMessage = $"Blood tubing usage at {bloodTubingPercentage:F1}%. ({bloodTubingRemaining} use(s) remaining).";
            }

            var equipmentStatus = new
            {
                patientId = patient.PatientID,
                patientName = patient.Name,
                dialyser = new
                {
                    currentUsageCount = patient.DialyserCount,
                    maxUsageLimit = DIALYSER_MAX,
                    remainingUses = dialyserRemaining,
                    usagePercentage = Math.Round(dialyserPercentage, 1),
                    status = dialyserStatus,
                    message = dialyserMessage,
                    requiresReplacement = dialyserRequiresReplacement
                },
                bloodTubing = new
                {
                    currentUsageCount = patient.BloodTubingCount,
                    maxUsageLimit = BLOOD_TUBING_MAX,
                    remainingUses = bloodTubingRemaining,
                    usagePercentage = Math.Round(bloodTubingPercentage, 1),
                    status = bloodTubingStatus,
                    message = bloodTubingMessage,
                    requiresReplacement = bloodTubingRequiresReplacement
                },
                totalDialysisCompleted = patient.TotalDialysisCompleted
            };

            return Ok(ApiResponse<object>.SuccessResponse(equipmentStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment status for patient {PatientId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving equipment status"));
        }
    }
    
    /// <summary>
    /// Calculate session dates based on HD Cycle pattern
    /// </summary>
    private List<DateTime> CalculateSessionDates(string hdCycle, DateTime startDate, int daysAhead)
    {
        var dates = new List<DateTime>();
        var daysOfWeek = ParseHDCycleToDays(hdCycle);
        
        if (!daysOfWeek.Any())
            return dates;
        
        var currentDate = startDate;
        var endDate = startDate.AddDays(daysAhead);
        
        while (currentDate <= endDate)
        {
            if (daysOfWeek.Contains(currentDate.DayOfWeek))
            {
                dates.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }
        
        return dates;
    }
    
    /// <summary>
    /// Parse HD Cycle string to days of week
    /// </summary>
    private List<DayOfWeek> ParseHDCycleToDays(string hdCycle)
    {
        var days = new List<DayOfWeek>();
        
        if (string.IsNullOrEmpty(hdCycle))
            return days;
        
        hdCycle = hdCycle.ToUpper();
        
        if (hdCycle.Contains("EVERY DAY") || hdCycle.Contains("DAILY"))
        {
            days.AddRange(new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, 
                                  DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday });
        }
        else if (hdCycle.Contains("MWF") || (hdCycle.Contains("MONDAY") && hdCycle.Contains("WEDNESDAY") && hdCycle.Contains("FRIDAY")))
        {
            days.AddRange(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
        }
        else if (hdCycle.Contains("TTS") || (hdCycle.Contains("TUESDAY") && hdCycle.Contains("THURSDAY") && hdCycle.Contains("SATURDAY")))
        {
            days.AddRange(new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday });
        }
        else if (hdCycle.Contains("EVERY 2 DAYS"))
        {
            days.AddRange(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
        }
        else if (hdCycle.Contains("EVERY 3 DAYS"))
        {
            days.AddRange(new[] { DayOfWeek.Monday, DayOfWeek.Thursday });
        }
        else if (hdCycle.Contains("EVERY 4 DAYS") || hdCycle.Contains("EVERY 5 DAYS") || hdCycle.Contains("EVERY WEEK"))
        {
            days.AddRange(new[] { DayOfWeek.Monday });
        }
        
        return days;
    }
}
