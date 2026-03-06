using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Controllers;

/// <summary>
/// Receives lightweight user interaction events from the SPA (12-004)
/// and exposes a CSV export for Foundation Admins (12-005).
/// No PHI — events contain only opaque user IDs, page paths, component names.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize]
public class UserEventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserEventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public record TrackEventDto(
        string EventType,
        string? Page,
        string? Component,
        object? Properties,
        string? SessionId,
        DateTime? OccurredAt);

    /// <summary>
    /// Batch-ingest analytics events from the SPA.
    /// Accepts a JSON array of events; inserts all in one SaveChanges call.
    /// </summary>
    [HttpPost("events")]
    public async Task<IActionResult> Track([FromBody] IEnumerable<TrackEventDto> events)
    {
        var userId = User.FindFirst("sub")?.Value
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? "unknown";

        foreach (var e in events)
        {
            if (string.IsNullOrWhiteSpace(e.EventType)) continue;

            _context.UserEvents.Add(new UserEvent
            {
                UserId         = userId,
                SessionId      = e.SessionId,
                EventType      = e.EventType,
                Page           = e.Page,
                Component      = e.Component,
                PropertiesJson = e.Properties != null ? JsonSerializer.Serialize(e.Properties) : null,
                OccurredAt     = e.OccurredAt?.ToUniversalTime() ?? DateTime.UtcNow,
            });
        }

        await _context.SaveChangesAsync();
        return Accepted();
    }

    /// <summary>
    /// Export user events as CSV for a date range (12-005). Foundation Admin only.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Policy = "FoundationAnalyst")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? eventType)
    {
        var query = _context.UserEvents
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Select(e => new
            {
                e.Id, e.UserId, e.SessionId, e.EventType,
                e.Page, e.Component, e.OccurredAt, e.PropertiesJson
            })
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,UserId,SessionId,EventType,Page,Component,OccurredAt,Properties");
        foreach (var e in events)
        {
            csv.AppendLine(string.Join(",",
                e.Id, Esc(e.UserId), Esc(e.SessionId), Esc(e.EventType),
                Esc(e.Page), Esc(e.Component), e.OccurredAt.ToString("O"),
                Esc(e.PropertiesJson)));
        }

        var bytes    = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"user-events-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private static string Esc(string? v) =>
        v == null ? string.Empty : $"\"{v.Replace("\"", "\"\"")}\"";
}
