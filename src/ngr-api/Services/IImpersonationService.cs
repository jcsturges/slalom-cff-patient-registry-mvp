using NgrApi.DTOs;

namespace NgrApi.Services;

public interface IImpersonationService
{
    /// <summary>Start an impersonation session. Returns the session DTO.</summary>
    Task<ImpersonationSessionDto> StartAsync(
        string adminUserId, string adminEmail,
        string targetUserId, TimeSpan? duration = null);

    /// <summary>End an impersonation session by session ID.</summary>
    Task EndAsync(Guid sessionId, string endReason = "Manual");

    /// <summary>Get the current active session for an admin user (null if none).</summary>
    Task<ImpersonationSessionDto?> GetActiveSessionAsync(string adminUserId);

    /// <summary>Validate a session by ID — returns null if expired/ended.</summary>
    Task<NgrApi.Models.ImpersonationSession?> ValidateSessionAsync(Guid sessionId);
}
