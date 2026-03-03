using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IFormService
{
    // Form Definitions
    Task<IEnumerable<FormDefinition>> GetFormDefinitionsAsync();
    Task<FormDefinition?> GetFormDefinitionByIdAsync(int id);
    Task<FormDefinition> CreateFormDefinitionAsync(FormDefinition formDefinition);
    Task<FormDefinition?> UpdateFormDefinitionAsync(int id, FormDefinition formDefinition);
    Task<bool> DeleteFormDefinitionAsync(int id);

    // Form Submissions (05-001, 05-002)
    Task<IEnumerable<FormSubmissionDto>> GetPatientFormSubmissionsAsync(int patientId, string? formCode = null, int page = 1, int pageSize = 5);
    Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId, IPatientService patientService);
    Task<FormSubmissionDto> CreateFormSubmissionAsync(CreateFormSubmissionDto dto, string createdBy);
    Task<bool> DeleteFormSubmissionAsync(int id);
}
