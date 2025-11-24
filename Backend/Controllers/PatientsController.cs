using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HDScheduler.API.Models;
using HDScheduler.API.Repositories;
using HDScheduler.API.DTOs;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientRepository patientRepository, ILogger<PatientsController> logger)
    {
        _patientRepository = patientRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
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
    [Authorize(Roles = "Admin,HOD,Doctor,Nurse,Technician")]
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
    [Authorize(Roles = "Admin,Doctor,Nurse")]
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
                IsActive = true
            };

            var patientId = await _patientRepository.CreateAsync(patient);
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
}
