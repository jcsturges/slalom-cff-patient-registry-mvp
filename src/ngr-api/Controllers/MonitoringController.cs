using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;

namespace NgrApi.Controllers;

/// <summary>
/// System health and monitoring metrics (12-006).
/// Foundation Analyst+ only.
/// </summary>
[ApiController]
[Route("api/admin/monitoring")]
[Authorize(Policy = "FoundationAnalyst")]
public class MonitoringController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public MonitoringController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns system health metrics: uptime, DB latency, entity counts, recent error rate.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMetrics()
    {
        var sw = Stopwatch.StartNew();
        bool dbHealthy;
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            dbHealthy = true;
        }
        catch
        {
            dbHealthy = false;
        }
        sw.Stop();

        var cutoff24h = DateTime.UtcNow.AddHours(-24);

        var patientCount  = await _context.Patients.CountAsync();
        var userCount     = await _context.Users.CountAsync();
        var recentAudits  = await _context.AuditLogs.CountAsync(l => l.Timestamp >= cutoff24h);

        // Error-proxy: count audit log entries with "Failed" or "Error" in action
        // (a rough indicator; real error tracking lives in Application Insights)
        var errorCount = await _context.ReportExecutions
            .CountAsync(e => e.ExecutedAt >= cutoff24h && e.Status == "Failed");

        return Ok(new
        {
            uptime = new
            {
                serverStartedAt = _startTime,
                uptimeHours     = (DateTime.UtcNow - _startTime).TotalHours,
            },
            database = new
            {
                healthy       = dbHealthy,
                latencyMs     = sw.ElapsedMilliseconds,
            },
            counts = new
            {
                patients = patientCount,
                users    = userCount,
            },
            last24h = new
            {
                auditEvents    = recentAudits,
                reportFailures = errorCount,
            },
            applicationInsights = new
            {
                note = "Real-time metrics (error rate, response times, availability) are visible in Azure Application Insights.",
            },
            generatedAt = DateTime.UtcNow,
        });
    }
}
