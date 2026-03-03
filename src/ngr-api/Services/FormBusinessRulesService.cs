using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IFormBusinessRulesService
{
    /// <summary>Check if a form of the given type can be created for a patient (gating rules)</summary>
    Task<(bool Allowed, string? Reason)> CanCreateFormAsync(int patientId, string formType, int programId);

    /// <summary>Check transplant organ uniqueness</summary>
    Task<(bool Allowed, string? Reason)> ValidateTransplantCreationAsync(int patientId, string organ);

    /// <summary>Get default year for Annual Review</summary>
    Task<int> GetDefaultAnnualReviewYearAsync(int patientId, int programId);

    /// <summary>Check Annual Review uniqueness (one per patient per program per year)</summary>
    Task<(bool Allowed, string? Reason)> ValidateAnnualReviewCreationAsync(int patientId, int programId, int year);

    /// <summary>Check date uniqueness for Encounter, Labs, or Phone Note</summary>
    Task<(bool Allowed, string? Reason)> ValidateDateUniquenessAsync(int patientId, int programId, string formType, DateTime date, int? excludeFormId = null);

    /// <summary>Carry-forward data from prior encounter for Medications or Complications</summary>
    Task<string?> GetCarryForwardDataAsync(int patientId, int programId, string subFormType);

    /// <summary>Validate Care Episode segment overlaps</summary>
    Task<FormValidationResultDto> ValidateCareEpisodeSegmentsAsync(int patientId, string formDataJson, int? excludeFormId = null);

    /// <summary>Check if Care Episode can be marked complete (all segments closed)</summary>
    (bool CanComplete, List<string> OpenSegments) CheckCareEpisodeCompletion(string formDataJson);
}

public class FormBusinessRulesService : IFormBusinessRulesService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FormBusinessRulesService> _logger;

    public FormBusinessRulesService(ApplicationDbContext context, ILogger<FormBusinessRulesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── 07-001: Demographics Gating ──────────────────────────────

    public async Task<(bool Allowed, string? Reason)> CanCreateFormAsync(
        int patientId, string formType, int programId)
    {
        // Demographics itself has no prerequisites
        if (formType == FormTypes.Demographics)
            return (true, null);

        // Check if Demographics is Complete
        var demographicsComplete = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .AnyAsync(fs =>
                fs.PatientId == patientId &&
                fs.FormDefinition.FormType == FormTypes.Demographics &&
                fs.CompletionStatus == "Complete");

        if (!demographicsComplete)
            return (false, "Demographics must be Complete before creating other forms. Please complete the Demographics form first.");

        // Sweat Test and file uploads are allowed without Diagnosis (07-002)
        if (formType == FormTypes.SweatTest)
            return (true, null);

        // 07-002: Diagnosis Gating — all other forms require Complete Diagnosis
        if (formType != FormTypes.Diagnosis)
        {
            var diagnosisComplete = await _context.FormSubmissions
                .Include(fs => fs.FormDefinition)
                .AnyAsync(fs =>
                    fs.PatientId == patientId &&
                    fs.FormDefinition.FormType == FormTypes.Diagnosis &&
                    fs.CompletionStatus == "Complete");

            if (!diagnosisComplete)
                return (false, "Diagnosis must be Complete before creating clinical forms. Please complete the Diagnosis form first.");
        }

        return (true, null);
    }

    // ── 07-003: Transplant Organ Uniqueness ──────────────────────

    public async Task<(bool Allowed, string? Reason)> ValidateTransplantCreationAsync(
        int patientId, string organ)
    {
        // Check if patient has an existing Transplant form for this organ
        // without a 'Had Transplantation' step
        var existingTransplants = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.FormDefinition.FormType == FormTypes.Transplant &&
                fs.TransplantOrgan == organ)
            .ToListAsync();

        foreach (var existing in existingTransplants)
        {
            try
            {
                var data = JsonDocument.Parse(existing.FormDataJson);
                var hasTransplantation = data.RootElement.TryGetProperty("hadTransplantation", out var val)
                    && val.GetBoolean();
                data.Dispose();

                if (!hasTransplantation)
                    return (false, $"An existing Transplant form for '{organ}' does not have a 'Had Transplantation' step. Complete the existing transplant journey before creating a new one.");
            }
            catch
            {
                // If data can't be parsed, allow creation
            }
        }

        return (true, null);
    }

    // ── 07-004: Annual Review Year Defaulting ────────────────────

    public async Task<int> GetDefaultAnnualReviewYearAsync(int patientId, int programId)
    {
        var currentYear = DateTime.UtcNow.Year;

        // Get years that already have Annual Reviews for this patient/program
        var existingYears = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == FormTypes.AnnualReview &&
                fs.AnnualReviewYear.HasValue)
            .Select(fs => fs.AnnualReviewYear!.Value)
            .ToListAsync();

        // Get locked years
        var lockedYears = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == FormTypes.AnnualReview &&
                fs.LockStatus == "Locked" &&
                fs.AnnualReviewYear.HasValue)
            .Select(fs => fs.AnnualReviewYear!.Value)
            .Distinct()
            .ToListAsync();

        // Try current year first, then prior year
        if (!existingYears.Contains(currentYear) && !lockedYears.Contains(currentYear))
            return currentYear;
        if (!existingYears.Contains(currentYear - 1) && !lockedYears.Contains(currentYear - 1))
            return currentYear - 1;

        // Fallback to current year (will fail uniqueness check)
        return currentYear;
    }

    public async Task<(bool Allowed, string? Reason)> ValidateAnnualReviewCreationAsync(
        int patientId, int programId, int year)
    {
        // No future years
        if (year > DateTime.UtcNow.Year)
            return (false, "Cannot create Annual Review for a future year.");

        // No locked years
        var isLocked = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .AnyAsync(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == FormTypes.AnnualReview &&
                fs.AnnualReviewYear == year &&
                fs.LockStatus == "Locked");

        if (isLocked)
            return (false, $"Annual Review for year {year} is locked and cannot be modified.");

        // One per patient per program per year
        var exists = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .AnyAsync(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == FormTypes.AnnualReview &&
                fs.AnnualReviewYear == year);

        if (exists)
            return (false, $"An Annual Review already exists for year {year} in this program.");

        return (true, null);
    }

    // ── 07-005/07/10: Date Uniqueness ────────────────────────────

    public async Task<(bool Allowed, string? Reason)> ValidateDateUniquenessAsync(
        int patientId, int programId, string formType, DateTime date, int? excludeFormId = null)
    {
        var query = _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == formType);

        if (excludeFormId.HasValue)
            query = query.Where(fs => fs.Id != excludeFormId.Value);

        bool duplicate = formType switch
        {
            FormTypes.Encounter => await query.AnyAsync(fs => fs.EncounterDate.HasValue && fs.EncounterDate.Value.Date == date.Date),
            FormTypes.LabsAndTests => await query.AnyAsync(fs => fs.LabDate.HasValue && fs.LabDate.Value.Date == date.Date),
            FormTypes.PhoneNote => await query.AnyAsync(fs => fs.PhoneNoteDate.HasValue && fs.PhoneNoteDate.Value.Date == date.Date),
            _ => false,
        };

        if (duplicate)
        {
            var typeName = formType switch
            {
                FormTypes.Encounter => "Encounter",
                FormTypes.LabsAndTests => "Labs & Tests",
                FormTypes.PhoneNote => "Phone Note",
                _ => formType,
            };
            return (false, $"A {typeName} already exists for {date:MMMM d, yyyy} in this program.");
        }

        return (true, null);
    }

    // ── 07-006: Encounter Carry-Forward ──────────────────────────

    public async Task<string?> GetCarryForwardDataAsync(
        int patientId, int programId, string subFormType)
    {
        // Find the most recent prior Encounter in this program
        var priorEncounter = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.ProgramId == programId &&
                fs.FormDefinition.FormType == FormTypes.Encounter)
            .OrderByDescending(fs => fs.EncounterDate ?? fs.UpdatedAt)
            .FirstOrDefaultAsync();

        if (priorEncounter == null) return null;

        // Extract the sub-form data
        try
        {
            var data = JsonDocument.Parse(priorEncounter.FormDataJson);
            if (data.RootElement.TryGetProperty(subFormType, out var subFormData))
            {
                var result = subFormData.GetRawText();
                data.Dispose();
                return result;
            }
            data.Dispose();
        }
        catch
        {
            // Parse error — no carry-forward available
        }

        return null;
    }

    // ── 07-008: Care Episode Segment Overlap Validation ──────────

    public async Task<FormValidationResultDto> ValidateCareEpisodeSegmentsAsync(
        int patientId, string formDataJson, int? excludeFormId = null)
    {
        var result = new FormValidationResultDto { IsValid = true, CanSave = true, CanComplete = true };

        // Parse current episode's segments
        List<(DateTime Start, DateTime? End, int Index)> currentSegments;
        try
        {
            var data = JsonDocument.Parse(formDataJson);
            currentSegments = ParseSegments(data);
            data.Dispose();
        }
        catch
        {
            result.CanSave = false;
            result.Messages.Add(new ValidationMessageDto
            {
                Severity = "SaveBlocking",
                FieldId = "segments",
                FieldLabel = "Segments",
                Message = "Invalid segment data.",
            });
            return result;
        }

        // 1. Check within-episode overlap
        for (int i = 0; i < currentSegments.Count; i++)
        {
            for (int j = i + 1; j < currentSegments.Count; j++)
            {
                if (SegmentsOverlap(currentSegments[i], currentSegments[j]))
                {
                    result.CanSave = false;
                    result.IsValid = false;
                    result.Messages.Add(new ValidationMessageDto
                    {
                        Severity = "SaveBlocking",
                        FieldId = $"segment_{i}",
                        FieldLabel = $"Segment {i + 1}",
                        Message = $"Segment {i + 1} overlaps with Segment {j + 1}.",
                        CorrectiveAction = "Adjust start or end dates to remove overlap.",
                    });
                }
            }
        }

        // Check only one open-ended segment per episode
        var openEnded = currentSegments.Where(s => !s.End.HasValue).ToList();
        if (openEnded.Count > 1)
        {
            result.CanSave = false;
            result.Messages.Add(new ValidationMessageDto
            {
                Severity = "SaveBlocking",
                FieldId = "segments",
                FieldLabel = "Segments",
                Message = $"Only one open-ended segment is allowed per episode. Found {openEnded.Count}.",
            });
        }

        // 2. Check between-episode overlap
        var otherEpisodes = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Where(fs =>
                fs.PatientId == patientId &&
                fs.FormDefinition.FormType == FormTypes.CareEpisode &&
                (!excludeFormId.HasValue || fs.Id != excludeFormId.Value))
            .ToListAsync();

        foreach (var otherEp in otherEpisodes)
        {
            try
            {
                var otherData = JsonDocument.Parse(otherEp.FormDataJson);
                var otherSegments = ParseSegments(otherData);
                otherData.Dispose();

                foreach (var seg in currentSegments)
                {
                    foreach (var otherSeg in otherSegments)
                    {
                        if (SegmentsOverlap(seg, otherSeg))
                        {
                            result.CanSave = false;
                            result.IsValid = false;
                            result.Messages.Add(new ValidationMessageDto
                            {
                                Severity = "SaveBlocking",
                                FieldId = $"segment_{seg.Index}",
                                FieldLabel = $"Segment {seg.Index + 1}",
                                Message = $"Segment {seg.Index + 1} overlaps with a segment in Care Episode #{otherEp.Id}.",
                                CorrectiveAction = "Adjust dates or merge with the existing episode.",
                            });
                        }
                    }
                }
            }
            catch { /* Skip unparseable episodes */ }
        }

        return result;
    }

    // ── 07-009: Care Episode Completion ──────────────────────────

    public (bool CanComplete, List<string> OpenSegments) CheckCareEpisodeCompletion(string formDataJson)
    {
        var openSegments = new List<string>();
        try
        {
            var data = JsonDocument.Parse(formDataJson);
            var segments = ParseSegments(data);
            data.Dispose();

            for (int i = 0; i < segments.Count; i++)
            {
                if (!segments[i].End.HasValue)
                    openSegments.Add($"Segment {i + 1}");
            }
        }
        catch
        {
            openSegments.Add("Invalid segment data");
        }

        return (openSegments.Count == 0, openSegments);
    }

    // ── Private Helpers ──────────────────────────────────────────

    private static List<(DateTime Start, DateTime? End, int Index)> ParseSegments(JsonDocument data)
    {
        var segments = new List<(DateTime Start, DateTime? End, int Index)>();

        if (data.RootElement.TryGetProperty("segments", out var segs) && segs.ValueKind == JsonValueKind.Array)
        {
            int idx = 0;
            foreach (var seg in segs.EnumerateArray())
            {
                if (seg.TryGetProperty("startDateTime", out var startProp) &&
                    DateTime.TryParse(startProp.GetString(), out var start))
                {
                    DateTime? end = null;
                    if (seg.TryGetProperty("endDateTime", out var endProp) &&
                        endProp.ValueKind != JsonValueKind.Null &&
                        DateTime.TryParse(endProp.GetString(), out var endVal))
                    {
                        end = endVal;
                    }

                    segments.Add((start, end, idx));
                }
                idx++;
            }
        }

        return segments;
    }

    private static bool SegmentsOverlap(
        (DateTime Start, DateTime? End, int Index) a,
        (DateTime Start, DateTime? End, int Index) b)
    {
        // Open-ended segments treated as [Start, +∞)
        var aEnd = a.End ?? DateTime.MaxValue;
        var bEnd = b.End ?? DateTime.MaxValue;

        return aEnd >= b.Start && bEnd >= a.Start;
    }
}
