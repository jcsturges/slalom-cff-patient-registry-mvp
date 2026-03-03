namespace NgrApi.DTOs;

public class FormSubmissionDto
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string FormCode { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public int PatientId { get; set; }
    public int? EncounterId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string CompletionStatus { get; set; } = string.Empty;
    public string LockStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LastUpdateSource { get; set; } = string.Empty;
    public bool RequiresReview { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Type-specific metadata
    public string? EncounterDate { get; set; }
    public int? AnnualReviewYear { get; set; }
    public string? TransplantOrgan { get; set; }
    public string? CareEpisodeStartDate { get; set; }
    public string? CareEpisodeEndDate { get; set; }
    public string? PhoneNoteDate { get; set; }
    public string? LabDate { get; set; }
}

public class CreateFormSubmissionDto
{
    public int FormDefinitionId { get; set; }
    public int PatientId { get; set; }
    public int? EncounterId { get; set; }
    public int ProgramId { get; set; }
    public string? FormDataJson { get; set; }
}

public class UpdateFormDataDto
{
    public string FormDataJson { get; set; } = string.Empty;
    /// <summary>If true, attempt to mark the form as Complete</summary>
    public bool MarkComplete { get; set; }
}

/// <summary>Dashboard response with all form tables for a patient</summary>
public class PatientDashboardDto
{
    public PatientDto Patient { get; set; } = null!;
    public List<FormSubmissionDto> SharedForms { get; set; } = new();
    public List<FormSubmissionDto> Transplants { get; set; } = new();
    public List<FormSubmissionDto> AnnualReviews { get; set; } = new();
    public List<FormSubmissionDto> Encounters { get; set; } = new();
    public List<FormSubmissionDto> Labs { get; set; } = new();
    public List<FormSubmissionDto> CareEpisodes { get; set; } = new();
    public List<FormSubmissionDto> PhoneNotes { get; set; } = new();
    public List<FormSubmissionDto> AldStatus { get; set; } = new();
    public List<PatientFileDto> Files { get; set; } = new();
}

/// <summary>Validation result from the 4-tier validation engine</summary>
public class FormValidationResultDto
{
    public bool IsValid { get; set; }
    public bool CanSave { get; set; }
    public bool CanComplete { get; set; }
    public List<ValidationMessageDto> Messages { get; set; } = new();
}

public class ValidationMessageDto
{
    /// <summary>Warning, CompletionBlocking, SaveBlocking, DependencyChange</summary>
    public string Severity { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? CorrectiveAction { get; set; }
}

/// <summary>Database lock request</summary>
public class DatabaseLockRequestDto
{
    public int ReportingYear { get; set; }
    /// <summary>"synchronous" or "scheduled"</summary>
    public string Mode { get; set; } = "synchronous";
    /// <summary>For scheduled mode: when to execute (ISO datetime)</summary>
    public DateTime? ScheduledAt { get; set; }
}

public class DatabaseLockResultDto
{
    public int ReportingYear { get; set; }
    public int FormsLocked { get; set; }
    public int FormsSkipped { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Form definition with schema for rendering</summary>
public class FormDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FormType { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool AutoComplete { get; set; }
    public string SchemaJson { get; set; } = string.Empty;
    public string? ValidationRulesJson { get; set; }
    public string? UiSchemaJson { get; set; }
}
