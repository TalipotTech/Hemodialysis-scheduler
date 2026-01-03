using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;
using HDScheduler.API.Services;
using HDScheduler.API.Data;
using HDScheduler.API.Models;
using Dapper;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IHDCycleService _hdCycleService;
    private readonly ILogger<ScheduleController> _logger;
    private readonly DapperContext _context;

    public ScheduleController(
        IHDScheduleRepository scheduleRepository,
        IPatientRepository patientRepository,
        IHDCycleService hdCycleService,
        ILogger<ScheduleController> logger,
        DapperContext context)
    {
        _scheduleRepository = scheduleRepository;
        _patientRepository = patientRepository;
        _hdCycleService = hdCycleService;
        _logger = logger;
        _context = context;
    }

    [HttpGet("daily")]
    [AllowAnonymous] // Temporarily disabled auth for debugging
    // [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All medical staff can view daily schedule
    public async Task<ActionResult<ApiResponse<object>>> GetDailySchedule([FromQuery] string? date = null)
    {
        try
        {
            // Parse the date if provided, otherwise use today's date
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            _logger.LogInformation($"GetDailySchedule called for date: {targetDate:yyyy-MM-dd}");

            // Get all schedules for the target date
            // For past dates, include discharged sessions (they were active on that day)
            // Only exclude IsMovedToHistory (these are archived/deleted)
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var daySchedules = allSchedules.Where(s => 
                s.SessionDate.Date == targetDate && 
                !s.IsMovedToHistory).ToList();
            
            // Get start and end of week for session counting
            var startOfWeek = targetDate.AddDays(-(int)targetDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            _logger.LogInformation($"Found {daySchedules.Count} schedules for {targetDate:yyyy-MM-dd}");
            foreach (var schedule in daySchedules)
            {
                _logger.LogInformation($"  - ScheduleID: {schedule.ScheduleID}, Patient: {schedule.PatientName}, Slot: {schedule.SlotID}, Bed: {schedule.BedNumber}, Status: {schedule.SessionStatus}");
            }

            // Get slots from database with MaxBeds
            using var connection = _context.CreateConnection();
            
            // HYBRID MODE: Show both real sessions and auto-suggested patients
            // Real sessions = confirmed, in database (shown with ✅ badge)
            // Suggested sessions = based on HD Cycle, need confirmation (shown with 📋 badge)
            
            // STEP 1: Find patients who should be scheduled today based on HD Cycle
            var patientsQuery = @"
                SELECT PatientID, Name, MRN, Age, Gender, HDStartDate, HDCycle, HDFrequency, 
                       DryWeight, DialyserType, PreferredSlotID
                FROM Patients 
                WHERE IsActive = 1 
                    AND HDStartDate IS NOT NULL 
                    AND HDCycle IS NOT NULL
                    AND CAST(HDStartDate AS DATE) <= CAST(@TargetDate AS DATE)";
            
            var activePatients = await connection.QueryAsync<dynamic>(patientsQuery, new { TargetDate = targetDate });
            
            _logger.LogInformation($"HYBRID MODE: Found {daySchedules.Count} real sessions + checking {activePatients.Count()} active patients for auto-suggestions");
            
            // Check which patients should be scheduled today based on their HD Cycle
            foreach (var patient in activePatients)
            {
                // Skip if HDStartDate is null or empty
                if (patient.HDStartDate == null || string.IsNullOrWhiteSpace(patient.HDStartDate.ToString()))
                {
                    _logger.LogWarning($"Patient {patient.Name} (ID: {patient.PatientID}) has NULL HDStartDate - skipping");
                    continue;
                }
                
                DateTime patientHDStartDate;
                if (!DateTime.TryParse(patient.HDStartDate.ToString(), out patientHDStartDate))
                {
                    _logger.LogWarning($"Patient {patient.Name} (ID: {patient.PatientID}) has invalid HDStartDate format: {patient.HDStartDate} - skipping");
                    continue;
                }
                
                _logger.LogInformation($"Checking patient {patient.Name} (ID: {patient.PatientID}): HDStartDate={patientHDStartDate:yyyy-MM-dd}, HDCycle={patient.HDCycle}");
                
                bool shouldBeScheduledToday = ShouldPatientBeScheduledOnDate(
                    patientHDStartDate, 
                    patient.HDCycle, 
                    targetDate);
                
                _logger.LogInformation($"  -> Should be scheduled on {targetDate:yyyy-MM-dd}? {shouldBeScheduledToday}");
                
                if (shouldBeScheduledToday)
                {
                    // Check if this patient already has a confirmed session for today
                    bool hasSessionToday = daySchedules.Any(s => s.PatientID == (int)patient.PatientID);
                    
                    if (!hasSessionToday)
                    {
                        // Add an AUTO-SUGGESTED session (virtual, needs confirmation)
                        _logger.LogInformation($"📋 AUTO-SUGGESTING patient {patient.Name} (ID: {patient.PatientID}) based on HD Cycle: {patient.HDCycle}");
                        
                        // Use preferred slot if available, otherwise default to Morning
                        int suggestedSlot = 1; // Default Morning
                        if (patient.PreferredSlotID != null)
                        {
                            suggestedSlot = (int)patient.PreferredSlotID;
                            _logger.LogInformation($"   Using preferred slot {suggestedSlot} for {patient.Name}");
                        }
                        
                        var suggestedSession = new HDSchedule
                        {
                            ScheduleID = 0, // Virtual session (not yet confirmed/saved)
                            PatientID = (int)patient.PatientID,
                            PatientName = patient.Name?.ToString(),
                            SessionDate = targetDate,
                            SlotID = suggestedSlot, // Use preferred slot or default
                            BedNumber = null, // Not assigned yet - staff will assign when confirming
                            DryWeight = patient.DryWeight != null ? (decimal?)Convert.ToDecimal(patient.DryWeight) : null,
                            HDStartDate = patientHDStartDate,
                            HDCycle = patient.HDCycle?.ToString(),
                            HDFrequency = patient.HDFrequency != null ? (int?)Convert.ToInt32(patient.HDFrequency) : null,
                            DialyserType = patient.DialyserType?.ToString(),
                            SessionStatus = "Suggested", // Changed from "Auto-Scheduled" to "Suggested"
                            IsAutoGenerated = true, // Mark as auto-suggested (not confirmed)
                            IsDischarged = false,
                            IsMovedToHistory = false
                        };
                        
                        daySchedules.Add(suggestedSession);
                    }
                    else
                    {
                        _logger.LogInformation($"✅ Patient {patient.Name} (ID: {patient.PatientID}) already has confirmed session today");
                    }
                }
            }
            
            _logger.LogInformation($"📊 SUMMARY: {daySchedules.Count(s => !s.IsAutoGenerated)} confirmed + {daySchedules.Count(s => s.IsAutoGenerated)} suggested = {daySchedules.Count} total");
            var slotsQuery = @"
                SELECT SlotID, SlotName, 
                       CASE SlotID
                           WHEN 1 THEN '06:00 - 10:00'
                           WHEN 2 THEN '11:00 - 15:00'
                           WHEN 3 THEN '16:00 - 20:00'
                           WHEN 4 THEN '21:00 - 01:00'
                           ELSE 'Unknown'
                       END as TimeRange,
                       MaxBeds
                FROM Slots 
                WHERE IsActive = 1
                ORDER BY SlotID";
            var slots = await connection.QueryAsync<SlotInfo>(slotsQuery);

            // Build slot schedules
            var slotSchedules = new List<object>();
            foreach (var slot in slots)
            {
                var slotSchedulesForSlot = daySchedules.Where(s => s.SlotID == slot.SlotID).ToList();
                
                // Create bed status array dynamically based on MaxBeds
                var beds = new List<object>();
                
                // Track which schedules have been assigned to beds
                var assignedScheduleIds = new HashSet<int>();
                
                for (int bedNum = 1; bedNum <= slot.MaxBeds; bedNum++)
                {
                    var schedule = slotSchedulesForSlot.FirstOrDefault(s => s.BedNumber == bedNum);
                    
                    if (schedule != null)
                    {
                        assignedScheduleIds.Add(schedule.ScheduleID);
                        
                        // Fetch patient data to get actual age
                        var patient = await _patientRepository.GetByIdAsync(schedule.PatientID);
                        
                        // Determine bed status based on session status, discharge status, and date
                        string bedStatus;
                        
                        // Check if this is a past date (should show as completed/blue)
                        if (schedule.SessionDate.Date < DateTime.Today)
                        {
                            bedStatus = "completed"; // Past session (historical view - show in blue)
                        }
                        else if (schedule.IsDischarged)
                        {
                            bedStatus = "completed"; // Completed/discharged session (for today)
                        }
                        else if (schedule.SessionDate.Date > DateTime.Today)
                        {
                            bedStatus = "pre-scheduled"; // Future scheduled session (not today)
                        }
                        else if (schedule.SessionStatus == "Pre-Scheduled")
                        {
                            bedStatus = "pre-scheduled"; // Today's session but not yet activated
                        }
                        else
                        {
                            bedStatus = "occupied"; // Today's active/in-treatment session (RED)
                        }
                        
                        // Calculate weekly session info
                        var patientWeekSessions = allSchedules
                            .Where(s => s.PatientID == schedule.PatientID && 
                                       s.SessionDate >= startOfWeek && 
                                       s.SessionDate < endOfWeek &&
                                       !s.IsDischarged)
                            .OrderBy(s => s.SessionDate)
                            .ToList();
                        
                        var sessionNumber = patientWeekSessions.FindIndex(s => s.ScheduleID == schedule.ScheduleID) + 1;
                        var totalSessions = patientWeekSessions.Count;
                        
                        beds.Add(new
                        {
                            bedNumber = bedNum,
                            status = bedStatus,
                            scheduleId = schedule.ScheduleID,
                            sessionStatus = schedule.SessionStatus ?? "Active",
                            sessionDate = schedule.SessionDate.ToString("yyyy-MM-dd"),
                            sessionNumber = sessionNumber,
                            totalWeeklySessions = totalSessions,
                            patient = new
                            {
                                id = schedule.PatientID,
                                patientId = schedule.PatientID,
                                name = schedule.PatientName ?? patient?.Name ?? "Unknown",
                                age = patient?.Age ?? 0,
                                bloodPressure = schedule.BloodPressure,
                                hdCycle = patient?.HDCycle,
                                isDischarged = schedule.IsDischarged
                            }
                        });
                    }
                    else
                    {
                        beds.Add(new
                        {
                            bedNumber = bedNum,
                            status = "available",
                            scheduleId = (int?)null,
                            patient = (object?)null
                        });
                    }
                }
                
                // Handle schedules without bed assignments (BedNumber is null or 0)
                // Assign them to available beds
                var unassignedSchedules = slotSchedulesForSlot.Where(s => 
                    (s.BedNumber == null || s.BedNumber == 0) && 
                    !assignedScheduleIds.Contains(s.ScheduleID)).ToList();
                
                foreach (var schedule in unassignedSchedules)
                {
                    // Find first available bed
                    for (int bedNum = 1; bedNum <= slot.MaxBeds; bedNum++)
                    {
                        var bedIndex = bedNum - 1;
                        if (bedIndex < beds.Count)
                        {
                            var bedObj = beds[bedIndex] as dynamic;
                            if (bedObj != null && bedObj.status == "available")
                            {
                                // Fetch patient data
                                var patient = await _patientRepository.GetByIdAsync(schedule.PatientID);
                                
                                // Calculate weekly session info
                                var patientWeekSessions = allSchedules
                                    .Where(s => s.PatientID == schedule.PatientID && 
                                               s.SessionDate >= startOfWeek && 
                                               s.SessionDate < endOfWeek &&
                                               !s.IsDischarged)
                                    .OrderBy(s => s.SessionDate)
                                    .ToList();
                                
                                var sessionNumber = patientWeekSessions.FindIndex(s => s.ScheduleID == schedule.ScheduleID) + 1;
                                var totalSessions = patientWeekSessions.Count;
                                
                                // Determine bed status based on discharge status
                                string bedStatus = "pre-scheduled";
                                if (schedule.IsDischarged)
                                {
                                    bedStatus = "completed";
                                }
                                
                                // Update this bed with the pre-scheduled patient
                                beds[bedIndex] = new
                                {
                                    bedNumber = bedNum,
                                    status = bedStatus,
                                    scheduleId = schedule.ScheduleID,
                                    sessionStatus = schedule.SessionStatus ?? "Pre-Scheduled",
                                    sessionDate = schedule.SessionDate.ToString("yyyy-MM-dd"),
                                    sessionNumber = sessionNumber,
                                    totalWeeklySessions = totalSessions,
                                    needsBedAssignment = true,
                                    patient = new
                                    {
                                        id = schedule.PatientID,
                                        patientId = schedule.PatientID,
                                        name = schedule.PatientName ?? patient?.Name ?? "Unknown",
                                        age = patient?.Age ?? 0,
                                        bloodPressure = schedule.BloodPressure,
                                        hdCycle = patient?.HDCycle,
                                        isDischarged = schedule.IsDischarged
                                    }
                                };
                                break; // Move to next schedule
                            }
                        }
                    }
                }

                slotSchedules.Add(new
                {
                    slotID = slot.SlotID,
                    slotName = slot.SlotName,
                    timeRange = slot.TimeRange,
                    maxBeds = slot.MaxBeds,
                    beds = beds
                });
            }

            // Get pre-scheduled sessions without slot/bed assignments
            var unassignedPreScheduled = daySchedules.Where(s => 
                (s.SlotID == null || s.SlotID == 0) && 
                s.SessionStatus == "Pre-Scheduled").ToList();
            
            var unassignedSessions = new List<object>();
            foreach (var schedule in unassignedPreScheduled)
            {
                var patient = await _patientRepository.GetByIdAsync(schedule.PatientID);
                unassignedSessions.Add(new
                {
                    scheduleId = schedule.ScheduleID,
                    sessionDate = schedule.SessionDate.ToString("yyyy-MM-dd"),
                    sessionStatus = schedule.SessionStatus,
                    hdCycle = schedule.HDCycle,
                    patient = new
                    {
                        id = schedule.PatientID,
                        name = patient?.Name ?? "Unknown",
                        mrn = patient?.MRN,
                        age = patient?.Age ?? 0,
                        hdFrequency = patient?.HDFrequency ?? 0
                    }
                });
            }
            
            _logger.LogInformation($"Found {unassignedSessions.Count} unassigned pre-scheduled sessions for {targetDate:yyyy-MM-dd}");

            // Calculate reservation statistics
            var allSchedulesToday = daySchedules;
            var allSchedulesFuture = allSchedules.Where(s => 
                s.SessionDate.Date > targetDate && 
                !s.IsDischarged && 
                !s.IsMovedToHistory).ToList();

            var activePatientIds = allSchedulesToday.Select(s => s.PatientID).Distinct().ToList();
            var reservedPatientIds = allSchedulesFuture.Select(s => s.PatientID).Distinct().ToList();

            // Count by status
            var activeCount = allSchedulesToday.Count(s => s.SessionStatus != "Pre-Scheduled");
            var preScheduledCount = allSchedulesToday.Count(s => s.SessionStatus == "Pre-Scheduled");

            var dailySchedule = new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                slots = slotSchedules,
                unassignedPreScheduled = unassignedSessions,
                statistics = new
                {
                    totalActivePatients = activePatientIds.Count,
                    totalReservedPatients = reservedPatientIds.Count,
                    activeSessionsToday = activeCount,
                    preScheduledSessionsToday = preScheduledCount,
                    unassignedPreScheduledCount = unassignedSessions.Count,
                    totalSessionsToday = allSchedulesToday.Count,
                    futureSessionsCount = allSchedulesFuture.Count
                }
            };

            return Ok(ApiResponse<object>.SuccessResponse(dailySchedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily schedule");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the daily schedule"));
        }
    }

    [HttpGet("availability")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All medical staff can check bed availability
    public async Task<ActionResult<ApiResponse<object>>> GetBedAvailability([FromQuery] string? date = null)
    {
        try
        {
            // Parse the date if provided, otherwise use today's date
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            // Get all schedules for the target date (excluding discharged)
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var daySchedules = allSchedules.Where(s => s.SessionDate.Date == targetDate && !s.IsDischarged).ToList();

            // Define slots
            var slots = new[]
            {
                new { slotID = 1, slotName = "Morning Shift", timeRange = "06:00 AM - 10:00 AM" },
                new { slotID = 2, slotName = "Afternoon Shift", timeRange = "11:00 AM - 03:00 PM" },
                new { slotID = 3, slotName = "Evening Shift", timeRange = "04:00 PM - 08:00 PM" },
                new { slotID = 4, slotName = "Night Shift", timeRange = "09:00 PM - 01:00 AM" }
            };

            var availabilityList = new List<object>();
            foreach (var slot in slots)
            {
                var slotSchedules = daySchedules.Where(s => s.SlotID == slot.slotID).ToList();
                var occupiedBeds = slotSchedules.Select(s => s.BedNumber).ToList();
                var availableBeds = new List<int>();
                
                for (int bedNum = 1; bedNum <= 10; bedNum++)
                {
                    if (!occupiedBeds.Contains(bedNum))
                    {
                        availableBeds.Add(bedNum);
                    }
                }

                availabilityList.Add(new
                {
                    slotId = slot.slotID,
                    slotName = slot.slotName,
                    timeRange = slot.timeRange,
                    totalBeds = 10,
                    occupiedBeds = occupiedBeds.Count,
                    availableBeds = availableBeds
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(availabilityList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bed availability");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving bed availability"));
        }
    }

    [HttpPost("move-to-history")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> MoveCompletedSessionsToHistory()
    {
        try
        {
            var moved = await _scheduleRepository.MoveCompletedSessionsToHistoryAsync();
            
            if (moved)
            {
                return Ok(ApiResponse<object>.SuccessResponse(new { message = "Completed sessions moved to history successfully" }));
            }
            else
            {
                return Ok(ApiResponse<object>.SuccessResponse(new { message = "No sessions to move at this time" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving completed sessions to history");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while moving sessions to history"));
        }
    }

    [HttpPost("force-discharge/{scheduleId}")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> ForceDischargeSession(int scheduleId)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Schedule not found"));
            }

            // Mark as moved to history (which will show as discharged in UI)
            await _scheduleRepository.UpdateAsync(new HDScheduler.API.Models.HDSchedule 
            { 
                ScheduleID = schedule.ScheduleID,
                PatientID = schedule.PatientID,
                SessionDate = schedule.SessionDate,
                SlotID = schedule.SlotID,
                BedNumber = schedule.BedNumber,
                IsMovedToHistory = true,
                IsDischarged = false,
                UpdatedAt = DateTime.Now
            });

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Session marked as complete successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing discharge");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("auto-discharge-info")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse")]
    public async Task<ActionResult<ApiResponse<object>>> GetAutoDischargeInfo()
    {
        try
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            
            // Get sessions that are running and will be auto-discharged
            var activeSessions = schedules.Where(s => 
                s.TreatmentStartTime.HasValue && 
                !s.IsDischarged && 
                !s.IsMovedToHistory).ToList();

            var autoDischargeInfo = activeSessions.Select(s => {
                var startTime = s.TreatmentStartTime ?? DateTime.Now;
                var autoDischargeTime = startTime.AddHours(5);
                var hoursRunning = (DateTime.Now - startTime).TotalHours;
                var hoursUntilAutoDischarge = (autoDischargeTime - DateTime.Now).TotalHours;
                
                return new {
                    scheduleId = s.ScheduleID,
                    patientId = s.PatientID,
                    patientName = s.PatientName,
                    treatmentStartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    autoDischargeTime = autoDischargeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    hoursRunning = Math.Round(hoursRunning, 2),
                    hoursUntilAutoDischarge = Math.Round(hoursUntilAutoDischarge, 2),
                    willAutoDischarge = hoursUntilAutoDischarge > 0,
                    bedNumber = s.BedNumber,
                    slotId = s.SlotID,
                    sessionStatus = s.SessionStatus
                };
            }).OrderBy(x => x.hoursUntilAutoDischarge).ToList();

            return Ok(ApiResponse<object>.SuccessResponse(new { 
                autoDischargeEnabled = true,
                autoDischargeAfterHours = 5,
                checkIntervalMinutes = 5,
                activeSessions = autoDischargeInfo.Count,
                sessions = autoDischargeInfo
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auto-discharge info");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("patient-statistics")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
    public async Task<ActionResult<ApiResponse<object>>> GetPatientStatistics([FromQuery] string? date = null)
    {
        try
        {
            DateTime referenceDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var allSchedules = await _scheduleRepository.GetAllAsync();

            // Calculate date ranges
            var startOfDay = referenceDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var startOfWeek = referenceDate.AddDays(-(int)referenceDate.DayOfWeek); // Sunday
            var endOfWeek = startOfWeek.AddDays(7);

            var startOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var startOfYear = new DateTime(referenceDate.Year, 1, 1);
            var endOfYear = startOfYear.AddYears(1);

            // Day Statistics - Only count patients who actually did/are doing dialysis
            var daySchedules = allSchedules.Where(s => 
                s.SessionDate >= startOfDay && s.SessionDate < endOfDay).ToList();
            
            // Only count Active + Completed (actual dialysis cases, exclude pre-scheduled)
            var dayActualCases = daySchedules.Where(s => 
                s.SessionStatus == "Active" || 
                s.SessionStatus == "Completed").ToList();
            
            var dayActive = dayActualCases.Count(s => s.SessionStatus == "Active");
            var dayCompleted = dayActualCases.Count(s => s.SessionStatus == "Completed");
            var dayDischarged = daySchedules.Count(s => s.IsMovedToHistory == true); // Final discharge only
            var dayTotal = dayActive + dayCompleted; // Only actual dialysis cases
            var dayUniquePatients = dayActualCases.Select(s => s.PatientID).Distinct().Count();

            // Week Statistics - Only count patients who actually did/are doing dialysis
            var weekSchedules = allSchedules.Where(s => 
                s.SessionDate >= startOfWeek && s.SessionDate < endOfWeek).ToList();
            
            // Only count Active + Completed (actual dialysis cases, exclude pre-scheduled)
            var weekActualCases = weekSchedules.Where(s => 
                s.SessionStatus == "Active" || 
                s.SessionStatus == "Completed").ToList();
            
            var weekActive = weekActualCases.Count(s => s.SessionStatus == "Active");
            var weekCompleted = weekActualCases.Count(s => s.SessionStatus == "Completed");
            var weekDischarged = weekSchedules.Count(s => s.IsMovedToHistory == true); // Final discharge only
            var weekTotal = weekActive + weekCompleted; // Only actual dialysis cases
            var weekUniquePatients = weekActualCases.Select(s => s.PatientID).Distinct().Count();

            // Month Statistics - Only count patients who actually did/are doing dialysis
            var monthSchedules = allSchedules.Where(s => 
                s.SessionDate >= startOfMonth && s.SessionDate < endOfMonth).ToList();
            
            // Only count Active + Completed (actual dialysis cases, exclude pre-scheduled)
            var monthActualCases = monthSchedules.Where(s => 
                s.SessionStatus == "Active" || 
                s.SessionStatus == "Completed").ToList();
            
            var monthActive = monthActualCases.Count(s => s.SessionStatus == "Active");
            var monthCompleted = monthActualCases.Count(s => s.SessionStatus == "Completed");
            var monthDischarged = monthSchedules.Count(s => s.IsMovedToHistory == true); // Final discharge only
            var monthTotal = monthActive + monthCompleted; // Only actual dialysis cases
            var monthUniquePatients = monthActualCases.Select(s => s.PatientID).Distinct().Count();

            // Year Statistics - Only count patients who actually did/are doing dialysis
            var yearSchedules = allSchedules.Where(s => 
                s.SessionDate >= startOfYear && s.SessionDate < endOfYear).ToList();
            
            // Only count Active + Completed (actual dialysis cases, exclude pre-scheduled)
            var yearActualCases = yearSchedules.Where(s => 
                s.SessionStatus == "Active" || 
                s.SessionStatus == "Completed").ToList();
            
            var yearActive = yearActualCases.Count(s => s.SessionStatus == "Active");
            var yearCompleted = yearActualCases.Count(s => s.SessionStatus == "Completed");
            var yearDischarged = yearSchedules.Count(s => s.IsMovedToHistory == true); // Final discharge only
            var yearTotal = yearActive + yearCompleted; // Only actual dialysis cases
            var yearUniquePatients = yearActualCases.Select(s => s.PatientID).Distinct().Count();

            var statistics = new
            {
                referenceDate = referenceDate.ToString("yyyy-MM-dd"),
                generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                
                day = new
                {
                    date = referenceDate.ToString("yyyy-MM-dd"),
                    dayOfWeek = referenceDate.DayOfWeek.ToString(),
                    active = dayActive,
                    completed = dayCompleted,
                    discharged = dayDischarged,
                    total = dayTotal,
                    uniquePatients = dayUniquePatients,
                    dischargeRate = dayTotal > 0 ? Math.Round((double)dayDischarged / dayTotal * 100, 2) : 0
                },
                
                week = new
                {
                    startDate = startOfWeek.ToString("yyyy-MM-dd"),
                    endDate = endOfWeek.AddDays(-1).ToString("yyyy-MM-dd"),
                    weekNumber = System.Globalization.ISOWeek.GetWeekOfYear(referenceDate),
                    active = weekActive,
                    completed = weekCompleted,
                    discharged = weekDischarged,
                    total = weekTotal,
                    uniquePatients = weekUniquePatients,
                    dischargeRate = weekTotal > 0 ? Math.Round((double)weekDischarged / weekTotal * 100, 2) : 0,
                    averageSessionsPerDay = Math.Round((double)weekTotal / 7, 2)
                },
                
                month = new
                {
                    month = referenceDate.ToString("MMMM yyyy"),
                    startDate = startOfMonth.ToString("yyyy-MM-dd"),
                    endDate = endOfMonth.AddDays(-1).ToString("yyyy-MM-dd"),
                    active = monthActive,
                    completed = monthCompleted,
                    discharged = monthDischarged,
                    total = monthTotal,
                    uniquePatients = monthUniquePatients,
                    dischargeRate = monthTotal > 0 ? Math.Round((double)monthDischarged / monthTotal * 100, 2) : 0,
                    averageSessionsPerDay = Math.Round((double)monthTotal / DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month), 2)
                },
                
                year = new
                {
                    year = referenceDate.Year,
                    startDate = startOfYear.ToString("yyyy-MM-dd"),
                    endDate = endOfYear.AddDays(-1).ToString("yyyy-MM-dd"),
                    active = yearActive,
                    completed = yearCompleted,
                    discharged = yearDischarged,
                    total = yearTotal,
                    uniquePatients = yearUniquePatients,
                    dischargeRate = yearTotal > 0 ? Math.Round((double)yearDischarged / yearTotal * 100, 2) : 0,
                    averageSessionsPerMonth = Math.Round((double)yearTotal / 12, 2),
                    averageSessionsPerDay = Math.Round((double)yearTotal / 365, 2)
                }
            };

            return Ok(ApiResponse<object>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient statistics");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    private int CalculateAge(DateTime date)
    {
        // This is a placeholder - you should get actual patient birth date
        return 0;
    }
    
    /// <summary>
    /// Check if a patient should be scheduled on a specific date based on their HD Cycle
    /// </summary>
    private bool ShouldPatientBeScheduledOnDate(DateTime hdStartDate, string hdCycle, DateTime targetDate)
    {
        // Patient can't be scheduled before their HD start date
        if (targetDate < hdStartDate.Date)
            return false;
        
        // Parse HD Cycle
        var dayOfWeek = targetDate.DayOfWeek;
        
        switch (hdCycle?.ToLower())
        {
            case "everyday":
            case "every day":
            case "daily":
            case "everyday (daily)":
            case "every day (daily)":
                return true; // Every day
            
            case "mwf":
            case "mon-wed-fri":
                return dayOfWeek == DayOfWeek.Monday || 
                       dayOfWeek == DayOfWeek.Wednesday || 
                       dayOfWeek == DayOfWeek.Friday;
            
            case "tts":
            case "tue-thu-sat":
                return dayOfWeek == DayOfWeek.Tuesday || 
                       dayOfWeek == DayOfWeek.Thursday || 
                       dayOfWeek == DayOfWeek.Saturday;
            
            case "alternate":
            case "alternate days":
                // Calculate days since HD start date
                var daysSinceStart = (targetDate.Date - hdStartDate.Date).Days;
                return daysSinceStart % 2 == 0; // Every other day
            
            case "3x/week":
            case "3 times per week":
                // Default to MWF pattern
                return dayOfWeek == DayOfWeek.Monday || 
                       dayOfWeek == DayOfWeek.Wednesday || 
                       dayOfWeek == DayOfWeek.Friday;
            
            case "2x/week":
            case "2 times per week":
                // Default to Monday and Thursday
                return dayOfWeek == DayOfWeek.Monday || 
                       dayOfWeek == DayOfWeek.Thursday;
            
            case "weekly":
            case "1x/week":
                // Same day of week as HD start date
                return dayOfWeek == hdStartDate.DayOfWeek;
            
            default:
                // Custom or unknown pattern - don't auto-schedule
                return false;
        }
    }
}

public class SlotInfo
{
    public int SlotID { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public int MaxBeds { get; set; }
}
