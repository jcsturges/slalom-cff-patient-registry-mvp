using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Represents a patient encounter (annual review, hospitalization, etc.)
/// </summary>
[Table("Encounters", Schema = "ngr")]
public class Encounter
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProgramId { get; set; }

    [Required]
    public int EncounterTypeId { get; set; }

    [Required]
    public DateTime EncounterDate { get; set; }

    /// <summary>
    /// Computed year for reporting purposes
    /// </summary>
    [Required]
    public int EncounterYear { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "OPEN"; // OPEN, COMPLETE, LOCKED

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey(nameof(ProgramId))]
    public virtual CareProgram CareProgram { get; set; } = null!;

    [ForeignKey(nameof(EncounterTypeId))]
    public virtual EncounterType EncounterType { get; set; } = null!;

    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}
