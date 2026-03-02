namespace NgrApi.Models;

/// <summary>
/// Represents a CMS content item (help article, documentation page)
/// </summary>
public class Content
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Represents a system-wide announcement
/// </summary>
public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string TargetAudience { get; set; } = "All";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Represents a help page in the hierarchical help system
/// </summary>
public class HelpPage
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public string? ContextKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public HelpPage? Parent { get; set; }
    public ICollection<HelpPage> Children { get; set; } = new List<HelpPage>();
}

/// <summary>
/// Represents a contact/support request submitted by a user
/// </summary>
public class ContactRequest
{
    public int Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProgramNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentFileName { get; set; }
    public string? AttachmentBlobPath { get; set; }
    public string Status { get; set; } = "New";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents a CSV import job
/// </summary>
public class ImportJob
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? BlobPath { get; set; }
    public int ProgramId { get; set; }
    public int? FormDefinitionId { get; set; }
    public int? TotalRows { get; set; }
    public int? ProcessedRows { get; set; }
    public int? ErrorRows { get; set; }
    public string? MappingJson { get; set; }
    public string? ResultsJson { get; set; }
    public string? ErrorsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public CareProgram Program { get; set; } = null!;
    public FormDefinition? FormDefinition { get; set; }
}
