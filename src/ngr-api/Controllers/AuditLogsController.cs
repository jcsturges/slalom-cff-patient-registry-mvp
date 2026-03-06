using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;

namespace NgrApi.Controllers;

/// <summary>
/// Audit log query and export endpoints (12-001, 12-002, 12-005).
/// All endpoints are Foundation Analyst+ only. PHI is never returned — entity IDs are opaque.
/// </summary>
[ApiController]
[Route("api/audit")]
[Authorize(Policy = "FoundationAnalyst")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Query audit logs with optional filters (12-001).
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? isImpersonated,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page     = Math.Max(1, page);

        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(l => l.EntityId == entityId);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(l => l.UserId == userId);
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);
        if (isImpersonated.HasValue)
            query = query.Where(l => l.IsImpersonated == isImpersonated.Value);

        var total = await query.CountAsync();
        var logs  = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.EntityType,
                l.EntityId,
                l.Action,
                l.UserId,
                l.UserEmail,
                l.Timestamp,
                l.IpAddress,
                l.OldValues,
                l.NewValues,
                l.IsImpersonated,
                l.ActingAdminId,
                l.ImpersonationSessionId,
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, logs });
    }

    /// <summary>
    /// Get audit history for a specific patient by CFF ID (12-002).
    /// Returns field-level change records without PHI.
    /// </summary>
    [HttpGet("patients/{cffId:long}")]
    public async Task<IActionResult> GetPatientAuditHistory(
        long cffId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page     = Math.Max(1, page);

        // Resolve the patient's opaque DB ID from CFF ID — never expose CFF ID in logs
        var patient = await _context.Patients
            .Where(p => p.CffId == cffId)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync();

        if (patient == null) return NotFound();

        var entityId = patient.Id.ToString();

        var query = _context.AuditLogs
            .Where(l => l.EntityType == "Patient" && l.EntityId == entityId);

        if (from.HasValue) query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)   query = query.Where(l => l.Timestamp <= to.Value);

        var total = await query.CountAsync();
        var logs  = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.Action,
                l.UserId,
                l.UserEmail,
                l.Timestamp,
                l.OldValues,
                l.NewValues,
                l.IsImpersonated,
                l.ActingAdminId,
            })
            .ToListAsync();

        return Ok(new { cffId, total, page, pageSize, logs });
    }

    /// <summary>
    /// Export audit logs as CSV for a date range (12-005).
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? entityType)
    {
        var query = _context.AuditLogs
            .Where(l => l.Timestamp >= from && l.Timestamp <= to);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new
            {
                l.Id, l.EntityType, l.EntityId, l.Action,
                l.UserId, l.UserEmail, l.Timestamp, l.IpAddress,
                l.IsImpersonated, l.ActingAdminId, l.ImpersonationSessionId,
                l.OldValues, l.NewValues
            })
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,EntityType,EntityId,Action,UserId,UserEmail,Timestamp,IpAddress,IsImpersonated,ActingAdminId,ImpersonationSessionId,OldValues,NewValues");
        foreach (var l in logs)
        {
            csv.AppendLine(string.Join(",",
                l.Id, Esc(l.EntityType), Esc(l.EntityId), Esc(l.Action),
                Esc(l.UserId), Esc(l.UserEmail), l.Timestamp.ToString("O"),
                Esc(l.IpAddress), l.IsImpersonated, Esc(l.ActingAdminId),
                l.ImpersonationSessionId?.ToString(), Esc(l.OldValues), Esc(l.NewValues)));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"audit-export-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private static string Esc(string? v) =>
        v == null ? string.Empty : $"\"{v.Replace("\"", "\"\"")}\"";
}
