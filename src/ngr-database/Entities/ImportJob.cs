using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Tracks CSV import jobs.
/// </summary>
[Table("ImportJobs", Schema = "ngr")]
public class ImportJob
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(500)]
    public string BlobUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, PROCESSING, PREVIEW, CONFIRMED, FAILED

    public int? TotalRows { get; set; }

    public int? ProcessedRows { get; set; }

    public int? ErrorRows { get; set; }

    /// <summary>
    /// Field mapping configuration (JSON)
    /// </summary>
    public string? MappingJson { get; set; }

    /// <summary>
    /// Processing results (JSON)
    /// </summary>
    public string? ResultsJson { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(ProgramId))]
    public virtual CareProgram CareProgram { get; set; } = null!;

    public virtual ICollection<ImportError> ImportErrors { get; set; } = new List<ImportError>();
}
