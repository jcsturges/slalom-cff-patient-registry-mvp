using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        IAuditService auditService,
        ApplicationDbContext context,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _auditService = auditService;
        _context = context;
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

    // ── Dashboard Endpoint (05-001) ─────────────────────────────

    /// <summary>Get patient dashboard with all form tables and files</summary>
    [HttpGet("{id:int}/dashboard")]
    public async Task<ActionResult<FormSubmissionDto>> GetDashboard(
        int id, [FromServices] IFormService formService)
    {
        try
        {
            var dashboard = await formService.GetPatientDashboardAsync(id, _patientService);
            return Ok(dashboard);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ── Form Submissions (05-002) ────────────────────────────────

    /// <summary>Get form submissions for a patient</summary>
    [HttpGet("{id:int}/forms")]
    public async Task<ActionResult<IEnumerable<FormSubmissionDto>>> GetFormSubmissions(
        int id,
        [FromQuery] string? formCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromServices] IFormService formService = null!)
    {
        var submissions = await formService.GetPatientFormSubmissionsAsync(id, formCode, page, pageSize);
        return Ok(submissions);
    }

    /// <summary>Create a form submission for a patient</summary>
    [HttpPost("{id:int}/forms")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<FormSubmissionDto>> CreateFormSubmission(
        int id,
        [FromBody] CreateFormSubmissionDto dto,
        [FromServices] IFormService formService)
    {
        if (dto.PatientId != id) dto.PatientId = id;
        var (_, userEmail) = GetUserInfo();
        var result = await formService.CreateFormSubmissionAsync(dto, userEmail);
        return CreatedAtAction(nameof(GetFormSubmissions), new { id }, result);
    }

    /// <summary>Get a specific form submission with schema</summary>
    [HttpGet("{id:int}/forms/{formId:int}")]
    public async Task<ActionResult<FormSubmissionDto>> GetFormSubmission(
        int id, int formId,
        [FromServices] IFormService formService)
    {
        var submission = await formService.GetFormSubmissionByIdAsync(formId);
        if (submission == null) return NotFound();
        return Ok(submission);
    }

    /// <summary>Update form data (save / mark complete)</summary>
    [HttpPut("{id:int}/forms/{formId:int}")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<ActionResult<FormSubmissionDto>> UpdateFormData(
        int id, int formId,
        [FromBody] UpdateFormDataDto dto,
        [FromServices] IFormService formService)
    {
        try
        {
            var isAdmin = User.IsInRole("FoundationAnalyst") || User.IsInRole("SystemAdmin");
            var (_, userEmail) = GetUserInfo();
            var result = await formService.UpdateFormDataAsync(formId, dto, userEmail, isAdmin);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Validate form data without saving</summary>
    [HttpPost("{id:int}/forms/{formId:int}/validate")]
    public async Task<ActionResult<FormValidationResultDto>> ValidateForm(
        int id, int formId,
        [FromServices] IFormService formService)
    {
        var submission = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .FirstOrDefaultAsync(f => f.Id == formId);
        if (submission == null) return NotFound();
        var result = formService.ValidateFormData(submission);
        return Ok(result);
    }

    /// <summary>Delete a form submission</summary>
    [HttpDelete("{id:int}/forms/{formId:int}")]
    [Authorize(Policy = "ClinicalUser")]
    public async Task<IActionResult> DeleteFormSubmission(
        int id, int formId,
        [FromServices] IFormService formService)
    {
        try
        {
            var isAdmin = User.IsInRole("FoundationAnalyst") || User.IsInRole("SystemAdmin");
            var deleted = await formService.DeleteFormSubmissionAsync(formId, isAdmin);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Execute database lock for a reporting year (Foundation Admin only)</summary>
    [HttpPost("database-lock")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<DatabaseLockResultDto>> ExecuteDatabaseLock(
        [FromBody] DatabaseLockRequestDto dto,
        [FromServices] IFormService formService)
    {
        var (_, userEmail) = GetUserInfo();
        var result = await formService.ExecuteDatabaseLockAsync(dto.ReportingYear, userEmail);
        return Ok(result);
    }

    // ── Hard-Delete (05-005) ─────────────────────────────────────

    /// <summary>Permanently delete a patient record (Foundation Admin only)</summary>
    [HttpPost("{id:int}/hard-delete")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<IActionResult> HardDelete(int id, [FromBody] HardDeleteConfirmDto dto)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null) return NotFound();

        if (patient.CffId != dto.ConfirmCffId)
            return BadRequest("CFF ID confirmation does not match. Hard-delete aborted.");

        var (userId, userEmail) = GetUserInfo();

        // Log audit BEFORE deleting (no PHI — only CFF ID)
        await _auditService.LogActionAsync(
            "Patient", id.ToString(), "HardDelete",
            new { CffId = patient.CffId }, null, userId, userEmail,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        // Cascade delete all related data
        var entity = await _patientService.GetPatientByIdAsync(id);
        if (entity == null) return NotFound();

        // Use EF cascade delete
        var patientEntity = await _context.Patients.FindAsync(id);
        if (patientEntity != null)
        {
            _context.Patients.Remove(patientEntity);
            await _context.SaveChangesAsync();
        }

        _logger.LogWarning("Patient {PatientId} (CFF ID {CffId}) hard-deleted by {User}",
            id, patient.CffId, userEmail);

        return NoContent();
    }

    // ── Bulk Association Modification (05-004) ───────────────────

    /// <summary>Bulk modify program associations for multiple patients (Foundation Admin)</summary>
    [HttpPost("bulk-association")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<BulkAssociationResultDto>> BulkModifyAssociations(
        [FromBody] BulkAssociationModifyDto dto)
    {
        var (userId, userEmail) = GetUserInfo();
        int affected = 0;

        foreach (var patientId in dto.PatientIds)
        {
            try
            {
                switch (dto.Action)
                {
                    case "remove_all_consent":
                    case "remove_all_withdrawal":
                        await _patientService.RemoveFromProgramAsync(
                            patientId, 0,
                            new RemovePatientFromProgramDto { RemovalReason = "Patient withdrew consent" },
                            userEmail);
                        affected++;
                        break;

                    case "remove_all_inactivity":
                        // Remove from all active programs
                        var associations = await _patientService.GetProgramAssociationsAsync(patientId);
                        foreach (var assoc in associations.Where(a => a.Status == "Active"))
                        {
                            await _patientService.RemoveFromProgramAsync(
                                patientId, assoc.ProgramId,
                                new RemovePatientFromProgramDto { RemovalReason = dto.Reason ?? "Inactivity" },
                                userEmail);
                        }
                        affected++;
                        break;

                    case "add_to_program":
                        if (dto.TargetProgramId.HasValue)
                        {
                            await _patientService.AddToProgramAsync(patientId,
                                new AddPatientToProgramDto { ProgramId = dto.TargetProgramId.Value, IsPrimaryProgram = false },
                                userEmail);
                            affected++;
                        }
                        break;

                    case "transfer":
                        if (dto.SourceProgramId.HasValue && dto.TargetProgramId.HasValue)
                        {
                            await _patientService.RemoveFromProgramAsync(
                                patientId, dto.SourceProgramId.Value,
                                new RemovePatientFromProgramDto { RemovalReason = dto.Reason ?? "Transfer" },
                                userEmail);
                            await _patientService.AddToProgramAsync(patientId,
                                new AddPatientToProgramDto { ProgramId = dto.TargetProgramId.Value, IsPrimaryProgram = false },
                                userEmail);
                            affected++;
                        }
                        break;

                    case "remove_from_program":
                        if (dto.SourceProgramId.HasValue)
                        {
                            await _patientService.RemoveFromProgramAsync(
                                patientId, dto.SourceProgramId.Value,
                                new RemovePatientFromProgramDto { RemovalReason = dto.Reason ?? "Admin removal" },
                                userEmail);
                            affected++;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bulk action failed for patient {PatientId}", patientId);
            }
        }

        await _auditService.LogActionAsync(
            "BulkAssociationModify", "batch", dto.Action,
            null, new { dto.PatientIds, dto.Action, dto.TargetProgramId, dto.Reason, Affected = affected },
            userId, userEmail, HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new BulkAssociationResultDto
        {
            PatientsAffected = affected,
            Action = dto.Action,
            Status = "Completed",
        });
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
