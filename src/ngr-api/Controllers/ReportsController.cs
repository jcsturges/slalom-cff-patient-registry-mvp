using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportingService reportingService, ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    private (string UserId, string UserEmail) GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "unknown";
        return (userId, userEmail);
    }

    // ── Saved Reports CRUD ───────────────────────────────────────

    [HttpGet("saved")]
    public async Task<ActionResult<IEnumerable<SavedReportDto>>> GetSavedReports(
        [FromQuery] string? scope, [FromQuery] int? programId)
    {
        var (_, userEmail) = GetUserInfo();
        var reports = await _reportingService.GetSavedReportsAsync(scope, programId, userEmail);
        return Ok(reports);
    }

    [HttpGet("saved/{id:int}")]
    public async Task<ActionResult<SavedReportDto>> GetSavedReport(int id)
    {
        var report = await _reportingService.GetSavedReportByIdAsync(id);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpPost("saved")]
    public async Task<ActionResult<SavedReportDto>> CreateSavedReport([FromBody] CreateSavedReportDto dto)
    {
        var (_, userEmail) = GetUserInfo();
        var report = await _reportingService.CreateSavedReportAsync(dto, userEmail);
        return CreatedAtAction(nameof(GetSavedReport), new { id = report.Id }, report);
    }

    [HttpPut("saved/{id:int}")]
    public async Task<ActionResult<SavedReportDto>> UpdateSavedReport(int id, [FromBody] UpdateSavedReportDto dto)
    {
        var report = await _reportingService.UpdateSavedReportAsync(id, dto);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpDelete("saved/{id:int}")]
    public async Task<IActionResult> DeleteSavedReport(int id)
    {
        var deleted = await _reportingService.DeleteSavedReportAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ── Execution ────────────────────────────────────────────────

    [HttpPost("execute")]
    public async Task<ActionResult<ReportResultDto>> ExecuteReport([FromBody] ExecuteReportDto dto)
    {
        var (_, userEmail) = GetUserInfo();
        var result = await _reportingService.ExecuteReportAsync(dto, userEmail);
        return Ok(result);
    }

    // ── Pre-defined Reports ──────────────────────────────────────

    [HttpGet("incomplete-records")]
    public async Task<ActionResult<ReportResultDto>> IncompleteRecords(
        [FromQuery] int programId, [FromQuery] int reportingYear)
    {
        var (_, userEmail) = GetUserInfo();
        var result = await _reportingService.RunIncompleteRecordsReportAsync(programId, reportingYear, userEmail);
        return Ok(result);
    }

    [HttpGet("patients-due-visit")]
    public async Task<ActionResult<ReportResultDto>> PatientsDueVisit([FromQuery] int programId)
    {
        var (_, userEmail) = GetUserInfo();
        var result = await _reportingService.RunPatientsDueVisitReportAsync(programId, userEmail);
        return Ok(result);
    }

    [HttpGet("diabetes-testing")]
    public async Task<ActionResult<ReportResultDto>> DiabetesTesting([FromQuery] int programId)
    {
        var (_, userEmail) = GetUserInfo();
        var result = await _reportingService.RunDiabetesTestingReportAsync(programId, userEmail);
        return Ok(result);
    }

    // ── Admin Reports (Foundation Admin only) ────────────────────

    [HttpGet("admin/program-list")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> ProgramListReport()
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunProgramListReportAsync(userEmail));
    }

    [HttpGet("admin/merges")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> MergeReport()
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunMergeReportAsync(userEmail));
    }

    [HttpGet("admin/transfers")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> TransferReport()
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunTransferReportAsync(userEmail));
    }

    [HttpGet("admin/file-uploads")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> FileUploadReport()
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunFileUploadReportAsync(userEmail));
    }

    // ── Audit Reports ────────────────────────────────────────────

    [HttpGet("audit/user-management")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> UserManagementAudit(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunUserManagementAuditAsync(startDate, endDate, userEmail));
    }

    [HttpGet("audit/downloads")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ReportResultDto>> DownloadAudit(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var (_, userEmail) = GetUserInfo();
        return Ok(await _reportingService.RunDownloadAuditAsync(startDate, endDate, userEmail));
    }

    // ── Download ─────────────────────────────────────────────────

    [HttpPost("download")]
    public async Task<IActionResult> DownloadReport([FromBody] ReportDownloadRequestDto dto)
    {
        try
        {
            var (_, userEmail) = GetUserInfo();
            var role = User.IsInRole("FoundationAnalyst") ? "FoundationAnalyst" : "ClinicalUser";
            var bytes = await _reportingService.GenerateDownloadAsync(
                dto.ExecutionId, dto.Format, userEmail, role, null);

            var contentType = dto.Format == "excel"
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "text/csv";
            var ext = dto.Format == "excel" ? ".xlsx" : ".csv";

            return File(bytes, contentType, $"report_{dto.ExecutionId}{ext}");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
