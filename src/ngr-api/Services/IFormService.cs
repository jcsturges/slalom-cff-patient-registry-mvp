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

    // Form Submissions
    Task<IEnumerable<FormSubmissionDto>> GetPatientFormSubmissionsAsync(int patientId, string? formCode = null, int page = 1, int pageSize = 5);
    Task<FormSubmissionDto?> GetFormSubmissionByIdAsync(int id);
    Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId, IPatientService patientService);
    Task<FormSubmissionDto> CreateFormSubmissionAsync(CreateFormSubmissionDto dto, string createdBy);
    Task<FormSubmissionDto?> UpdateFormDataAsync(int id, UpdateFormDataDto dto, string updatedBy, bool isFoundationAdmin);
    Task<bool> DeleteFormSubmissionAsync(int id, bool isFoundationAdmin = false);

    // Validation
    FormValidationResultDto ValidateFormData(FormSubmission submission);

    // Database Lock
    Task<DatabaseLockResultDto> ExecuteDatabaseLockAsync(int reportingYear, string executedBy);

    // EMR
    Task<FormSubmissionDto?> ApplyEmrUpdateAsync(int id, string formDataJson);
}
