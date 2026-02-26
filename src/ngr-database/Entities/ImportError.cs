using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Tracks errors during CSV import processing.
/// </summary>
[Table("ImportErrors", Schema = "ngr")]
public class ImportError
{
    [Key]
    public long Id { get; set; }

    [Required]
    public int ImportJobId { get; set; }

    [Required]
    public int RowNumber { get; set; }

    [MaxLength(100)]
    public string? FieldName { get; set; }

    [Required]
    [MaxLength(50)]
    public string ErrorType { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ErrorMessage { get; set; } = string.Empty;

    public string? RawData { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ImportJobId))]
    public virtual ImportJob ImportJob { get; set; } = null!;
}
