using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Lookup table for encounter types (Annual, Quarterly, etc.)
/// </summary>
[Table("EncounterTypes", Schema = "ngr")]
public class EncounterType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public virtual ICollection<FormDefinition> FormDefinitions { get; set; } = new List<FormDefinition>();
}
