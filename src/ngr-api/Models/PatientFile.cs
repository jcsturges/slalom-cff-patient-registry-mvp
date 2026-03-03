namespace NgrApi.Models;

/// <summary>
/// Represents a file/document attached to a patient record (SRS Section 6.7)
/// </summary>
public class PatientFile
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }

    /// <summary>Original file name as uploaded</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Renamed file name per naming convention</summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>Azure Blob Storage path</summary>
    public string BlobPath { get; set; } = string.Empty;

    /// <summary>MIME content type</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>File extension (.pdf, .jpg, etc.)</summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>File size in bytes</summary>
    public long FileSize { get; set; }

    /// <summary>User-provided description (≤1000 chars)</summary>
    public string? Description { get; set; }

    /// <summary>Informed Consent, Genotype Results, Sweat Test Results, Lab Results, Other</summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>Free-text description when FileType is "Other"</summary>
    public string? OtherFileTypeDescription { get; set; }

    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public CareProgram Program { get; set; } = null!;
}
