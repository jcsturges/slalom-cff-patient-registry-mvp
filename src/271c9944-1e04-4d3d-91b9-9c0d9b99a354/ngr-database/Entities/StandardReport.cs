using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Pre-built standard reports.
/// </summary>
[Table("StandardReports", Schema = "ngr")]
public class StandardReport
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
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Report query definition (JSON)
    /// </summary>
    [Required]
    public string QueryDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Report columns (JSON)
    /// </summary>
    [Required]
    public string ColumnsJson { get; set; } = string.Empty;

    /// <summary>
    /// Available filters (JSON)
    /// </summary>
    public string? AvailableFiltersJson { get; set; }

    [Required]
    [MaxLength(50)]
    public string RequiredRole { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
