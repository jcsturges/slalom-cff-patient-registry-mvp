using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// EMR CSV upload and import history endpoints (SRS Section 9 / Story 10-001, 10-003, 10-004).
/// Accessible to ClinicalUser policy (ClinicalUser, ProgramAdmin, SystemAdmin).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ClinicalUser")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IEmrMappingService _mappingService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        IImportService importService,
        IEmrMappingService mappingService,
        ILogger<ImportController> logger)
    {
        _importService = importService;
        _mappingService = mappingService;
        _logger = logger;
    }

    // ── CSV Upload ────────────────────────────────────────────────────────────

    /// <summary>Upload an EMR CSV file for a given program</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<ActionResult<ImportJobDto>> Upload(
        [FromForm] IFormFile file,
        [FromForm] int programId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("sub")
                     ?? "unknown";

        try
        {
            var job = await _importService.UploadCsvAsync(file, programId, userEmail);
            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Validate only (no persist) ────────────────────────────────────────────

    /// <summary>Validate a CSV without persisting — returns errors/warnings</summary>
    [HttpPost("validate")]
    [RequestSizeLimit(52_428_800)]
    public async Task<ActionResult<EmrValidationResult>> Validate(
        [FromForm] IFormFile file,
        [FromForm] int programId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        using var stream = file.OpenReadStream();
        var result = await _importService.ValidateCsvAsync(stream, programId);
        return Ok(result);
    }

    // ── Import history ────────────────────────────────────────────────────────

    /// <summary>List import jobs for a program (upload history)</summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<IEnumerable<ImportJobDto>>> GetJobs([FromQuery] int programId)
    {
        var jobs = await _importService.GetImportJobsByProgramAsync(programId);
        return Ok(jobs);
    }

    /// <summary>Get summary of a single import job</summary>
    [HttpGet("jobs/{id:int}")]
    public async Task<ActionResult<ImportJobDto>> GetJob(int id)
    {
        var job = await _importService.GetImportJobAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    /// <summary>Get full details including errors and warnings for a job</summary>
    [HttpGet("jobs/{id:int}/errors")]
    public async Task<ActionResult<ImportJobDetailDto>> GetJobErrors(int id)
    {
        var detail = await _importService.GetImportJobDetailAsync(id);
        if (detail == null) return NotFound();
        return Ok(detail);
    }

    // ── Field mappings ────────────────────────────────────────────────────────

    /// <summary>Get effective field mappings for a program</summary>
    [HttpGet("mappings")]
    public async Task<ActionResult<IEnumerable<EmrFieldMappingDto>>> GetMappings([FromQuery] int programId)
    {
        var mappings = await _mappingService.GetEffectiveMappingsAsync(programId);
        var dtos = mappings.Values.Select(m => new EmrFieldMappingDto
        {
            Id = m.Id,
            ProgramId = m.ProgramId,
            CsvColumnName = m.CsvColumnName,
            FormType = m.FormType,
            FieldPath = m.FieldPath,
            DataType = m.DataType,
            IsRequired = m.IsRequired,
            TransformHint = m.TransformHint,
            IsActive = m.IsActive,
        });
        return Ok(dtos);
    }
}
