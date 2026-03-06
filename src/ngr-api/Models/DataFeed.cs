namespace NgrApi.Models;

// ── Outbound Data Feed ────────────────────────────────────────────────────────

/// <summary>
/// Tracks each execution of the nightly delta feed or a full resync (SRS 10.1).
/// One row per run. Status transitions: Pending → Running → Completed | Failed.
/// </summary>
public class FeedRun
{
    public int Id { get; set; }

    /// <summary>"Delta" — only records changed since last successful run.
    /// "Full"  — all current records + all tombstones within retention window.</summary>
    public string RunType { get; set; } = "Delta";

    /// <summary>"Pending", "Running", "Completed", "Failed"</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Inclusive start of the extraction window (null = beginning of time for Full runs)</summary>
    public DateTime? WindowStart { get; set; }

    /// <summary>Inclusive end of the extraction window</summary>
    public DateTime? WindowEnd { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>Total records included across all entity types</summary>
    public int TotalRecords { get; set; }

    public int ErrorCount { get; set; }

    /// <summary>Machine-readable reconciliation JSON (13-004): counts by entity + operation</summary>
    public string? ReconciliationJson { get; set; }

    /// <summary>Azure Blob path where the feed file was written (13-005)</summary>
    public string? BlobPath { get; set; }

    /// <summary>User or scheduler that triggered this run</summary>
    public string TriggeredBy { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Config-driven mapping from NGR entity properties to CFF Data Warehouse column names (13-001).
/// Versioned — a new row with Version+1 supersedes the previous one for the same NgrEntity+NgrProperty.
/// </summary>
public class FeedFieldMapping
{
    public int Id { get; set; }

    /// <summary>NGR entity name (e.g. "Patient", "FormSubmission")</summary>
    public string NgrEntity { get; set; } = string.Empty;

    /// <summary>Property name on the entity (e.g. "CffId", "DateOfBirth")</summary>
    public string NgrProperty { get; set; } = string.Empty;

    /// <summary>Corresponding column name in CFF Data Warehouse (e.g. "PT_CFF_ID", "PT_DOB")</summary>
    public string CffColumnName { get; set; } = string.Empty;

    /// <summary>"string" | "date" | "boolean" | "integer" | "decimal"</summary>
    public string DataType { get; set; } = "string";

    /// <summary>Optional lightweight transform hint: "date:yyyy-MM-dd", "boolean:Y/N", etc.</summary>
    public string? TransformHint { get; set; }

    /// <summary>Sequential version for audit trail of mapping changes</summary>
    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Tombstone record retained for deleted or inactivated entities (13-002).
/// Retained for at least 90 days (configurable via DataFeed:TombstoneRetentionDays)
/// so the downstream Data Warehouse can process deletions on its own schedule.
/// </summary>
public class DeletionTombstone
{
    public int Id { get; set; }

    /// <summary>Entity type that was deleted (e.g. "Patient", "FormSubmission")</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>NGR database primary key (opaque integer)</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>CFF ID or other stable cross-system identifier (e.g. CFF patient number)</summary>
    public string? SourceSystemId { get; set; }

    /// <summary>"HardDelete", "Inactivated", "ConsentWithdrawn", "Merged"</summary>
    public string DeletedReason { get; set; } = string.Empty;

    public DateTime DeletedAt { get; set; }

    /// <summary>UTC datetime after which this record can be purged from the tombstone table</summary>
    public DateTime RetainUntil { get; set; }

    /// <summary>User or process that performed the deletion</summary>
    public string DeletedBy { get; set; } = string.Empty;
}

// ── Inbound Historical Migration ──────────────────────────────────────────────

/// <summary>
/// Tracks each execution of a migration phase (SRS 11). Re-runnable per phase.
/// </summary>
public class MigrationRun
{
    public int Id { get; set; }

    /// <summary>Phase being executed: "Demographics", "Diagnosis", "SweatTest", "ALD",
    /// "Transplant", "AnnualReview", "Encounter", "Labs", "CareEpisode", "PhoneNote", "Files"</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>"Pending", "Running", "Completed", "Failed", "PartialSuccess"</summary>
    public string Status { get; set; } = "Pending";

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>Number of records read from the source system</summary>
    public int SourceCount { get; set; }

    /// <summary>Number of records successfully written to the NGR database</summary>
    public int TargetCount { get; set; }

    public int ErrorCount { get; set; }

    /// <summary>JSON array of error messages (no PHI — entity IDs and error types only)</summary>
    public string? ErrorsJson { get; set; }

    /// <summary>JSON validation report generated by MigrationValidationService (13-008)</summary>
    public string? ValidationReportJson { get; set; }

    public string TriggeredBy { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
