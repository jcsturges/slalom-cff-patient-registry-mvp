namespace NgrApi.Models;

/// <summary>
/// Maps a CSV column from an EMR export to a Registry form field (SRS Section 9).
/// ProgramId = null means this is a global default mapping; program-level records
/// override the default for that program.
/// </summary>
public class EmrFieldMapping
{
    public int Id { get; set; }

    /// <summary>null = global default; set to override per program</summary>
    public int? ProgramId { get; set; }

    /// <summary>Exact CSV header name as it appears in the EMR export (e.g. "PT_DOB")</summary>
    public string CsvColumnName { get; set; } = string.Empty;

    /// <summary>One of FormTypes constants (e.g. "DEMOGRAPHICS", "ENCOUNTER", "LABS_TESTS")</summary>
    public string FormType { get; set; } = string.Empty;

    /// <summary>JSON field path inside FormDataJson (e.g. "dateOfBirth", "weight_kg")</summary>
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>"string" | "date" | "boolean" | "integer" | "decimal"</summary>
    public string DataType { get; set; } = "string";

    /// <summary>Whether a non-blank value is required for a row to be considered valid</summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Optional lightweight transform hint applied before writing the value:
    /// "uppercase", "lowercase", "date:yyyy-MM-dd", "boolean:Y/N"
    /// </summary>
    public string? TransformHint { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public CareProgram? Program { get; set; }
}

/// <summary>
/// Crosswalk between an institution's MRN and the CFF Registry patient record.
/// Each care program maintains its own crosswalk (SRS Section 9).
/// </summary>
public class InstitutionMrnCrosswalk
{
    public int Id { get; set; }
    public int ProgramId { get; set; }

    /// <summary>The EMR Medical Record Number used by the institution</summary>
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public long CffId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public CareProgram Program { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

/// <summary>
/// Per-program SFTP connection configuration for automated EMR transfers (SRS Section 9 / Story 10-002).
/// Actual SFTP server infrastructure is provisioned separately; this model holds the config.
/// </summary>
public class SftpConfig
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;

    /// <summary>Encrypted at rest via Azure Key Vault reference or AES envelope</summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>Remote directory path where the EMR system deposits CSV files</summary>
    public string RemoteDirectory { get; set; } = "/";

    /// <summary>Glob pattern for files to pick up (e.g. "*.csv")</summary>
    public string FilePattern { get; set; } = "*.csv";

    /// <summary>Cron expression for scheduled polling (e.g. "0 2 * * *" = 02:00 UTC daily)</summary>
    public string ScheduleCron { get; set; } = "0 2 * * *";

    public bool IsEnabled { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation property
    public CareProgram Program { get; set; } = null!;
}
