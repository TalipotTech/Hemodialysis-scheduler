using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientActivityController : ControllerBase
{
    private readonly IPatientActivityRepository _activityRepository;
    private readonly ILogger<PatientActivityController> _logger;

    public PatientActivityController(
        IPatientActivityRepository activityRepository,
        ILogger<PatientActivityController> logger)
    {
        _activityRepository = activityRepository;
        _logger = logger;
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordActivity([FromBody] PatientActivityLog activity)
    {
        try
        {
            if (activity.PatientID <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid PatientID" });
            }

            activity.CreatedAt = DateTime.Now;
            
            var activityId = await _activityRepository.CreateActivityAsync(activity);
            
            _logger.LogInformation($"Patient activity recorded: {activity.ActivityType} for PatientID {activity.PatientID}");
            
            return Ok(new 
            { 
                success = true, 
                message = $"Activity recorded successfully", 
                data = new { activityId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording patient activity");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{patientId}/activities")]
    public async Task<IActionResult> GetPatientActivities(int patientId)
    {
        try
        {
            var activities = await _activityRepository.GetPatientActivitiesAsync(patientId);
            
            return Ok(new 
            { 
                success = true, 
                data = activities,
                message = $"Found {activities.Count} activities"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving activities for patient {patientId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{patientId}/timeline")]
    public async Task<IActionResult> GetPatientHistoryTimeline(int patientId)
    {
        try
        {
            var timeline = await _activityRepository.GetPatientHistoryTimelineAsync(patientId);
            
            return Ok(new 
            { 
                success = true, 
                data = timeline,
                message = $"Found {timeline.Count} timeline events"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving timeline for patient {patientId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("late")]
    public async Task<IActionResult> RecordLateArrival([FromBody] LateArrivalRequest request)
    {
        try
        {
            var activity = new PatientActivityLog
            {
                PatientID = request.PatientID,
                ScheduleID = request.ScheduleID,
                ActivityDate = request.ActivityDate ?? DateTime.Now,
                ActivityType = "LATE",
                Reason = request.Reason ?? "Running late",
                Details = request.Details ?? "Patient marked as running late",
                RecordedBy = request.RecordedBy ?? "System",
                CreatedAt = DateTime.Now
            };

            var activityId = await _activityRepository.CreateActivityAsync(activity);
            
            return Ok(new { success = true, message = "Late arrival recorded", data = new { activityId } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording late arrival");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("rescheduled")]
    public async Task<IActionResult> RecordRescheduled([FromBody] RescheduleRequest request)
    {
        try
        {
            var activity = new PatientActivityLog
            {
                PatientID = request.PatientID,
                ScheduleID = request.ScheduleID,
                ActivityDate = DateTime.Now,
                ActivityType = "RESCHEDULED",
                Reason = request.Reason ?? "Rescheduled by request",
                Details = $"Changed from {request.OldDateTime:g} to {request.NewDateTime:g}",
                OldDateTime = request.OldDateTime,
                NewDateTime = request.NewDateTime,
                RecordedBy = request.RecordedBy ?? "System",
                CreatedAt = DateTime.Now
            };

            var activityId = await _activityRepository.CreateActivityAsync(activity);
            
            return Ok(new { success = true, message = "Reschedule recorded", data = new { activityId } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording reschedule");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("discharged")]
    public async Task<IActionResult> RecordDischarge([FromBody] DischargeRequest request)
    {
        try
        {
            var activity = new PatientActivityLog
            {
                PatientID = request.PatientID,
                ScheduleID = request.ScheduleID,
                ActivityDate = DateTime.Now,
                ActivityType = "DISCHARGED",
                Reason = request.Reason ?? "Discharged",
                Details = request.Details ?? "Patient discharged from program",
                RecordedBy = request.RecordedBy ?? "System",
                CreatedAt = DateTime.Now
            };

            var activityId = await _activityRepository.CreateActivityAsync(activity);
            
            return Ok(new { success = true, message = "Discharge recorded in history", data = new { activityId } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording discharge");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

// DTOs for specific activity types
public class LateArrivalRequest
{
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public DateTime? ActivityDate { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public string? RecordedBy { get; set; }
}

public class RescheduleRequest
{
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public DateTime OldDateTime { get; set; }
    public DateTime NewDateTime { get; set; }
    public string? Reason { get; set; }
    public string? RecordedBy { get; set; }
}

public class DischargeRequest
{
    public int PatientID { get; set; }
    public int? ScheduleID { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public string? RecordedBy { get; set; }
}
