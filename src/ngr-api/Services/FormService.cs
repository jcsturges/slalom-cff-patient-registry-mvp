using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class FormService : IFormService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FormService> _logger;

    public FormService(ApplicationDbContext context, ILogger<FormService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FormDefinition>> GetFormDefinitionsAsync()
    {
        return await _context.FormDefinitions.ToListAsync();
    }

    public async Task<FormDefinition?> GetFormDefinitionByIdAsync(int id)
    {
        return await _context.FormDefinitions.FindAsync(id);
    }

    public async Task<FormDefinition> CreateFormDefinitionAsync(FormDefinition formDefinition)
    {
        formDefinition.CreatedAt = DateTime.UtcNow;
        _context.FormDefinitions.Add(formDefinition);
        await _context.SaveChangesAsync();
        return formDefinition;
    }

    public async Task<FormDefinition?> UpdateFormDefinitionAsync(int id, FormDefinition formDefinition)
    {
        var existing = await _context.FormDefinitions.FindAsync(id);
        if (existing == null) return null;

        existing.Name = formDefinition.Name;
        existing.Description = formDefinition.Description;
        existing.SchemaJson = formDefinition.SchemaJson;
        existing.ValidationRulesJson = formDefinition.ValidationRulesJson;
        existing.UiSchemaJson = formDefinition.UiSchemaJson;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteFormDefinitionAsync(int id)
    {
        var existing = await _context.FormDefinitions.FindAsync(id);
        if (existing == null) return false;

        _context.FormDefinitions.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
