using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
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

    // ── Form Definitions ─────────────────────────────────────────

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

    // ── Form Submissions (05-001, 05-002) ────────────────────────

    public async Task<IEnumerable<FormSubmissionDto>> GetPatientFormSubmissionsAsync(
        int patientId, string? formCode = null, int page = 1, int pageSize = 5)
    {
        var query = _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Include(fs => fs.Program)
            .Where(fs => fs.PatientId == patientId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(formCode))
            query = query.Where(fs => fs.FormDefinition.Code == formCode);

        var submissions = await query
            .OrderByDescending(fs => fs.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return submissions.Select(MapSubmissionToDto);
    }

    public async Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId, IPatientService patientService)
    {
        var patient = await patientService.GetPatientByIdAsync(patientId);
        if (patient == null) throw new ArgumentException("Patient not found");

        // Get all form submissions grouped by type
        var allSubmissions = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Include(fs => fs.Program)
            .Where(fs => fs.PatientId == patientId)
            .OrderByDescending(fs => fs.UpdatedAt)
            .ToListAsync();

        // Get files
        var files = await _context.PatientFiles
            .Include(f => f.Program)
            .Where(f => f.PatientId == patientId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();

        // Categorize forms by code prefix
        var grouped = allSubmissions
            .GroupBy(fs => CategorizeForm(fs.FormDefinition.Code))
            .ToDictionary(g => g.Key, g => g.Select(MapSubmissionToDto).ToList());

        return new PatientDashboardDto
        {
            Patient = patient,
            SharedForms = grouped.GetValueOrDefault("shared", new()),
            Transplants = grouped.GetValueOrDefault("transplant", new()),
            AnnualReviews = grouped.GetValueOrDefault("annual_review", new()),
            Encounters = grouped.GetValueOrDefault("encounter", new()),
            Labs = grouped.GetValueOrDefault("lab", new()),
            CareEpisodes = grouped.GetValueOrDefault("care_episode", new()),
            PhoneNotes = grouped.GetValueOrDefault("phone_note", new()),
            AldStatus = grouped.GetValueOrDefault("ald", new()),
            Files = files.Select(f => new PatientFileDto
            {
                Id = f.Id,
                PatientId = f.PatientId,
                ProgramId = f.ProgramId,
                ProgramName = f.Program.Name,
                OriginalFileName = f.OriginalFileName,
                StoredFileName = f.StoredFileName,
                ContentType = f.ContentType,
                FileExtension = f.FileExtension,
                FileSize = f.FileSize,
                Description = f.Description,
                FileType = f.FileType,
                OtherFileTypeDescription = f.OtherFileTypeDescription,
                UploadedAt = f.UploadedAt,
                UploadedBy = f.UploadedBy,
            }).ToList(),
        };
    }

    public async Task<FormSubmissionDto> CreateFormSubmissionAsync(CreateFormSubmissionDto dto, string createdBy)
    {
        var formDef = await _context.FormDefinitions.FindAsync(dto.FormDefinitionId)
            ?? throw new ArgumentException("Form definition not found");

        var submission = new FormSubmission
        {
            FormDefinitionId = dto.FormDefinitionId,
            PatientId = dto.PatientId,
            EncounterId = dto.EncounterId,
            ProgramId = dto.ProgramId,
            FormDataJson = dto.FormDataJson ?? "{}",
            Status = "Incomplete",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(submission).Reference(s => s.FormDefinition).LoadAsync();
        await _context.Entry(submission).Reference(s => s.Program).LoadAsync();

        return MapSubmissionToDto(submission);
    }

    public async Task<bool> DeleteFormSubmissionAsync(int id)
    {
        var existing = await _context.FormSubmissions.FindAsync(id);
        if (existing == null) return false;

        _context.FormSubmissions.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static string CategorizeForm(string code)
    {
        var lower = code.ToLower();
        if (lower.Contains("demo") || lower.Contains("diag") || lower.Contains("sweat"))
            return "shared";
        if (lower.Contains("transplant")) return "transplant";
        if (lower.Contains("annual") || lower.Contains("review")) return "annual_review";
        if (lower.Contains("encounter")) return "encounter";
        if (lower.Contains("lab")) return "lab";
        if (lower.Contains("care") || lower.Contains("episode")) return "care_episode";
        if (lower.Contains("phone") || lower.Contains("note")) return "phone_note";
        if (lower.Contains("ald")) return "ald";
        return "shared";
    }

    private static FormSubmissionDto MapSubmissionToDto(FormSubmission fs) => new()
    {
        Id = fs.Id,
        FormDefinitionId = fs.FormDefinitionId,
        FormName = fs.FormDefinition?.Name ?? "",
        FormCode = fs.FormDefinition?.Code ?? "",
        PatientId = fs.PatientId,
        EncounterId = fs.EncounterId,
        ProgramId = fs.ProgramId,
        ProgramName = fs.Program?.Name ?? "",
        Status = fs.Status,
        CreatedAt = fs.CreatedAt,
        UpdatedAt = fs.UpdatedAt,
        LastModifiedBy = fs.UpdatedBy,
    };
}
