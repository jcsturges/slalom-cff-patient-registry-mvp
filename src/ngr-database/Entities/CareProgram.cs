using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Represents a CF care center/program (136 total).
/// </summary>
[Table("CarePrograms", Schema = "ngr")]
public class CareProgram
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

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

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? AccreditationDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<ProgramUser> ProgramUsers { get; set; } = new List<ProgramUser>();
    public virtual ICollection<PatientProgram> PatientPrograms { get; set; } = new List<PatientProgram>();
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    public virtual ICollection<ImportJob> ImportJobs { get; set; } = new List<ImportJob>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<SavedReport> SavedReports { get; set; } = new List<SavedReport>();
}
