using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Extended demographic information for patients.
/// </summary>
[Table("PatientDemographics", Schema = "ngr")]
public class PatientDemographics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [MaxLength(200)]
    public string? Address1 { get; set; }

    [MaxLength(200)]
    public string? Address2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    [MaxLength(10)]
    public string? ZipCode { get; set; }

    [MaxLength(50)]
    public string Country { get; set; } = "USA";

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(50)]
    public string? EmergencyContactRelation { get; set; }

    [MaxLength(50)]
    public string? PrimaryLanguage { get; set; }

    [MaxLength(50)]
    public string? Ethnicity { get; set; }

    [MaxLength(100)]
    public string? Race { get; set; }

    [MaxLength(50)]
    public string? InsuranceType { get; set; }

    [MaxLength(200)]
    public string? InsuranceProvider { get; set; }

    [MaxLength(100)]
    public string? InsurancePolicyNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; } = null!;
}
