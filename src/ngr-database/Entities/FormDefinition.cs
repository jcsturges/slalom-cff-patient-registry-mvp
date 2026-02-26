using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Defines an eCRF template with JSON schema.
/// </summary>
[Table("FormDefinitions", Schema = "ngr")]
public class FormDefinition
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int Version { get; set; } = 1;

    /// <summary>
    /// JSON Schema for form fields
    /// </summary>
    [Required]
    public string SchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON validation rules
    /// </summary>
    public string? ValidationRulesJson { get; set; }

    /// <summary>
    /// JSON UI rendering hints
    /// </summary>
    public string? UISchemaJson { get; set; }

    public int? EncounterTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EncounterTypeId))]
    public virtual EncounterType? EncounterType { get; set; }

    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}
