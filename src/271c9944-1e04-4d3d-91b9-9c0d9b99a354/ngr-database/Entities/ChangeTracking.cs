using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Change tracking for incremental data warehouse sync.
/// </summary>
[Table("ChangeTracking", Schema = "ngr")]
public class ChangeTracking
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public int EntityId { get; set; }

    [Required]
    [MaxLength(10)]
    public string ChangeType { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed { get; set; } = false;
}
