using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ngr.Database.Entities;

/// <summary>
/// Audit trail for individual form field changes.
/// </summary>
[Table("FormFieldHistory", Schema = "ngr")]
public class FormFieldHistory
{
    [Key]
    public long Id { get; set; }

    [Required]
    public int FormSubmissionId { get; set; }

    /// <summary>
    /// JSON path to the field (e.g., "demographics.address.city")
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FieldPath { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(255)]
    public string ChangedBy { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(FormSubmissionId))]
    public virtual FormSubmission FormSubmission { get; set; } = null!;
}
