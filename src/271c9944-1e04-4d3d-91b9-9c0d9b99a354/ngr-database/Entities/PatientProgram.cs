using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Patient-program assignment (roster).
/// </summary>
[Table("PatientPrograms", Schema = "ngr")]
public class PatientProgram
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProgramId { get; set; }

    /// <summary>
    /// Program-specific MRN
    /// </summary>
    [MaxLength(50)]
    public string? LocalMRN { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, TRANSFERRED, REMOVED

    public bool IsPrimaryProgram { get; set; } = false;

    [Required]
    public DateTime EnrollmentDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    [MaxLength(255)]
    public string? DischargeReason { get; set; }

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
}
