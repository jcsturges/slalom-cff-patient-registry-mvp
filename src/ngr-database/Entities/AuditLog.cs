using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Comprehensive audit log for all system operations.
/// </summary>
[Table("AuditLogs", Schema = "ngr")]
public class AuditLog
{
    [Key]
    public long Id { get; set; }

    public int? UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // CREATE, READ, UPDATE, DELETE, LOGIN, LOGOUT, EXPORT

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityId { get; set; }

    public int? ProgramId { get; set; }

    /// <summary>
    /// Old values before change (JSON)
    /// </summary>
    public string? OldValuesJson { get; set; }

    /// <summary>
    /// New values after change (JSON)
    /// </summary>
    public string? NewValuesJson { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(500)]
    public string? RequestPath { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(ProgramId))]
    public virtual CareProgram? CareProgram { get; set; }
}
