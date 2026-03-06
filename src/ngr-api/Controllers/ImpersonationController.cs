using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// Foundation Admin user impersonation (SRS Section 5.2.3).
/// Start and end impersonation sessions, and query session status.
/// </summary>
[ApiController]
[Route("api/admin/impersonation")]
public class ImpersonationController : ControllerBase
{
    private readonly IImpersonationService _impersonationService;
    private readonly ILogger<ImpersonationController> _logger;

    public ImpersonationController(
        IImpersonationService impersonationService,
        ILogger<ImpersonationController> logger)
    {
        _impersonationService = impersonationService;
        _logger = logger;
    }

    /// <summary>Start an impersonation session (FoundationAnalyst only)</summary>
    [HttpPost("start")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ImpersonationSessionDto>> Start([FromBody] StartImpersonationDto dto)
    {
        // Reject if the admin is already in an impersonation session
        if (HttpContext.Items.ContainsKey("IsImpersonating"))
            return Conflict("Cannot start a new impersonation session while already impersonating a user.");

        var adminUserId = User.FindFirstValue("sub")
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? "unknown";
        var adminEmail  = User.FindFirstValue(ClaimTypes.Email)
                       ?? User.FindFirstValue("email")
                       ?? "unknown";

        try
        {
            var session = await _impersonationService.StartAsync(adminUserId, adminEmail, dto.TargetUserId);
            return Ok(session);
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

    /// <summary>End the current impersonation session</summary>
    [HttpPost("end")]
    [Authorize]
    public async Task<IActionResult> End([FromBody] EndImpersonationDto dto)
    {
        await _impersonationService.EndAsync(dto.SessionId, dto.EndReason ?? "Manual");
        return Ok(new { success = true });
    }

    /// <summary>Get the current active session for the calling admin</summary>
    [HttpGet("status")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<ActionResult<ImpersonationSessionDto>> Status()
    {
        var adminUserId = User.FindFirstValue("sub")
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? "unknown";

        var session = await _impersonationService.GetActiveSessionAsync(adminUserId);
        if (session == null) return NoContent();
        return Ok(session);
    }
}
