namespace NgrApi.Models;

/// <summary>
/// Represents a form template/definition
/// </summary>
public class FormDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public string? EncounterTypeCode { get; set; }
    public string SchemaJson { get; set; } = string.Empty;
    public string? ValidationRulesJson { get; set; }
    public string? UiSchemaJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a submitted form response
/// </summary>
public class FormSubmission
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public int PatientId { get; set; }
    public int? EncounterId { get; set; }
    public int ProgramId { get; set; }
    public string FormDataJson { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public FormDefinition FormDefinition { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Encounter? Encounter { get; set; }
    public CareProgram Program { get; set; } = null!;
}
