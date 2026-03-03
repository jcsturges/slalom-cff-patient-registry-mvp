using System.Text.Json;
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
        => await _context.FormDefinitions.Where(f => f.IsActive).ToListAsync();

    public async Task<FormDefinition?> GetFormDefinitionByIdAsync(int id)
        => await _context.FormDefinitions.FindAsync(id);

    public async Task<FormDefinition> CreateFormDefinitionAsync(FormDefinition fd)
    {
        fd.CreatedAt = DateTime.UtcNow;
        fd.IsShared = FormTypes.IsShared(fd.FormType);
        fd.AutoComplete = FormTypes.AutoCompleteFormTypes.Contains(fd.FormType);
        _context.FormDefinitions.Add(fd);
        await _context.SaveChangesAsync();
        return fd;
    }

    public async Task<FormDefinition?> UpdateFormDefinitionAsync(int id, FormDefinition fd)
    {
        var existing = await _context.FormDefinitions.FindAsync(id);
        if (existing == null) return null;
        existing.Name = fd.Name;
        existing.Description = fd.Description;
        existing.SchemaJson = fd.SchemaJson;
        existing.ValidationRulesJson = fd.ValidationRulesJson;
        existing.UiSchemaJson = fd.UiSchemaJson;
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

    // ── Form Submissions ─────────────────────────────────────────

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

    public async Task<FormSubmissionDto?> GetFormSubmissionByIdAsync(int id)
    {
        var fs = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Include(f => f.Program)
            .FirstOrDefaultAsync(f => f.Id == id);
        return fs == null ? null : MapSubmissionToDto(fs);
    }

    public async Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId, IPatientService patientService)
    {
        var patient = await patientService.GetPatientByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found");

        var allSubmissions = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Include(fs => fs.Program)
            .Where(fs => fs.PatientId == patientId)
            .OrderByDescending(fs => fs.UpdatedAt)
            .ToListAsync();

        var files = await _context.PatientFiles
            .Include(f => f.Program)
            .Where(f => f.PatientId == patientId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();

        var grouped = allSubmissions
            .GroupBy(fs => CategorizeForm(fs.FormDefinition.FormType))
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
                Id = f.Id, PatientId = f.PatientId, ProgramId = f.ProgramId,
                ProgramName = f.Program.Name, OriginalFileName = f.OriginalFileName,
                StoredFileName = f.StoredFileName, ContentType = f.ContentType,
                FileExtension = f.FileExtension, FileSize = f.FileSize,
                Description = f.Description, FileType = f.FileType,
                OtherFileTypeDescription = f.OtherFileTypeDescription,
                UploadedAt = f.UploadedAt, UploadedBy = f.UploadedBy,
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
            CompletionStatus = "Incomplete",
            LockStatus = "Unlocked",
            Status = "Incomplete",
            LastUpdateSource = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        await _context.Entry(submission).Reference(s => s.FormDefinition).LoadAsync();
        await _context.Entry(submission).Reference(s => s.Program).LoadAsync();

        return MapSubmissionToDto(submission);
    }

    // ── Save / Mark Complete (06-002) ────────────────────────────

    public async Task<FormSubmissionDto?> UpdateFormDataAsync(
        int id, UpdateFormDataDto dto, string updatedBy, bool isFoundationAdmin)
    {
        var fs = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Include(f => f.Program)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fs == null) return null;

        // Lock check: CP users cannot edit locked forms
        if (fs.LockStatus == "Locked" && !isFoundationAdmin)
            throw new InvalidOperationException("This form is locked and cannot be edited.");

        fs.FormDataJson = dto.FormDataJson;
        fs.UpdatedAt = DateTime.UtcNow;
        fs.UpdatedBy = updatedBy;
        fs.LastUpdateSource = "User";
        fs.RequiresReview = false;

        // Status evaluation
        if (dto.MarkComplete)
        {
            // Validate completion criteria
            var validation = ValidateFormData(fs);
            if (!validation.CanComplete)
                throw new InvalidOperationException("Form does not meet completion criteria.");
            fs.CompletionStatus = "Complete";
            fs.Status = "Complete";
        }
        else if (fs.FormDefinition.AutoComplete)
        {
            // Auto-complete: check if all required fields are filled
            var validation = ValidateFormData(fs);
            fs.CompletionStatus = validation.CanComplete ? "Complete" : "Incomplete";
            fs.Status = fs.CompletionStatus;
        }
        else
        {
            // If editing a Complete form and required fields now missing, revert
            if (fs.CompletionStatus == "Complete")
            {
                var validation = ValidateFormData(fs);
                if (!validation.CanComplete)
                {
                    fs.CompletionStatus = "Incomplete";
                    fs.Status = "Incomplete";
                }
            }
        }

        await _context.SaveChangesAsync();
        return MapSubmissionToDto(fs);
    }

    // ── Deletion (06-004) ────────────────────────────────────────

    public async Task<bool> DeleteFormSubmissionAsync(int id, bool isFoundationAdmin = false)
    {
        var fs = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fs == null) return false;

        // Cannot delete locked forms
        if (fs.LockStatus == "Locked")
            throw new InvalidOperationException("Locked forms cannot be deleted.");

        // Only Foundation Admins can delete shared forms
        if (fs.FormDefinition.IsShared && !isFoundationAdmin)
            throw new InvalidOperationException("Only Foundation Administrators can delete shared forms.");

        _context.FormSubmissions.Remove(fs);
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Database Lock Execution (06-005) ─────────────────────────

    public async Task<DatabaseLockResultDto> ExecuteDatabaseLockAsync(
        int reportingYear, string executedBy)
    {
        int locked = 0;
        int skipped = 0;

        var yearStart = new DateTime(reportingYear, 1, 1);
        var yearEnd = new DateTime(reportingYear, 12, 31, 23, 59, 59);

        // Lock Annual Review forms by year
        var annualReviews = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Where(f => f.FormDefinition.FormType == FormTypes.AnnualReview
                     && f.AnnualReviewYear == reportingYear
                     && f.LockStatus == "Unlocked")
            .ToListAsync();

        foreach (var form in annualReviews)
        {
            form.LockStatus = "Locked";
            form.Status = form.CompletionStatus == "Complete" ? "Complete" : "Incomplete";
            locked++;
        }

        // Lock date-based forms within reporting year
        var dateBasedForms = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Where(f => f.LockStatus == "Unlocked"
                     && (f.FormDefinition.FormType == FormTypes.Encounter
                      || f.FormDefinition.FormType == FormTypes.LabsAndTests
                      || f.FormDefinition.FormType == FormTypes.PhoneNote))
            .ToListAsync();

        foreach (var form in dateBasedForms)
        {
            var formDate = form.EncounterDate ?? form.LabDate ?? form.PhoneNoteDate;
            if (formDate.HasValue && formDate.Value >= yearStart && formDate.Value <= yearEnd)
            {
                form.LockStatus = "Locked";
                locked++;
            }
        }

        // Lock Care Episodes that ended within reporting year
        var careEpisodes = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Where(f => f.FormDefinition.FormType == FormTypes.CareEpisode
                     && f.LockStatus == "Unlocked"
                     && f.CareEpisodeEndDate.HasValue)
            .ToListAsync();

        foreach (var form in careEpisodes)
        {
            if (form.CareEpisodeEndDate!.Value >= yearStart && form.CareEpisodeEndDate.Value <= yearEnd)
            {
                form.LockStatus = "Locked";
                locked++;
            }
            // Care Episodes with no end date are not impacted
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Database lock for year {Year} completed: {Locked} locked, {Skipped} skipped. By: {User}",
            reportingYear, locked, skipped, executedBy);

        return new DatabaseLockResultDto
        {
            ReportingYear = reportingYear,
            FormsLocked = locked,
            FormsSkipped = skipped,
            Status = "Completed",
        };
    }

    // ── Validation Engine (06-010) ───────────────────────────────

    public FormValidationResultDto ValidateFormData(FormSubmission submission)
    {
        var result = new FormValidationResultDto
        {
            IsValid = true,
            CanSave = true,
            CanComplete = true,
            Messages = new List<ValidationMessageDto>(),
        };

        // Parse form data
        JsonDocument formData;
        try
        {
            formData = JsonDocument.Parse(submission.FormDataJson);
        }
        catch
        {
            result.CanSave = false;
            result.CanComplete = false;
            result.IsValid = false;
            result.Messages.Add(new ValidationMessageDto
            {
                Severity = "SaveBlocking",
                FieldId = "_formData",
                FieldLabel = "Form Data",
                Message = "Form data is not valid JSON.",
                CorrectiveAction = "Fix the form data structure.",
            });
            return result;
        }

        // Parse schema to get required fields
        if (!string.IsNullOrEmpty(submission.FormDefinition?.SchemaJson))
        {
            try
            {
                var schema = JsonDocument.Parse(submission.FormDefinition.SchemaJson);
                if (schema.RootElement.TryGetProperty("required", out var requiredProp)
                    && requiredProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var req in requiredProp.EnumerateArray())
                    {
                        var fieldId = req.GetString();
                        if (fieldId == null) continue;

                        var hasValue = formData.RootElement.TryGetProperty(fieldId, out var val)
                                       && val.ValueKind != JsonValueKind.Null
                                       && val.ToString() != "";

                        if (!hasValue)
                        {
                            result.CanComplete = false;
                            result.Messages.Add(new ValidationMessageDto
                            {
                                Severity = "CompletionBlocking",
                                FieldId = fieldId,
                                FieldLabel = fieldId,
                                Message = $"Required field '{fieldId}' is empty.",
                                CorrectiveAction = $"Fill in the {fieldId} field.",
                            });
                        }
                    }
                }

                // Check validation rules if defined
                if (!string.IsNullOrEmpty(submission.FormDefinition.ValidationRulesJson))
                {
                    var rules = JsonDocument.Parse(submission.FormDefinition.ValidationRulesJson);
                    // Generic rule evaluation would go here
                    // For now, schema-based required field check covers the basics
                }
            }
            catch
            {
                // Schema parse error is non-blocking
            }
        }

        formData.Dispose();
        return result;
    }

    // ── EMR Status Downgrade (06-012) ────────────────────────────

    public async Task<FormSubmissionDto?> ApplyEmrUpdateAsync(
        int id, string formDataJson)
    {
        var fs = await _context.FormSubmissions
            .Include(f => f.FormDefinition)
            .Include(f => f.Program)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fs == null) return null;

        fs.FormDataJson = formDataJson;
        fs.LastUpdateSource = "EMR";
        fs.RequiresReview = true;
        fs.UpdatedAt = DateTime.UtcNow;
        fs.UpdatedBy = "EMR";

        // Auto-downgrade to Incomplete (except Demographics which auto-completes)
        if (fs.FormDefinition.FormType == FormTypes.Demographics && fs.FormDefinition.AutoComplete)
        {
            var validation = ValidateFormData(fs);
            fs.CompletionStatus = validation.CanComplete ? "Complete" : "Incomplete";
        }
        else
        {
            fs.CompletionStatus = "Incomplete";
        }
        fs.Status = fs.CompletionStatus;

        await _context.SaveChangesAsync();
        return MapSubmissionToDto(fs);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static string CategorizeForm(string formType) => formType switch
    {
        FormTypes.Demographics or FormTypes.Diagnosis or FormTypes.SweatTest => "shared",
        FormTypes.Transplant => "transplant",
        FormTypes.AnnualReview => "annual_review",
        FormTypes.Encounter => "encounter",
        FormTypes.LabsAndTests => "lab",
        FormTypes.CareEpisode => "care_episode",
        FormTypes.PhoneNote => "phone_note",
        FormTypes.AldInitiation => "ald",
        _ => "shared",
    };

    private static FormSubmissionDto MapSubmissionToDto(FormSubmission fs) => new()
    {
        Id = fs.Id,
        FormDefinitionId = fs.FormDefinitionId,
        FormName = fs.FormDefinition?.Name ?? "",
        FormCode = fs.FormDefinition?.Code ?? "",
        FormType = fs.FormDefinition?.FormType ?? "",
        IsShared = fs.FormDefinition?.IsShared ?? false,
        PatientId = fs.PatientId,
        EncounterId = fs.EncounterId,
        ProgramId = fs.ProgramId,
        ProgramName = fs.Program?.Name ?? "",
        CompletionStatus = fs.CompletionStatus,
        LockStatus = fs.LockStatus,
        Status = fs.Status,
        LastUpdateSource = fs.LastUpdateSource,
        RequiresReview = fs.RequiresReview,
        CreatedAt = fs.CreatedAt,
        UpdatedAt = fs.UpdatedAt,
        LastModifiedBy = fs.UpdatedBy,
        EncounterDate = fs.EncounterDate?.ToString("o"),
        AnnualReviewYear = fs.AnnualReviewYear,
        TransplantOrgan = fs.TransplantOrgan,
        CareEpisodeStartDate = fs.CareEpisodeStartDate?.ToString("o"),
        CareEpisodeEndDate = fs.CareEpisodeEndDate?.ToString("o"),
        PhoneNoteDate = fs.PhoneNoteDate?.ToString("o"),
        LabDate = fs.LabDate?.ToString("o"),
    };
}
