using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Lookup table for form submission statuses.
/// </summary>
[Table("FormStatuses", Schema = "ngr")]
public class FormStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}
