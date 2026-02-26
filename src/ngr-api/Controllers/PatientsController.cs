using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// Controller for managing patients
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        IAuditService auditService,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>Get all patients with optional filtering</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients(
        [FromQuery] int? careProgramId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var patients = await _patientService.GetPatientsAsync(careProgramId, status, searchTerm, page, pageSize);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            return StatusCode(500, "An error occurred while retrieving patients");
        }
    }

    /// <summary>Get a specific patient by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PatientDto>> GetPatient(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found");
            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while retrieving the patient");
        }
    }

    /// <summary>Create a new patient</summary>
    [HttpPost]
    [Authorize(Policy = "ClinicalUser")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] CreatePatientDto createPatientDto)
    {
        try
        {
            var userEmail = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst("sub")?.Value ?? "Unknown";

            var patient = await _patientService.CreatePatientAsync(createPatientDto, userEmail);

            await _auditService.LogActionAsync(
                "Patient", patient.Id.ToString(), "Create",
                null, patient, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid patient data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, "An error occurred while creating the patient");
        }
    }

    /// <summary>Update an existing patient</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ClinicalUser")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientDto>> UpdatePatient(int id, [FromBody] UpdatePatientDto updatePatientDto)
    {
        try
        {
            var userEmail = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst("sub")?.Value ?? "Unknown";

            var existingPatient = await _patientService.GetPatientByIdAsync(id);
            if (existingPatient == null)
                return NotFound($"Patient with ID {id} not found");

            var updatedPatient = await _patientService.UpdatePatientAsync(id, updatePatientDto, userEmail);

            await _auditService.LogActionAsync(
                "Patient", id.ToString(), "Update",
                existingPatient, updatedPatient, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(updatedPatient);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid patient data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while updating the patient");
        }
    }

    /// <summary>Soft-delete a patient (sets status to Inactive)</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ProgramAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePatient(int id)
    {
        try
        {
            var userEmail = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst("sub")?.Value ?? "Unknown";

            var existingPatient = await _patientService.GetPatientByIdAsync(id);
            if (existingPatient == null)
                return NotFound($"Patient with ID {id} not found");

            await _patientService.DeletePatientAsync(id, userEmail);

            await _auditService.LogActionAsync(
                "Patient", id.ToString(), "Delete",
                existingPatient, null, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while deleting the patient");
        }
    }

    /// <summary>Get patient count, optionally by care program</summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetPatientCount([FromQuery] int? careProgramId = null)
    {
        try
        {
            var count = await _patientService.GetPatientCountAsync(careProgramId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient count");
            return StatusCode(500, "An error occurred while retrieving patient count");
        }
    }
}
