using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Tracks data warehouse synchronization status.
/// </summary>
[Table("DataWarehouseSyncStatus", Schema = "ngr")]
public class DataWarehouseSyncStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string SyncType { get; set; } = string.Empty; // FULL, INCREMENTAL

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public DateTime LastSyncAt { get; set; }

    /// <summary>
    /// Last processed ID for incremental sync
    /// </summary>
    public long? LastSyncId { get; set; }

    [Required]
    public int RecordsProcessed { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // SUCCESS, FAILED, PARTIAL

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int? Duration { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
