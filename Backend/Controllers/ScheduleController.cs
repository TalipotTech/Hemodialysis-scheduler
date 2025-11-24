using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IHDScheduleRepository _scheduleRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(
        IHDScheduleRepository scheduleRepository,
        IPatientRepository patientRepository,
        ILogger<ScheduleController> logger)
    {
        _scheduleRepository = scheduleRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    [HttpGet("daily")]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")] // All medical staff can view daily schedule
    public async Task<ActionResult<ApiResponse<object>>> GetDailySchedule([FromQuery] string? date = null)
    {
        try
        {
            // Parse the date if provided, otherwise use today's date
            DateTime targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            // Get all schedules for the target date (excluding discharged and history)
            // This includes both Active and Pre-Scheduled sessions
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var daySchedules = allSchedules.Where(s => 
                s.SessionDate.Date == targetDate && 
                !s.IsDischarged && 
                !s.IsMovedToHistory).ToList();

            // Define slots
            var slots = new[]
            {
                new { slotID = 1, slotName = "Morning Shift", timeRange = "06:00 - 10:00" },
                new { slotID = 2, slotName = "Afternoon Shift", timeRange = "11:00 - 15:00" },
                new { slotID = 3, slotName = "Evening Shift", timeRange = "16:00 - 20:00" },
                new { slotID = 4, slotName = "Night Shift", timeRange = "21:00 - 01:00" }
            };

            // Build slot schedules
            var slotSchedules = new List<object>();
            foreach (var slot in slots)
            {
                var slotSchedulesForSlot = daySchedules.Where(s => s.SlotID == slot.slotID).ToList();
                
                // Create bed status array for 10 beds
                var beds = new List<object>();
                for (int bedNum = 1; bedNum <= 10; bedNum++)
                {
                    var schedule = slotSchedulesForSlot.FirstOrDefault(s => s.BedNumber == bedNum);
                    
                    if (schedule != null)
                    {
                        // Fetch patient data to get actual age
                        var patient = await _patientRepository.GetByIdAsync(schedule.PatientID);
                        
                        beds.Add(new
                        {
                            bedNumber = bedNum,
                            status = "occupied",
                            scheduleId = schedule.ScheduleID,
                            patient = new
                            {
                                id = schedule.PatientID,
                                name = schedule.PatientName ?? patient?.Name ?? "Unknown",
                                age = patient?.Age ?? 0,
                                bloodPressure = schedule.BloodPressure
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

                slotSchedules.Add(new
                {
                    slotID = slot.slotID,
                    slotName = slot.slotName,
                    timeRange = slot.timeRange,
                    beds = beds
                });
            }

            var dailySchedule = new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                slots = slotSchedules
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

    private int CalculateAge(DateTime date)
    {
        // This is a placeholder - you should get actual patient birth date
        return 0;
    }
}
