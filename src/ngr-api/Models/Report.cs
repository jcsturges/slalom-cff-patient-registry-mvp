namespace NgrApi.Models;

/// <summary>
/// Saved report query definition (SRS Section 7.3.4)
/// </summary>
public class SavedReport
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Scope: "my", "program", "global"</summary>
    public string Scope { get; set; } = "my";

    /// <summary>JSON query definition (selection criteria, columns, filters)</summary>
    public string QueryDefinitionJson { get; set; } = "{}";

    /// <summary>Report type: "custom", "incomplete_records", "due_visit", "diabetes_testing", etc.</summary>
    public string ReportType { get; set; } = "custom";

    public int? OwnerUserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public int? ProgramId { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
}

/// <summary>
/// Report execution result (cached for re-download). Captures full usage metadata per 12-003 AC.
/// </summary>
public class ReportExecution
{
    public int Id { get; set; }
    public int? SavedReportId { get; set; }

    /// <summary>For pre-defined reports: type identifier</summary>
    public string ReportType { get; set; } = string.Empty;

    public string ExecutedBy { get; set; } = string.Empty;
    public int? ProgramId { get; set; }

    /// <summary>JSON result data</summary>
    public string ResultDataJson { get; set; } = "[]";

    public int RecordCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public DateTime ExecutedAt { get; set; }

    // ── 12-003: Report Usage Tracking ─────────────────────────────────────────

    /// <summary>JSON of filter/parameter values used during execution (no PHI — field names only)</summary>
    public string? ParametersJson { get; set; }

    /// <summary>"screen", "export", "download"</summary>
    public string OutputMode { get; set; } = "screen";

    /// <summary>File format when exported: "csv", "xlsx", "pdf"</summary>
    public string? FileFormat { get; set; }

    /// <summary>Size of exported file in bytes (null for on-screen)</summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>"Success" or "Failed"</summary>
    public string Status { get; set; } = "Success";

    /// <summary>Error message on failure (no PHI)</summary>
    public string? ErrorMessage { get; set; }

    // Navigation
    public SavedReport? SavedReport { get; set; }
}

/// <summary>
/// Log entry for report/file downloads (SRS Section 7.2.3)
/// </summary>
public class ReportDownloadLog
{
    public int Id { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public int? ProgramId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserRole { get; set; }
    public string? FieldsIncluded { get; set; }
    public int PatientCount { get; set; }
    public string Format { get; set; } = "csv";
    public DateTime DownloadedAt { get; set; }
}
