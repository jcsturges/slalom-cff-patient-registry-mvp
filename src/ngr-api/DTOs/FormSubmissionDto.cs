namespace NgrApi.DTOs;

public class FormSubmissionDto
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string FormCode { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? EncounterId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Type-specific metadata (populated based on form type)
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
