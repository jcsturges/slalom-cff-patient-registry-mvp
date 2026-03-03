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
/// Report execution result (cached for re-download)
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
