using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Represents a submitted eCRF with JSON data.
/// </summary>
[Table("FormSubmissions", Schema = "ngr")]
public class FormSubmission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FormDefinitionId { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int? EncounterId { get; set; }

    [Required]
    public int ProgramId { get; set; }

    [Required]
    public int StatusId { get; set; }

    /// <summary>
    /// JSON form data
    /// </summary>
    [Required]
    public string FormDataJson { get; set; } = string.Empty;

    public DateTime? SubmittedAt { get; set; }

    [MaxLength(255)]
    public string? SubmittedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    [MaxLength(255)]
    public string? LockedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FormDefinitionId))]
    public virtual FormDefinition FormDefinition { get; set; } = null!;

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey(nameof(EncounterId))]
    public virtual Encounter? Encounter { get; set; }

    [ForeignKey(nameof(ProgramId))]
    public virtual CareProgram CareProgram { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    public virtual FormStatus FormStatus { get; set; } = null!;

    public virtual ICollection<FormFieldHistory> FieldHistory { get; set; } = new List<FormFieldHistory>();
}
