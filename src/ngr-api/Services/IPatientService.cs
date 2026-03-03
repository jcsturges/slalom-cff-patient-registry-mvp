using NgrApi.DTOs;

namespace NgrApi.Services;

/// <summary>
/// Service interface for patient operations (SRS Section 4)
/// </summary>
public interface IPatientService
{
    // Queries
    Task<IEnumerable<PatientDto>> GetPatientsAsync(int? careProgramId, string? status, string? searchTerm, int page, int pageSize);
    Task<PatientDto?> GetPatientByIdAsync(int id);
    Task<int> GetPatientCountAsync(int? careProgramId);

    // CRUD
    Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto, string createdBy);
    Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto, string updatedBy);
    Task DeletePatientAsync(int id, string deletedBy);

    // Program associations (04-002, 04-006, 04-007)
    Task<IEnumerable<PatientProgramAssociationDto>> GetProgramAssociationsAsync(int patientId);
    Task<PatientProgramAssociationDto> AddToProgramAsync(int patientId, AddPatientToProgramDto dto, string addedBy);
    Task RemoveFromProgramAsync(int patientId, int programId, RemovePatientFromProgramDto dto, string removedBy);

    // Duplicate detection (04-005)
    Task<IEnumerable<DuplicateMatchDto>> CheckDuplicatesAsync(DuplicateCheckDto dto);

    // Merge (04-008, 04-009, 04-010)
    Task<MergeResultDto> MergeAsync(MergeRequestDto dto, string mergedBy);
}
