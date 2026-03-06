namespace NgrApi.DTOs;

public class StartImpersonationDto
{
    public string TargetUserId { get; set; } = string.Empty;
}

public class EndImpersonationDto
{
    public Guid SessionId { get; set; }
    public string? EndReason { get; set; } = "Manual";
}

public class ImpersonationSessionDto
{
    public Guid SessionId { get; set; }
    public TargetUserDto TargetUser { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class TargetUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
}
