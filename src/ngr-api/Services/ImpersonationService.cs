using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public class ImpersonationService : IImpersonationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ImpersonationService> _logger;
    private readonly IConfiguration _config;

    // Default session duration — configurable via Impersonation:DurationMinutes
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(60);

    public ImpersonationService(
        ApplicationDbContext context,
        IUserService userService,
        IAuditService auditService,
        IConfiguration config,
        ILogger<ImpersonationService> logger)
    {
        _context = context;
        _userService = userService;
        _auditService = auditService;
        _config = config;
        _logger = logger;
    }

    public async Task<ImpersonationSessionDto> StartAsync(
        string adminUserId, string adminEmail,
        string targetUserId, TimeSpan? duration = null)
    {
        // Only one active session per admin at a time
        var existing = await _context.ImpersonationSessions
            .FirstOrDefaultAsync(s => s.AdminUserId == adminUserId
                                   && s.EndedAt == null
                                   && s.ExpiresAt > DateTime.UtcNow);
        if (existing != null)
            throw new InvalidOperationException("An active impersonation session already exists. End it before starting a new one.");

        // Resolve target user
        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.OktaId == targetUserId || u.Email == targetUserId);

        if (targetUser == null || !targetUser.IsActive)
            throw new ArgumentException($"Target user '{targetUserId}' not found or is inactive.");

        // Look up the target user's distinct role names from UserProgramRoles
        var roleNames = await _context.UserProgramRoles
            .Where(r => r.UserId == targetUser.Id && r.Status == "Active")
            .Include(r => r.Role)
            .Select(r => r.Role.Name)
            .Distinct()
            .ToListAsync();

        // Add the "Everyone" group (always present in Okta)
        var groups = new List<string> { "Everyone" };
        groups.AddRange(roleNames.Where(r => !string.IsNullOrEmpty(r)));

        var sessionDuration = duration ?? GetConfiguredDuration();
        var now = DateTime.UtcNow;

        var session = new ImpersonationSession
        {
            Id                  = Guid.NewGuid(),
            AdminUserId         = adminUserId,
            AdminEmail          = adminEmail,
            TargetUserId        = targetUser.OktaId,
            TargetUserEmail     = targetUser.Email,
            TargetUserName      = $"{targetUser.FirstName} {targetUser.LastName}".Trim(),
            TargetUserGroupsJson = JsonSerializer.Serialize(groups),
            StartedAt           = now,
            ExpiresAt           = now.Add(sessionDuration),
        };

        _context.ImpersonationSessions.Add(session);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "ImpersonationSession", session.Id.ToString(), "Start",
            null, $"{{\"adminId\":\"{adminUserId}\",\"targetId\":\"{targetUser.OktaId}\"}}",
            adminUserId, adminEmail, null);

        _logger.LogInformation(
            "Impersonation started: admin {Admin} → target {Target}, session {Session}, expires {Expires}",
            adminEmail, targetUser.Email, session.Id, session.ExpiresAt);

        return MapToDto(session, groups);
    }

    public async Task EndAsync(Guid sessionId, string endReason = "Manual")
    {
        var session = await _context.ImpersonationSessions.FindAsync(sessionId);
        if (session == null) return;

        session.EndedAt   = DateTime.UtcNow;
        session.EndReason = endReason;
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "ImpersonationSession", session.Id.ToString(), "End",
            null, $"{{\"endReason\":\"{endReason}\"}}",
            session.AdminUserId, session.AdminEmail);

        _logger.LogInformation(
            "Impersonation ended: admin {Admin} → target {Target}, reason {Reason}",
            session.AdminEmail, session.TargetUserEmail, endReason);
    }

    public async Task<ImpersonationSessionDto?> GetActiveSessionAsync(string adminUserId)
    {
        var session = await _context.ImpersonationSessions
            .FirstOrDefaultAsync(s => s.AdminUserId == adminUserId
                                   && s.EndedAt == null
                                   && s.ExpiresAt > DateTime.UtcNow);
        if (session == null) return null;

        var groups = JsonSerializer.Deserialize<List<string>>(session.TargetUserGroupsJson) ?? new List<string>();
        return MapToDto(session, groups);
    }

    public async Task<ImpersonationSession?> ValidateSessionAsync(Guid sessionId)
    {
        var session = await _context.ImpersonationSessions.FindAsync(sessionId);
        if (session == null) return null;
        if (session.EndedAt != null || session.ExpiresAt <= DateTime.UtcNow) return null;
        return session;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TimeSpan GetConfiguredDuration()
    {
        var minutes = _config.GetValue<int?>("Impersonation:DurationMinutes") ?? 60;
        return TimeSpan.FromMinutes(minutes);
    }

    private static ImpersonationSessionDto MapToDto(ImpersonationSession session, List<string> groups) => new()
    {
        SessionId  = session.Id,
        TargetUser = new TargetUserDto
        {
            Id     = session.TargetUserId,
            Name   = session.TargetUserName,
            Email  = session.TargetUserEmail,
            Groups = groups,
        },
        StartedAt = session.StartedAt,
        ExpiresAt = session.ExpiresAt,
        IsActive  = session.EndedAt == null && session.ExpiresAt > DateTime.UtcNow,
    };
}
