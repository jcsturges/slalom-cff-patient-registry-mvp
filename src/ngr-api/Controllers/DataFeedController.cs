using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// Outbound nightly data feed management (Epic 13, stories 13-001 to 13-005).
/// All endpoints require SystemAdmin — feed operations are high-privilege.
/// </summary>
[ApiController]
[Route("api/data-feed")]
[Authorize(Policy = "SystemAdmin")]
public class DataFeedController : ControllerBase
{
    private readonly IDataFeedService _feedService;

    public DataFeedController(IDataFeedService feedService)
    {
        _feedService = feedService;
    }

    /// <summary>Get paginated feed run history (13-004).</summary>
    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page     = Math.Max(1, page);
        var (runs, total) = await _feedService.GetRunsAsync(page, pageSize);
        return Ok(new { total, page, pageSize, runs });
    }

    /// <summary>Get a single run including full reconciliation report (13-004).</summary>
    [HttpGet("runs/{id:int}")]
    public async Task<IActionResult> GetRun(int id)
    {
        var run = await _feedService.GetRunByIdAsync(id);
        return run == null ? NotFound() : Ok(run);
    }

    /// <summary>
    /// Trigger a feed run (13-001, 13-003).
    /// Body: { "runType": "Delta" | "Full", "windowOverrideStart": "...", "windowOverrideEnd": "..." }
    /// Full resync uses the same endpoint with RunType="Full".
    /// WindowOverride* lets operators reprocess a specific time window (13-001 AC).
    /// </summary>
    [HttpPost("runs")]
    public async Task<IActionResult> TriggerRun([FromBody] TriggerFeedRunDto dto)
    {
        var userEmail = User.FindFirst("email")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? "unknown";

        FeedRunDto run;
        if (dto.RunType.Equals("Full", StringComparison.OrdinalIgnoreCase))
        {
            // Full resync — 13-003: operational safeguard (SystemAdmin already enforced by policy)
            run = await _feedService.RunFullResyncAsync(userEmail);
        }
        else
        {
            run = await _feedService.RunDeltaFeedAsync(
                userEmail,
                dto.WindowOverrideStart,
                dto.WindowOverrideEnd);
        }

        return Accepted(run);
    }

    /// <summary>List active field mappings (13-001).</summary>
    [HttpGet("field-mappings")]
    public async Task<IActionResult> GetFieldMappings()
    {
        var mappings = await _feedService.GetFieldMappingsAsync();
        return Ok(mappings);
    }

    /// <summary>Update a field mapping — creates a new versioned row (13-001).</summary>
    [HttpPut("field-mappings/{id:int}")]
    public async Task<IActionResult> UpdateFieldMapping(int id, [FromBody] UpdateFeedFieldMappingDto dto)
    {
        var userEmail = User.FindFirst("email")?.Value ?? "unknown";
        var mapping = await _feedService.UpdateFieldMappingAsync(id, dto, userEmail);
        return mapping == null ? NotFound() : Ok(mapping);
    }
}
