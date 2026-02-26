using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Master patient index for the NGR system.
/// </summary>
[Table("Patients", Schema = "ngr")]
public class Patient
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// NGR-assigned unique identifier (e.g., NGR-000001)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string RegistryId { get; set; } = string.Empty;

    /// <summary>
    /// Medical Record Number (may vary by program)
    /// </summary>
    [MaxLength(50)]
    public string? MRN { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    /// <summary>
    /// Last 4 digits of SSN only (encrypted)
    /// </summary>
    [MaxLength(4)]
    public string? SSNLast4 { get; set; }

    public bool IsDeceased { get; set; } = false;

    public DateTime? DeceasedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual PatientDemographics? Demographics { get; set; }
    public virtual ICollection<PatientProgram> PatientPrograms { get; set; } = new List<PatientProgram>();
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    public virtual ICollection<PatientMergeHistory> MergeHistoriesAsSurvivor { get; set; } = new List<PatientMergeHistory>();
}
