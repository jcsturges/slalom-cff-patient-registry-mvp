namespace NgrApi.Models;

/// <summary>
/// Represents a patient in the registry (SRS Section 4.1)
/// </summary>
public class Patient
{
    public int Id { get; set; }

    /// <summary>User-friendly registry ID (e.g., "NGR-A1B2C3")</summary>
    public string RegistryId { get; set; } = string.Empty;

    /// <summary>Unique numeric CFF ID — auto-assigned for new patients, migrated for existing</summary>
    public long CffId { get; set; }

    // ── Identity fields ──────────────────────────────────────────
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? LastNameAtBirth { get; set; }
    public DateTime DateOfBirth { get; set; }

    /// <summary>Biological sex at birth (Male, Female, Unknown)</summary>
    public string? BiologicalSexAtBirth { get; set; }

    /// <summary>Legacy Gender field (kept for backward compatibility)</summary>
    public string? Gender { get; set; }

    /// <summary>Last 4 digits of SSN (optional, encrypted at rest)</summary>
    public string? SsnLast4 { get; set; }

    public string? MedicalRecordNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // ── Status fields ────────────────────────────────────────────
    public string Status { get; set; } = "Active";
    public bool IsDeceased { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public bool ConsentWithdrawn { get; set; }

    // ── Calculated fields from CRFs (materialized for performance) ──
    public string? Diagnosis { get; set; }
    public string VitalStatus { get; set; } = "Alive";
    public bool? AldStatus { get; set; }
    public bool? HasLungTransplant { get; set; }
    public DateTime? LastSeenInProgram { get; set; }

    // ── Primary program (legacy FK, kept for backward compat) ────
    public int CareProgramId { get; set; }

    // ── Migration provenance (13-006) ────────────────────────────
    /// <summary>True when this record was created by the historical migration process</summary>
    public bool IsMigrated { get; set; }

    /// <summary>Stable identifier from the source system (e.g. portCF numeric ID)</summary>
    public string? SourceSystemId { get; set; }

    // ── Audit ────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public CareProgram CareProgram { get; set; } = null!;
    public PatientDemographics? Demographics { get; set; }
    public ICollection<PatientProgramAssignment> ProgramAssignments { get; set; } = new List<PatientProgramAssignment>();
    public ICollection<PatientAlias> Aliases { get; set; } = new List<PatientAlias>();
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
/// Patient assignment to a care program (SRS Section 4.2)
/// </summary>
public class PatientProgramAssignment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProgramId { get; set; }
    public string? LocalMRN { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsPrimaryProgram { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? DisenrollmentDate { get; set; }

    /// <summary>Reason for removal: "Patient no longer seen", "Patient withdrew consent", "Consent issue"</summary>
    public string? RemovalReason { get; set; }
    public string? RemovedBy { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public CareProgram Program { get; set; } = null!;
}

/// <summary>
/// Alias records from merged patients — stores secondary CFF IDs and past names
/// </summary>
public class PatientAlias
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    /// <summary>Type of alias: "CffId", "RegistryId", "Name"</summary>
    public string AliasType { get; set; } = string.Empty;

    /// <summary>The alias value (e.g., old CFF ID, old name)</summary>
    public string AliasValue { get; set; } = string.Empty;

    /// <summary>Source of alias (e.g., "Merge from patient #123")</summary>
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; set; } = null!;
}
