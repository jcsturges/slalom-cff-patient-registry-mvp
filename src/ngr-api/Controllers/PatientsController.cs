using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

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

    private (string UserId, string UserEmail) GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub") ?? "unknown";
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                       ?? User.FindFirstValue("email")
                       ?? User.Identity?.Name ?? "unknown";
        return (userId, userEmail);
    }

    // ── CRUD Endpoints ───────────────────────────────────────────

    /// <summary>Get patients with optional filtering (program-scoped via associations)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients(
        [FromQuery] int? careProgramId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var patients = await _patientService.GetPatientsAsync(careProgramId, status, searchTerm, page, pageSize);
        return Ok(patients);
    }

    /// <summary>Get a specific patient by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetPatient(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    /// <summary>Get patient count, optionally by care program</summary>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetPatientCount([FromQuery] int? careProgramId = null)
    {
        var count = await _patientService.GetPatientCountAsync(careProgramId);
        return Ok(count);
    }

    /// <summary>Create a new patient (04-004)</summary>
    [HttpPost]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] CreatePatientDto dto)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            var patient = await _patientService.CreatePatientAsync(dto, userEmail);

            await _auditService.LogActionAsync(
                "Patient", patient.Id.ToString(), "Create",
                null, patient, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Update an existing patient</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<PatientDto>> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            var existing = await _patientService.GetPatientByIdAsync(id);
            if (existing == null) return NotFound();

            var updated = await _patientService.UpdatePatientAsync(id, dto, userEmail);

            await _auditService.LogActionAsync(
                "Patient", id.ToString(), "Update",
                existing, updated, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Soft-delete a patient (sets status to Inactive)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ProgramAdmin")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            var existing = await _patientService.GetPatientByIdAsync(id);
            if (existing == null) return NotFound();

            await _patientService.DeletePatientAsync(id, userEmail);

            await _auditService.LogActionAsync(
                "Patient", id.ToString(), "Delete",
                existing, null, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Program Association Endpoints (04-002, 04-006, 04-007) ───

    /// <summary>Get all program associations for a patient</summary>
    [HttpGet("{id:int}/programs")]
    public async Task<ActionResult<IEnumerable<PatientProgramAssociationDto>>> GetProgramAssociations(int id)
    {
        var associations = await _patientService.GetProgramAssociationsAsync(id);
        return Ok(associations);
    }

    /// <summary>Add a patient to a program (re-acquisition from ORH supported)</summary>
    [HttpPost("{id:int}/programs")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<PatientProgramAssociationDto>> AddToProgram(
        int id, [FromBody] AddPatientToProgramDto dto)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            var result = await _patientService.AddToProgramAsync(id, dto, userEmail);

            await _auditService.LogActionAsync(
                "PatientProgramAssignment", result.Id.ToString(), "Create",
                null, result, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetProgramAssociations), new { id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Remove a patient from a program (04-007)</summary>
    [HttpPost("{id:int}/programs/{programId:int}/remove")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<IActionResult> RemoveFromProgram(
        int id, int programId, [FromBody] RemovePatientFromProgramDto dto)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            await _patientService.RemoveFromProgramAsync(id, programId, dto, userEmail);

            await _auditService.LogActionAsync(
                "PatientProgramAssignment", $"{id}_{programId}", "Remove",
                null, new { PatientId = id, ProgramId = programId, dto.RemovalReason },
                userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Duplicate Detection Endpoint (04-005) ────────────────────

    /// <summary>Check for duplicate patients before creating a new record</summary>
    [HttpPost("duplicates")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<IEnumerable<DuplicateMatchDto>>> CheckDuplicates(
        [FromBody] DuplicateCheckDto dto)
    {
        var matches = await _patientService.CheckDuplicatesAsync(dto);
        return Ok(matches);
    }

    // ── Merge Endpoints (04-008, 04-009, 04-010) ─────────────────

    /// <summary>Merge two patient records (CP Admin: within-program; Foundation Admin: system-wide)</summary>
    [HttpPost("merge")]
    [Authorize(Policy = "ProgramAdmin")]
    public async Task<ActionResult<MergeResultDto>> MergePatients([FromBody] MergeRequestDto dto)
    {
        try
        {
            var (userId, userEmail) = GetUserInfo();
            var result = await _patientService.MergeAsync(dto, userEmail);

            await _auditService.LogActionAsync(
                "PatientMerge", $"{dto.PrimaryPatientId}_{dto.SecondaryPatientId}", "Merge",
                null, result, userId, userEmail,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
