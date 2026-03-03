using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/data-export")]
[Authorize]
public class DataExportController : ControllerBase
{
    private readonly IDataExportService _exportService;

    public DataExportController(IDataExportService exportService)
    {
        _exportService = exportService;
    }

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "unknown";

    /// <summary>Execute raw data export and download as ZIP</summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteExport([FromBody] DataExportRequestDto dto)
    {
        var userEmail = GetUserEmail();
        var zipBytes = await _exportService.ExecuteExportAsync(dto, userEmail);
        var fileName = $"NGR_Export_{dto.ProgramId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
        return File(zipBytes, "application/zip", fileName);
    }

    // ── Saved Definitions ────────────────────────────────────────

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<SavedDownloadDefinitionDto>>> GetDefinitions(
        [FromQuery] int? programId)
    {
        var userEmail = GetUserEmail();
        var defs = await _exportService.GetSavedDefinitionsAsync(userEmail, programId);
        return Ok(defs);
    }

    [HttpGet("definitions/{id:int}")]
    public async Task<ActionResult<SavedDownloadDefinitionDto>> GetDefinition(int id)
    {
        var def = await _exportService.GetDefinitionByIdAsync(id);
        if (def == null) return NotFound();
        return Ok(def);
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<SavedDownloadDefinitionDto>> CreateDefinition(
        [FromBody] CreateSavedDownloadDto dto)
    {
        var userEmail = GetUserEmail();
        var def = await _exportService.CreateDefinitionAsync(dto, userEmail);
        return CreatedAtAction(nameof(GetDefinition), new { id = def.Id }, def);
    }

    [HttpPut("definitions/{id:int}")]
    public async Task<ActionResult<SavedDownloadDefinitionDto>> UpdateDefinition(
        int id, [FromBody] UpdateSavedDownloadDto dto)
    {
        var result = await _exportService.UpdateDefinitionAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("definitions/{id:int}")]
    public async Task<IActionResult> DeleteDefinition(int id)
    {
        var deleted = await _exportService.DeleteDefinitionAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
