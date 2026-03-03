namespace NgrApi.DTOs;

public class SavedReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public int? ProgramId { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
}

public class CreateSavedReportDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Scope { get; set; } = "my";
    public string QueryDefinitionJson { get; set; } = "{}";
    public int? ProgramId { get; set; }
}

public class UpdateSavedReportDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? QueryDefinitionJson { get; set; }
}

public class ExecuteReportDto
{
    /// <summary>Saved report ID (for custom reports) or report type (for pre-defined)</summary>
    public int? SavedReportId { get; set; }
    public string? ReportType { get; set; }
    public int? ProgramId { get; set; }
    /// <summary>Override query definition JSON (for ad-hoc execution)</summary>
    public string? QueryDefinitionJson { get; set; }
    public int? ReportingYear { get; set; }
}

public class ReportResultDto
{
    public int ExecutionId { get; set; }
    public string ReportTitle { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string ExecutedBy { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public int RecordCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public string? QuerySummary { get; set; }
}

public class ReportDownloadRequestDto
{
    public int ExecutionId { get; set; }
    /// <summary>"csv" or "excel"</summary>
    public string Format { get; set; } = "csv";
}
