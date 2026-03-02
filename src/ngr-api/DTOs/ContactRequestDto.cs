namespace NgrApi.DTOs;

public class ContactRequestDto
{
    public int Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProgramNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentFileName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateContactRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
