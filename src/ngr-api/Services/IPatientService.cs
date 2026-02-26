using NgrApi.DTOs;

namespace NgrApi.Services;

/// <summary>
/// Service interface for patient operations
/// </summary>
public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetPatientsAsync(int? careProgramId, string? status, string? searchTerm, int page, int pageSize);
    Task<PatientDto?> GetPatientByIdAsync(int id);
    Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto, string createdBy);
    Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto, string updatedBy);
    Task DeletePatientAsync(int id, string deletedBy);
    Task<int> GetPatientCountAsync(int? careProgramId);
}
