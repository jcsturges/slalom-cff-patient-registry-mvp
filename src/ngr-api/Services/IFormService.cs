using NgrApi.Models;

namespace NgrApi.Services;

public interface IFormService
{
    Task<IEnumerable<FormDefinition>> GetFormDefinitionsAsync();
    Task<FormDefinition?> GetFormDefinitionByIdAsync(int id);
    Task<FormDefinition> CreateFormDefinitionAsync(FormDefinition formDefinition);
    Task<FormDefinition?> UpdateFormDefinitionAsync(int id, FormDefinition formDefinition);
    Task<bool> DeleteFormDefinitionAsync(int id);
}
