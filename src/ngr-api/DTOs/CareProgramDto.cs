namespace NgrApi.DTOs;

public class CareProgramDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? AccreditationDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsOrphanHoldingProgram { get; set; }
    public bool IsTrainingProgram { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCareProgramDto
{
    public int ProgramId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProgramType { get; set; } = "Adult";
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsTrainingProgram { get; set; }
}

public class UpdateCareProgramDto
{
    public string Name { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
