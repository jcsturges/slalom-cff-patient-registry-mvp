using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

/// <summary>
/// Annual database lock management (SRS Section 3.8.7, 6.2.3).
/// Restricted to FoundationAnalyst (= Foundation Admin) role.
/// </summary>
[ApiController]
[Route("api/admin/database-locks")]
[Authorize(Policy = "FoundationAnalyst")]
public class DatabaseLockController : ControllerBase
{
    private readonly IDatabaseLockService _lockService;
    private readonly ILogger<DatabaseLockController> _logger;

    public DatabaseLockController(IDatabaseLockService lockService, ILogger<DatabaseLockController> logger)
    {
        _lockService = lockService;
        _logger = logger;
    }

    /// <summary>List all lock operations (history + current status)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DatabaseLockDto>>> GetLocks()
    {
        var locks = await _lockService.GetLocksAsync();
        return Ok(locks);
    }

    /// <summary>Get count of forms that would be affected by locking a reporting year</summary>
    [HttpGet("impact")]
    public async Task<ActionResult<DatabaseLockImpactDto>> GetImpact([FromQuery] int reportingYear)
    {
        if (reportingYear < 2000 || reportingYear > DateTime.UtcNow.Year)
            return BadRequest("Invalid reporting year.");

        var impact = await _lockService.GetImpactAsync(reportingYear);
        return Ok(impact);
    }

    /// <summary>Get progress for a specific lock operation (for polling in-progress locks)</summary>
    [HttpGet("{id:int}/progress")]
    public async Task<ActionResult<DatabaseLockProgressDto>> GetProgress(int id)
    {
        try
        {
            var progress = await _lockService.GetProgressAsync(id);
            return Ok(progress);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Initiate a lock operation (synchronous or batch)</summary>
    [HttpPost]
    public async Task<ActionResult<DatabaseLockDto>> CreateLock([FromBody] CreateDatabaseLockDto dto)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("email")
                     ?? User.FindFirstValue("sub")
                     ?? "unknown";

        if (dto.ReportingYear < 2000 || dto.ReportingYear > DateTime.UtcNow.Year)
            return BadRequest("Invalid reporting year.");

        if (dto.LockDate <= DateTime.UtcNow)
            return BadRequest("Lock date must be a future date.");

        if (dto.ExecutionMode == "Batch" && dto.ScheduledDate == null)
            return BadRequest("ScheduledDate is required for Batch execution mode.");

        try
        {
            DatabaseLockDto result = dto.ExecutionMode switch
            {
                "Synchronous" => await _lockService.ExecuteSynchronousLockAsync(
                    dto.ReportingYear, dto.LockDate, userEmail),
                "Batch" => await _lockService.ScheduleBatchLockAsync(
                    dto.ReportingYear, dto.LockDate, dto.ScheduledDate!.Value, userEmail),
                _ => throw new ArgumentException($"Unknown execution mode: {dto.ExecutionMode}"),
            };

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Lock operation failed for year {Year}", dto.ReportingYear);
            return StatusCode(500, ex.Message);
        }
    }
}
