namespace NgrApi.Models;

// ── Database Lock ─────────────────────────────────────────────────────────────

/// <summary>Tracks annual database lock operations (one per reporting year).</summary>
public class DatabaseLock
{
    public int Id { get; set; }

    /// <summary>The registry reporting year being locked (e.g. 2024)</summary>
    public int ReportingYear { get; set; }

    /// <summary>The cutoff date: forms with service dates on or before this date in ReportingYear are locked</summary>
    public DateTime LockDate { get; set; }

    /// <summary>"Synchronous" | "Batch"</summary>
    public string ExecutionMode { get; set; } = "Synchronous";

    /// <summary>When the batch is scheduled to run (Batch mode only)</summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>"Pending" | "InProgress" | "Completed" | "Failed"</summary>
    public string Status { get; set; } = "Pending";

    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>Number of forms successfully locked</summary>
    public int FormsLocked { get; set; }

    /// <summary>Number of forms skipped due to active editing sessions</summary>
    public int FormsSkipped { get; set; }

    /// <summary>Total forms eligible for locking (for batch progress)</summary>
    public int? ProgressFormsTotal { get; set; }

    /// <summary>Forms processed so far (for batch progress)</summary>
    public int ProgressFormsProcessed { get; set; }

    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    // Navigation
    public ICollection<DatabaseLockSkippedForm> SkippedForms { get; set; } = new List<DatabaseLockSkippedForm>();
}

/// <summary>Records form submissions that were skipped during a lock operation due to active sessions.</summary>
public class DatabaseLockSkippedForm
{
    public int Id { get; set; }
    public int DatabaseLockId { get; set; }
    public int FormSubmissionId { get; set; }

    /// <summary>The user who had the form open at lock time</summary>
    public string SessionUserId { get; set; } = string.Empty;

    public string SkipReason { get; set; } = string.Empty;
    public DateTime SkippedAt { get; set; }

    /// <summary>When this skipped form was finally resolved (locked)</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>"AutoLockedOnSave" | "ForceLockedByReconciliation" | null</summary>
    public string? ResolutionType { get; set; }

    // Navigation
    public DatabaseLock DatabaseLock { get; set; } = null!;
    public FormSubmission FormSubmission { get; set; } = null!;
}

// ── Impersonation ─────────────────────────────────────────────────────────────

/// <summary>Tracks active and historical Foundation Admin impersonation sessions.</summary>
public class ImpersonationSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The Foundation Admin performing the impersonation</summary>
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>The user being impersonated</summary>
    public string TargetUserId { get; set; } = string.Empty;
    public string TargetUserEmail { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;

    /// <summary>JSON-serialized groups/roles of the target user at session start</summary>
    public string TargetUserGroupsJson { get; set; } = "[]";

    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    /// <summary>When the session was ended (null if still active or expired)</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>"Manual" | "Expired" | "BrowserClosed"</summary>
    public string? EndReason { get; set; }
}
