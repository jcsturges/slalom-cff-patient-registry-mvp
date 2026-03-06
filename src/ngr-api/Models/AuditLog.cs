namespace NgrApi.Models;

/// <summary>
/// Represents an audit log entry
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }

    // ── Impersonation tracking ─────────────────────────────────────────────
    /// <summary>True when this action was performed during an impersonation session</summary>
    public bool IsImpersonated { get; set; }

    /// <summary>The Foundation Admin who was acting as the impersonated user (set when IsImpersonated = true)</summary>
    public string? ActingAdminId { get; set; }

    /// <summary>The impersonation session ID for cross-referencing</summary>
    public Guid? ImpersonationSessionId { get; set; }
}
