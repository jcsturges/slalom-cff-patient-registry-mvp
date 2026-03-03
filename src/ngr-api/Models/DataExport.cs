namespace NgrApi.Models;

/// <summary>
/// Saved download definition for raw data export (SRS Section 8.2.1)
/// </summary>
public class SavedDownloadDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }

    /// <summary>JSON: selected form types, date range, completeness, diagnosis filter, format</summary>
    public string ParametersJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
}
