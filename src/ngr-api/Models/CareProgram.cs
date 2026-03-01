namespace NgrApi.Models;

/// <summary>
/// Represents a care program (care center).
/// Program ID is a manually-assigned, immutable, unique identifier.
/// </summary>
public class CareProgram
{
    public int Id { get; set; }

    /// <summary>
    /// Manually-assigned program identifier. Unique, immutable, never reused.
    /// </summary>
    public int ProgramId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Pediatric, Adult, Affiliate, Orphaned-Record Holding, Training
    /// </summary>
    public string ProgramType { get; set; } = "Adult";

    public string? City { get; set; }
    public string? State { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? AccreditationDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsOrphanHoldingProgram { get; set; }
    public bool IsTrainingProgram { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Calculated display title: "{ProgramId} - {Name} ({ProgramType})"
    /// </summary>
    public string DisplayTitle => $"{ProgramId} - {Name} ({ProgramType})";
}
