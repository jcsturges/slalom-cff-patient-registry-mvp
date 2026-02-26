namespace NgrApi.Models;

/// <summary>
/// Represents a user's role assignment to a care program
/// </summary>
public class UserProgramRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProgramId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public CareProgram Program { get; set; } = null!;
}
