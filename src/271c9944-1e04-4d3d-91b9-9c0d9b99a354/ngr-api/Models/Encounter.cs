namespace NgrApi.Models;

/// <summary>
/// Represents a patient encounter
/// </summary>
public class Encounter
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }
    public string EncounterTypeCode { get; set; } = string.Empty;
    public string EncounterTypeName { get; set; } = string.Empty;
    public DateTime EncounterDate { get; set; }
    public int EncounterYear { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public CareProgram Program { get; set; } = null!;
    public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}
