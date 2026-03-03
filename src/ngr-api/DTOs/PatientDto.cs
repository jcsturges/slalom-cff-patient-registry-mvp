namespace NgrApi.DTOs;

/// <summary>Read DTO for patient list/roster views</summary>
public class PatientDto
{
    public int Id { get; set; }
    public string RegistryId { get; set; } = string.Empty;
    public long CffId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? LastNameAtBirth { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BiologicalSexAtBirth { get; set; }
    public string? Gender { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string VitalStatus { get; set; } = "Alive";
    public int CareProgramId { get; set; }
    public string CareProgramName { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Names of other programs the patient belongs to (excluding current)</summary>
    public List<string> OtherPrograms { get; set; } = new();
}

/// <summary>Write DTO for creating a new patient via the identity form</summary>
public class CreatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? LastNameAtBirth { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BiologicalSexAtBirth { get; set; }
    public string? SsnLast4 { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? CareProgramId { get; set; }

    /// <summary>If provided, the system attempts to match an existing patient by Registry ID</summary>
    public string? KnownRegistryId { get; set; }
}

/// <summary>Write DTO for updating patient identity and metadata</summary>
public class UpdatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? LastNameAtBirth { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BiologicalSexAtBirth { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>DTO for patient-program association</summary>
public class PatientProgramAssociationDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? LocalMRN { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPrimaryProgram { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? DisenrollmentDate { get; set; }
    public string? RemovalReason { get; set; }
}

/// <summary>DTO for adding a patient to a program</summary>
public class AddPatientToProgramDto
{
    public int ProgramId { get; set; }
    public string? LocalMRN { get; set; }
    public bool IsPrimaryProgram { get; set; }
}

/// <summary>DTO for removing a patient from a program</summary>
public class RemovePatientFromProgramDto
{
    /// <summary>"Patient no longer seen", "Patient withdrew consent", "Consent issue"</summary>
    public string RemovalReason { get; set; } = string.Empty;
}

/// <summary>DTO for duplicate detection match results</summary>
public class DuplicateMatchDto
{
    public int PatientId { get; set; }
    public string RegistryId { get; set; } = string.Empty;
    public long CffId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? LastNameAtBirth { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BiologicalSexAtBirth { get; set; }
    public List<string> ProgramAssociations { get; set; } = new();
    public bool IsOrh { get; set; }
    public double ConfidenceScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}

/// <summary>DTO for duplicate detection request</summary>
public class DuplicateCheckDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BiologicalSexAtBirth { get; set; }
    public string? RegistryId { get; set; }
}

/// <summary>DTO for merge request</summary>
public class MergeRequestDto
{
    public int PrimaryPatientId { get; set; }
    public int SecondaryPatientId { get; set; }
}

/// <summary>DTO for merge result</summary>
public class MergeResultDto
{
    public int PrimaryPatientId { get; set; }
    public int SecondaryPatientId { get; set; }
    public int AliasesCreated { get; set; }
    public int AssociationsMerged { get; set; }
    public string Status { get; set; } = string.Empty;
}
