namespace NgrApi.DTOs;

public class DataExportRequestDto
{
    /// <summary>Form type codes to export, or ["ALL"] for all forms</summary>
    public List<string> FormTypes { get; set; } = new();
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>"complete_only" or "all"</summary>
    public string CompletenessFilter { get; set; } = "all";
    /// <summary>"CF", "CFTR_related", "CRMS_CFSPID", or null for all</summary>
    public string? DiagnosisFilter { get; set; }
    /// <summary>"coded" or "descriptive"</summary>
    public string OutputFormat { get; set; } = "coded";
    public int ProgramId { get; set; }
}

public class SavedDownloadDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public string ParametersJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
}

public class CreateSavedDownloadDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProgramId { get; set; }
    public string ParametersJson { get; set; } = "{}";
}

public class UpdateSavedDownloadDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParametersJson { get; set; }
}
