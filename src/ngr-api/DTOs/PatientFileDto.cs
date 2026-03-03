namespace NgrApi.DTOs;

public class PatientFileDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? OtherFileTypeDescription { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
}

public class UpdatePatientFileDto
{
    public string? Description { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? OtherFileTypeDescription { get; set; }
}

/// <summary>Admin bulk program association modification</summary>
public class BulkAssociationModifyDto
{
    public List<int> PatientIds { get; set; } = new();
    /// <summary>
    /// Action: "remove_all_consent", "remove_all_inactivity", "remove_all_withdrawal",
    /// "add_to_program", "transfer", "remove_from_program"
    /// </summary>
    public string Action { get; set; } = string.Empty;
    public int? TargetProgramId { get; set; }
    public int? SourceProgramId { get; set; }
    public string? Reason { get; set; }
}

public class BulkAssociationResultDto
{
    public int PatientsAffected { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

/// <summary>Hard-delete confirmation DTO</summary>
public class HardDeleteConfirmDto
{
    /// <summary>User must type the CFF ID to confirm</summary>
    public long ConfirmCffId { get; set; }
}
