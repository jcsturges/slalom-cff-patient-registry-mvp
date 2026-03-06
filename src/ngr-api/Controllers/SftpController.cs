using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Controllers;

/// <summary>
/// SFTP configuration management endpoints (SRS Section 9 / Story 10-002).
/// Foundation Admin only — manages per-program automated transfer configuration.
/// Note: actual SFTP server infrastructure is provisioned separately (see runbook).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "FoundationAnalyst")]
public class SftpController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SftpController> _logger;

    public SftpController(ApplicationDbContext context, ILogger<SftpController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Get SFTP config for a program</summary>
    [HttpGet("{programId:int}")]
    public async Task<ActionResult<SftpConfigDto>> Get(int programId)
    {
        var config = await _context.SftpConfigs.FirstOrDefaultAsync(c => c.ProgramId == programId);
        if (config == null) return NotFound();
        return Ok(MapToDto(config));
    }

    /// <summary>Create or update SFTP config for a program</summary>
    [HttpPut("{programId:int}")]
    public async Task<ActionResult<SftpConfigDto>> Upsert(int programId, [FromBody] UpsertSftpConfigDto dto)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";

        var config = await _context.SftpConfigs.FirstOrDefaultAsync(c => c.ProgramId == programId);
        if (config == null)
        {
            config = new SftpConfig
            {
                ProgramId = programId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userEmail,
            };
            _context.SftpConfigs.Add(config);
        }

        config.Host = dto.Host;
        config.Port = dto.Port;
        config.Username = dto.Username;
        config.RemoteDirectory = dto.RemoteDirectory;
        config.FilePattern = dto.FilePattern;
        config.ScheduleCron = dto.ScheduleCron;
        config.IsEnabled = dto.IsEnabled;
        config.UpdatedAt = DateTime.UtcNow;
        config.UpdatedBy = userEmail;

        // Only update password when explicitly provided
        if (!string.IsNullOrEmpty(dto.Password))
        {
            // In production: encrypt via Azure Key Vault before storing.
            // Local dev: store as-is (placeholder — do not use plaintext in prod).
            config.EncryptedPassword = dto.Password;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("SFTP config upserted for program {ProgramId} by {User}", programId, userEmail);
        return Ok(MapToDto(config));
    }

    /// <summary>Delete SFTP config for a program</summary>
    [HttpDelete("{programId:int}")]
    public async Task<IActionResult> Delete(int programId)
    {
        var config = await _context.SftpConfigs.FirstOrDefaultAsync(c => c.ProgramId == programId);
        if (config == null) return NotFound();
        _context.SftpConfigs.Remove(config);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static SftpConfigDto MapToDto(SftpConfig c) => new()
    {
        Id = c.Id,
        ProgramId = c.ProgramId,
        Host = c.Host,
        Port = c.Port,
        Username = c.Username,
        RemoteDirectory = c.RemoteDirectory,
        FilePattern = c.FilePattern,
        ScheduleCron = c.ScheduleCron,
        IsEnabled = c.IsEnabled,
        LastRunAt = c.LastRunAt,
        LastRunStatus = c.LastRunStatus,
    };
}
