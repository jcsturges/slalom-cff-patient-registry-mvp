using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// System announcements for users.
/// </summary>
[Table("Announcements", Schema = "ngr")]
public class Announcement
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT

    [Required]
    [MaxLength(50)]
    public string TargetAudience { get; set; } = "ALL"; // ALL, FOUNDATION, PROGRAMS

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    [MaxLength(255)]
    public string? UpdatedBy { get; set; }
}
