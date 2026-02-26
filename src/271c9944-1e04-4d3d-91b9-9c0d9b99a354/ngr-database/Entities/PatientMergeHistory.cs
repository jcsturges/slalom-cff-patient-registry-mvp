using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Tracks patient merge operations for audit purposes.
/// </summary>
[Table("PatientMergeHistory", Schema = "ngr")]
public class PatientMergeHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SurvivorPatientId { get; set; }

    /// <summary>
    /// Original patient ID before merge
    /// </summary>
    [Required]
    public int MergedPatientId { get; set; }

    [Required]
    [MaxLength(20)]
    public string MergedRegistryId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MergeReason { get; set; }

    public DateTime MergedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(255)]
    public string MergedBy { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(SurvivorPatientId))]
    public virtual Patient SurvivorPatient { get; set; } = null!;
}
