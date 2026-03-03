namespace NgrApi.Models;

/// <summary>
/// Form type constants for the 10 CRF types (SRS Section 6.1)
/// </summary>
public static class FormTypes
{
    // Shared forms (editable by any associated program)
    public const string Demographics = "DEMOGRAPHICS";
    public const string Diagnosis = "DIAGNOSIS";
    public const string SweatTest = "SWEAT_TEST";
    public const string Transplant = "TRANSPLANT";
    public const string AldInitiation = "ALD_INITIATION";

    // Program-specific forms (editable only by reporting program)
    public const string AnnualReview = "ANNUAL_REVIEW";
    public const string Encounter = "ENCOUNTER";
    public const string LabsAndTests = "LABS_TESTS";
    public const string CareEpisode = "CARE_EPISODE";
    public const string PhoneNote = "PHONE_NOTE";

    public static readonly string[] SharedFormTypes =
        [Demographics, Diagnosis, SweatTest, Transplant, AldInitiation];

    public static readonly string[] ProgramSpecificFormTypes =
        [AnnualReview, Encounter, LabsAndTests, CareEpisode, PhoneNote];

    public static readonly string[] AllFormTypes =
        [Demographics, Diagnosis, SweatTest, Transplant, AldInitiation,
         AnnualReview, Encounter, LabsAndTests, CareEpisode, PhoneNote];

    /// <summary>Forms that auto-complete when all required fields are filled</summary>
    public static readonly string[] AutoCompleteFormTypes =
        [Demographics, Diagnosis, SweatTest];

    /// <summary>Forms that require explicit user "Mark Complete" action</summary>
    public static readonly string[] UserCompleteFormTypes =
        [AldInitiation, Transplant, AnnualReview, CareEpisode, Encounter, LabsAndTests];

    public static bool IsShared(string formType) =>
        SharedFormTypes.Contains(formType);
}

/// <summary>
/// Represents a form template/definition (SRS Section 6.1)
/// </summary>
public class FormDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }

    /// <summary>One of FormTypes constants</summary>
    public string FormType { get; set; } = string.Empty;

    /// <summary>Whether this form type is shared across programs</summary>
    public bool IsShared { get; set; }

    /// <summary>Whether completion is automatic or user-specified</summary>
    public bool AutoComplete { get; set; }

    public string? EncounterTypeCode { get; set; }
    public string SchemaJson { get; set; } = string.Empty;
    public string? ValidationRulesJson { get; set; }
    public string? UiSchemaJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a submitted form instance (SRS Section 6.2)
/// </summary>
public class FormSubmission
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public int PatientId { get; set; }
    public int? EncounterId { get; set; }
    public int ProgramId { get; set; }
    public string FormDataJson { get; set; } = string.Empty;

    /// <summary>Completion status: Incomplete, Complete</summary>
    public string CompletionStatus { get; set; } = "Incomplete";

    /// <summary>Lock status: Unlocked, Locked</summary>
    public string LockStatus { get; set; } = "Unlocked";

    /// <summary>Combined status for backward compatibility</summary>
    public string Status { get; set; } = "Incomplete";

    /// <summary>Source of last update: "User", "EMR", "System"</summary>
    public string LastUpdateSource { get; set; } = "User";

    /// <summary>Whether user needs to review after EMR update</summary>
    public bool RequiresReview { get; set; }

    // Date-context fields for form-type-specific queries
    public DateTime? EncounterDate { get; set; }
    public int? AnnualReviewYear { get; set; }
    public string? TransplantOrgan { get; set; }
    public DateTime? CareEpisodeStartDate { get; set; }
    public DateTime? CareEpisodeEndDate { get; set; }
    public DateTime? PhoneNoteDate { get; set; }
    public DateTime? LabDate { get; set; }

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
