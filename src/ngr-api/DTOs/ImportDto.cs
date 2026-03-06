namespace NgrApi.DTOs;

// ── Import Job ────────────────────────────────────────────────────────────────

public class ImportJobDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public int? TotalRows { get; set; }
    public int? ProcessedRows { get; set; }
    public int? ErrorRows { get; set; }
    public int? WarningCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}

public class ImportJobDetailDto : ImportJobDto
{
    public List<EmrImportIssue> Errors { get; set; } = new();
    public List<EmrImportIssue> Warnings { get; set; } = new();
}

// ── Validation / Error reporting ──────────────────────────────────────────────

/// <summary>A single error or warning from CSV validation or processing</summary>
public class EmrImportIssue
{
    /// <summary>"Error" | "Warning"</summary>
    public string Severity { get; set; } = "Error";

    /// <summary>Row number in the CSV (1-based, excluding header); null = file-level</summary>
    public int? RowNumber { get; set; }

    /// <summary>CSV column name the issue relates to; null = row/file level</summary>
    public string? CsvColumn { get; set; }

    /// <summary>Registry form field path (if mapping was found)</summary>
    public string? FieldPath { get; set; }

    public string Message { get; set; } = string.Empty;
}

public class EmrValidationResult
{
    public bool IsValid { get; set; }
    public int RowCount { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<EmrImportIssue> Errors { get; set; } = new();
    public List<EmrImportIssue> Warnings { get; set; } = new();
}

// ── SFTP Config ───────────────────────────────────────────────────────────────

public class SftpConfigDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string RemoteDirectory { get; set; } = "/";
    public string FilePattern { get; set; } = "*.csv";
    public string ScheduleCron { get; set; } = "0 2 * * *";
    public bool IsEnabled { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
}

public class UpsertSftpConfigDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;

    /// <summary>Provide to change password; omit to keep existing</summary>
    public string? Password { get; set; }

    public string RemoteDirectory { get; set; } = "/";
    public string FilePattern { get; set; } = "*.csv";
    public string ScheduleCron { get; set; } = "0 2 * * *";
    public bool IsEnabled { get; set; }
}

// ── Field Mapping ─────────────────────────────────────────────────────────────

public class EmrFieldMappingDto
{
    public int Id { get; set; }
    public int? ProgramId { get; set; }
    public string CsvColumnName { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public string FieldPath { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public string? TransformHint { get; set; }
    public bool IsActive { get; set; }
}
