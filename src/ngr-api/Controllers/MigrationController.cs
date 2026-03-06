using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// Historical data migration management (Epic 13, stories 13-006 to 13-008).
/// All endpoints require SystemAdmin — migration is the highest-risk operation.
/// </summary>
[ApiController]
[Route("api/migration")]
[Authorize(Policy = "SystemAdmin")]
public class MigrationController : ControllerBase
{
    private readonly IMigrationService _migrationService;
    private readonly IMigrationValidationService _validationService;

    public MigrationController(
        IMigrationService migrationService,
        IMigrationValidationService validationService)
    {
        _migrationService  = migrationService;
        _validationService = validationService;
    }

    /// <summary>Get paginated migration run history.</summary>
    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page     = Math.Max(1, page);
        var (runs, total) = await _migrationService.GetRunsAsync(page, pageSize);
        return Ok(new { total, page, pageSize, runs });
    }

    /// <summary>Get a single migration run including validation report (13-008).</summary>
    [HttpGet("runs/{id:int}")]
    public async Task<IActionResult> GetRun(int id)
    {
        var run = await _migrationService.GetRunByIdAsync(id);
        return run == null ? NotFound() : Ok(run);
    }

    /// <summary>
    /// Execute a migration phase (13-006, 13-007).
    /// Valid phases: Demographics, Diagnosis, SweatTest, ALD, Transplant,
    ///               AnnualReview, Encounter, Labs, CareEpisode, PhoneNote, Files
    /// </summary>
    [HttpPost("runs")]
    public async Task<IActionResult> TriggerRun([FromBody] TriggerMigrationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Phase))
            return BadRequest("Phase is required.");

        var userEmail = User.FindFirst("email")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? "unknown";

        var run = await _migrationService.ExecutePhaseAsync(dto.Phase, userEmail);
        return Accepted(run);
    }

    /// <summary>
    /// Run validation checks for a completed migration run (13-008).
    /// Generates a ValidationReport with record counts, spot checks, and integrity checks.
    /// </summary>
    [HttpPost("runs/{id:int}/validation")]
    public async Task<IActionResult> ValidateRun(int id)
    {
        var report = await _validationService.ValidateRunAsync(id);
        return Ok(report);
    }

    /// <summary>Get the validation report for a run (13-008).</summary>
    [HttpGet("runs/{id:int}/validation")]
    public async Task<IActionResult> GetValidationReport(int id)
    {
        var run = await _migrationService.GetRunByIdAsync(id);
        if (run == null) return NotFound();
        if (run.ValidationReport == null)
            return NotFound(new { message = "No validation report exists for this run. POST to /validation to generate one." });
        return Ok(run.ValidationReport);
    }

    /// <summary>List the supported migration phases.</summary>
    [HttpGet("phases")]
    public IActionResult GetPhases()
    {
        return Ok(new
        {
            phases = new[]
            {
                new { id = "Demographics",  description = "Shared form — patient demographics",         srsRef = "11" },
                new { id = "Diagnosis",     description = "Shared form — CF diagnosis data",            srsRef = "11" },
                new { id = "SweatTest",     description = "Shared form — sweat test results",           srsRef = "11" },
                new { id = "ALD",           description = "Shared form — ALD initiation",               srsRef = "11" },
                new { id = "Transplant",    description = "Shared form — transplant data",              srsRef = "11" },
                new { id = "AnnualReview",  description = "Program form — annual reviews (10 yrs)",    srsRef = "11" },
                new { id = "Encounter",     description = "Program form — clinic encounters (10 yrs)", srsRef = "11" },
                new { id = "Labs",          description = "Program form — labs/tests (10 yrs)",        srsRef = "11" },
                new { id = "CareEpisode",   description = "Program form — care episodes (10 yrs)",     srsRef = "11" },
                new { id = "PhoneNote",     description = "Program form — phone notes (10 yrs)",       srsRef = "11" },
                new { id = "Files",         description = "portCF file attachments (excludes deceased patients)", srsRef = "11" },
            },
            migrationUser  = MigrationService.MigrationUser,
            notes = new[]
            {
                "Requires Migration:SourceConnectionString in Azure Key Vault.",
                "Each phase is independently re-runnable.",
                "Migrated records tagged with IsMigrated=true and CreatedBy=migration-service@cff.org.",
                "Files phase skips patients where IsDeceased=true.",
            }
        });
    }
}
