namespace NgrApi.Models;

/// <summary>
/// Lightweight user interaction event for usability analytics (12-004).
/// No PHI — only opaque user/session IDs, page paths, and component names.
/// </summary>
public class UserEvent
{
    public int Id { get; set; }

    /// <summary>Opaque user identifier (Okta sub / OktaId)</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Browser session identifier (generated on login, stored in sessionStorage)</summary>
    public string? SessionId { get; set; }

    /// <summary>"page_view", "button_click", "form_submit", "login", "logout", "time_on_page"</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Route/path of the page (e.g. "/patients", "/reports/builder")</summary>
    public string? Page { get; set; }

    /// <summary>UI component that fired the event (e.g. "ImpersonationBanner", "PatientListPage")</summary>
    public string? Component { get; set; }

    /// <summary>JSON properties bag (no PHI — keys like "reportType", "durationMs")</summary>
    public string? PropertiesJson { get; set; }

    public DateTime OccurredAt { get; set; }
}

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
