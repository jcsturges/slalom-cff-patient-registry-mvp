using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// User-created saved reports.
/// </summary>
[Table("SavedReports", Schema = "ngr")]
public class SavedReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// NULL for Foundation-wide reports
    /// </summary>
    public int? ProgramId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Report query configuration (JSON)
    /// </summary>
    [Required]
    public string QueryDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Selected columns (JSON)
    /// </summary>
    [Required]
    public string ColumnsJson { get; set; } = string.Empty;

    /// <summary>
    /// Applied filters (JSON)
    /// </summary>
    public string? FiltersJson { get; set; }

    /// <summary>
    /// Sort configuration (JSON)
    /// </summary>
    public string? SortJson { get; set; }

    public bool IsShared { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(ProgramId))]
    public virtual CareProgram? CareProgram { get; set; }
}
