namespace NgrApi.Models;

/// <summary>
/// Represents a patient in the registry
/// </summary>
public class Patient
{
    public int Id { get; set; }
    public string RegistryId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string Status { get; set; } = "Active";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsDeceased { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public int CareProgramId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public CareProgram CareProgram { get; set; } = null!;
    public PatientDemographics? Demographics { get; set; }
    public ICollection<PatientProgramAssignment> ProgramAssignments { get; set; } = new List<PatientProgramAssignment>();
    public ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
}

/// <summary>
/// Patient demographic information
/// </summary>
public class PatientDemographics
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Ethnicity { get; set; }
    public string? Race { get; set; }
    public string? InsuranceType { get; set; }
    public string? InsuranceProvider { get; set; }

    // Navigation property
    public Patient Patient { get; set; } = null!;
}

/// <summary>
/// Patient assignment to a care program
/// </summary>
public class PatientProgramAssignment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }
    public string? LocalMRN { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public bool IsPrimaryProgram { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? DisenrollmentDate { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public CareProgram Program { get; set; } = null!;
}
