using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public class ImportService : IImportService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmrMappingService _mappingService;
    private readonly IFormService _formService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".csv" };
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    // Required headers that every EMR CSV must contain
    private static readonly string[] RequiredHeaders = ["CFF_ID", "MRN"];

    public ImportService(
        ApplicationDbContext context,
        IEmrMappingService mappingService,
        IFormService formService,
        IServiceScopeFactory scopeFactory,
        ILogger<ImportService> logger)
    {
        _context = context;
        _mappingService = mappingService;
        _formService = formService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // ── Upload ────────────────────────────────────────────────────────────────

    public async Task<ImportJobDto> UploadCsvAsync(IFormFile file, int programId, string uploadedBy)
    {
        // File-level checks before touching the DB
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Only .csv files are accepted. Received: {ext}");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File exceeds maximum size of 50 MB ({file.Length:N0} bytes received).");

        // Quick header validation
        EmrValidationResult validation;
        using (var stream = file.OpenReadStream())
            validation = await ValidateCsvAsync(stream, programId);

        var errorsJson = validation.Errors.Count > 0
            ? JsonSerializer.Serialize(validation.Errors)
            : null;

        var warningsJson = validation.Warnings.Count > 0
            ? JsonSerializer.Serialize(validation.Warnings)
            : null;

        var job = new ImportJob
        {
            FileName = file.FileName,
            ProgramId = programId,
            CreatedBy = uploadedBy,
            Status = validation.IsValid ? "Pending" : "ValidationFailed",
            TotalRows = validation.RowCount,
            ErrorsJson = errorsJson,
            MappingJson = warningsJson,  // reuse MappingJson for warnings for now
            CreatedAt = DateTime.UtcNow,
        };

        _context.ImportJobs.Add(job);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "CSV upload received: job {JobId}, file {FileName}, program {ProgramId}, rows {Rows}, valid: {Valid}",
            job.Id, file.FileName, programId, validation.RowCount, validation.IsValid);

        // Kick off async processing only if validation passed
        if (validation.IsValid)
        {
            // Store the CSV content for processing (in real production: upload to blob storage)
            // For local dev we hold the content in ResultsJson as base64 — same pattern as file upload
            using var ms = new MemoryStream();
            using (var stream = file.OpenReadStream())
                await stream.CopyToAsync(ms);

            job.BlobPath = $"emr-uploads/{programId}/{job.Id}/{file.FileName}";
            job.ResultsJson = Convert.ToBase64String(ms.ToArray());
            await _context.SaveChangesAsync();

            _ = Task.Run(() => ProcessImportInNewScopeAsync(job.Id));
        }

        return MapToDto(job);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public async Task<EmrValidationResult> ValidateCsvAsync(Stream csvStream, int programId)
    {
        var result = new EmrValidationResult();
        var effectiveMappings = await _mappingService.GetEffectiveMappingsAsync(programId);

        using var reader = new StreamReader(csvStream, leaveOpen: true);

        // ── Level 1: File structure — must have at least a header row ─────────
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Errors.Add(new EmrImportIssue
            {
                Severity = "Error",
                Message = "The CSV file is empty or has no header row.",
            });
            return result; // cannot continue
        }

        var headers = ParseCsvLine(headerLine);
        result.Headers = headers;

        // ── Level 2: Required headers present ────────────────────────────────
        foreach (var required in RequiredHeaders)
        {
            if (!headers.Contains(required, StringComparer.OrdinalIgnoreCase))
            {
                result.Errors.Add(new EmrImportIssue
                {
                    Severity = "Error",
                    Message = $"Required column '{required}' is missing from the CSV header.",
                    CsvColumn = required,
                });
            }
        }

        if (result.Errors.Count > 0)
            return result; // cannot safely continue without required columns

        // ── Level 3: Warn on unmapped columns ────────────────────────────────
        foreach (var header in headers)
        {
            if (!effectiveMappings.ContainsKey(header)
                && !header.Equals("CFF_ID", StringComparison.OrdinalIgnoreCase)
                && !header.Equals("MRN", StringComparison.OrdinalIgnoreCase))
            {
                result.Warnings.Add(new EmrImportIssue
                {
                    Severity = "Warning",
                    CsvColumn = header,
                    Message = $"Column '{header}' has no field mapping and will be ignored.",
                });
            }
        }

        // ── Level 4: Row-level validation ─────────────────────────────────────
        int rowNum = 1;
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);
            var rowDict = BuildRowDictionary(headers, values);

            rowNum++;
            result.RowCount++;

            // Required fields must not be blank
            foreach (var required in RequiredHeaders)
            {
                if (!rowDict.TryGetValue(required, out var val) || string.IsNullOrWhiteSpace(val))
                {
                    result.Errors.Add(new EmrImportIssue
                    {
                        Severity = "Error",
                        RowNumber = rowNum,
                        CsvColumn = required,
                        Message = $"Row {rowNum}: Required field '{required}' is blank.",
                    });
                }
            }

            // Field-type validation for mapped columns
            foreach (var header in headers)
            {
                if (!rowDict.TryGetValue(header, out var rawValue)) continue;
                if (string.IsNullOrWhiteSpace(rawValue)) continue;
                if (!effectiveMappings.TryGetValue(header, out var mapping)) continue;

                var issue = ValidateFieldValue(rawValue, mapping, rowNum);
                if (issue != null) result.Errors.Add(issue);
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    // ── Processing ────────────────────────────────────────────────────────────

    /// <summary>Creates a new DI scope so the background task has its own scoped services</summary>
    private async Task ProcessImportInNewScopeAsync(int importJobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mappingService = scope.ServiceProvider.GetRequiredService<IEmrMappingService>();
        var formService = scope.ServiceProvider.GetRequiredService<IFormService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImportService>>();
        await ProcessImportAsync(importJobId, context, mappingService, formService, logger);
    }

    public async Task ProcessImportAsync(int importJobId)
        => await ProcessImportInNewScopeAsync(importJobId);

    private async Task ProcessImportAsync(
        int importJobId,
        ApplicationDbContext context,
        IEmrMappingService mappingService,
        IFormService formService,
        ILogger<ImportService> logger)
    {
        var job = await context.ImportJobs.FindAsync(importJobId);
        if (job == null) return;

        job.Status = "Processing";
        await context.SaveChangesAsync();

        var errors = new List<EmrImportIssue>();
        int processed = 0;
        int errorRows = 0;

        try
        {
            if (string.IsNullOrEmpty(job.ResultsJson))
                throw new InvalidOperationException("No CSV content found for processing.");

            var csvBytes = Convert.FromBase64String(job.ResultsJson);
            using var ms = new MemoryStream(csvBytes);
            using var reader = new StreamReader(ms);

            var headerLine = await reader.ReadLineAsync() ?? "";
            var headers = ParseCsvLine(headerLine);
            var effectiveMappings = await mappingService.GetEffectiveMappingsAsync(job.ProgramId);

            int cffIdIdx  = headers.FindIndex(h => h.Equals("CFF_ID", StringComparison.OrdinalIgnoreCase));
            int mrnIdx    = headers.FindIndex(h => h.Equals("MRN",    StringComparison.OrdinalIgnoreCase));

            int rowNum = 1;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                rowNum++;

                var values = ParseCsvLine(line);
                var rowDict = BuildRowDictionary(headers, values);

                var mrnValue = mrnIdx >= 0 && mrnIdx < values.Count ? values[mrnIdx].Trim() : "";

                // Resolve patient: first try MRN crosswalk, fall back to Patient.MedicalRecordNumber
                var crosswalk = string.IsNullOrEmpty(mrnValue)
                    ? null
                    : await mappingService.ResolveMrnAsync(job.ProgramId, mrnValue);

                Patient? patient = null;
                if (crosswalk != null)
                {
                    patient = await context.Patients
                        .Include(p => p.ProgramAssignments)
                        .FirstOrDefaultAsync(p => p.Id == crosswalk.PatientId);
                }
                else if (!string.IsNullOrEmpty(mrnValue))
                {
                    // Fall back to direct MRN match on Patient record
                    patient = await context.Patients
                        .Include(p => p.ProgramAssignments)
                        .FirstOrDefaultAsync(p =>
                            p.MedicalRecordNumber == mrnValue
                            && p.ProgramAssignments.Any(a =>
                                a.ProgramId == job.ProgramId && a.Status == "Active"));
                }

                // Only patients with an active assignment to this program receive EMR data
                if (patient == null
                    || !patient.ProgramAssignments.Any(a =>
                        a.ProgramId == job.ProgramId && a.Status == "Active"))
                {
                    errors.Add(new EmrImportIssue
                    {
                        Severity = "Warning",
                        RowNumber = rowNum,
                        CsvColumn = "MRN",
                        Message = $"Row {rowNum}: No active Registry patient found for MRN '{mrnValue}' in program {job.ProgramId}. Row skipped.",
                    });
                    continue;
                }

                // Build per-form-type data dictionaries from the row
                var formDataByType = BuildFormDataFromRow(rowDict, effectiveMappings);

                // Apply each form type's data to the most-recent relevant FormSubmission
                foreach (var (formType, formData) in formDataByType)
                {
                    if (formData.Count == 0) continue;

                    var submission = await context.FormSubmissions
                        .Include(f => f.FormDefinition)
                        .Where(f =>
                            f.PatientId == patient.Id
                            && f.FormDefinition.FormType == formType
                            && (FormTypes.IsShared(formType) || f.ProgramId == job.ProgramId))
                        .OrderByDescending(f => f.UpdatedAt)
                        .FirstOrDefaultAsync();

                    if (submission == null) continue; // only update existing forms

                    // Merge EMR values into existing form data
                    var existingData = string.IsNullOrEmpty(submission.FormDataJson)
                        ? new Dictionary<string, JsonElement>()
                        : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(submission.FormDataJson)
                          ?? new Dictionary<string, JsonElement>();

                    foreach (var (field, value) in formData)
                        existingData[field] = JsonSerializer.SerializeToElement(value);

                    var mergedJson = JsonSerializer.Serialize(existingData);
                    await formService.ApplyEmrUpdateAsync(submission.Id, mergedJson);
                    processed++;
                }

                // Keep MRN crosswalk up to date
                if (!string.IsNullOrEmpty(mrnValue) && crosswalk == null)
                    await mappingService.UpsertMrnCrosswalkAsync(job.ProgramId, mrnValue, patient.Id, patient.CffId);
            }

            job.Status = "Completed";
            job.ProcessedRows = processed;
            job.ErrorRows = errorRows;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorsJson = errors.Count > 0 ? JsonSerializer.Serialize(errors) : null;
            // Clear the base64 CSV content — no longer needed once processed
            job.ResultsJson = null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process import job {ImportJobId}", importJobId);
            job.Status = "Failed";
            errors.Add(new EmrImportIssue { Severity = "Error", Message = ex.Message });
            job.ErrorsJson = JsonSerializer.Serialize(errors);
            job.CompletedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        logger.LogInformation(
            "Import job {JobId} finished: status={Status}, processed={Processed}, errors={Errors}",
            importJobId, job.Status, processed, errors.Count);
    }

    // ── Read methods ──────────────────────────────────────────────────────────

    public async Task<ImportJobDto?> GetImportJobAsync(int id)
    {
        var job = await _context.ImportJobs.FindAsync(id);
        return job == null ? null : MapToDto(job);
    }

    public async Task<ImportJobDetailDto?> GetImportJobDetailAsync(int id)
    {
        var job = await _context.ImportJobs.FindAsync(id);
        if (job == null) return null;

        var dto = new ImportJobDetailDto
        {
            Id = job.Id,
            FileName = job.FileName,
            Status = job.Status,
            ProgramId = job.ProgramId,
            TotalRows = job.TotalRows,
            ProcessedRows = job.ProcessedRows,
            ErrorRows = job.ErrorRows,
            CreatedAt = job.CreatedAt,
            CreatedBy = job.CreatedBy,
            CompletedAt = job.CompletedAt,
        };

        if (!string.IsNullOrEmpty(job.ErrorsJson))
        {
            var issues = JsonSerializer.Deserialize<List<EmrImportIssue>>(job.ErrorsJson) ?? [];
            dto.Errors = issues.Where(i => i.Severity == "Error").ToList();
            dto.Warnings = issues.Where(i => i.Severity == "Warning").ToList();
            dto.WarningCount = dto.Warnings.Count;
        }

        return dto;
    }

    public async Task<IEnumerable<ImportJobDto>> GetImportJobsByProgramAsync(int programId)
    {
        var jobs = await _context.ImportJobs
            .Where(j => j.ProgramId == programId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return jobs.Select(MapToDto);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ImportJobDto MapToDto(ImportJob job) => new()
    {
        Id = job.Id,
        FileName = job.FileName,
        Status = job.Status,
        ProgramId = job.ProgramId,
        TotalRows = job.TotalRows,
        ProcessedRows = job.ProcessedRows,
        ErrorRows = job.ErrorRows,
        CreatedAt = job.CreatedAt,
        CreatedBy = job.CreatedBy,
        CompletedAt = job.CompletedAt,
    };

    private static Dictionary<string, string> BuildRowDictionary(List<string> headers, List<string> values)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count && i < values.Count; i++)
            dict[headers[i]] = values[i].Trim();
        return dict;
    }

    /// <summary>Group row values by form type, applying transform hints</summary>
    private static Dictionary<string, Dictionary<string, object?>> BuildFormDataFromRow(
        Dictionary<string, string> rowDict,
        Dictionary<string, EmrFieldMapping> mappings)
    {
        var result = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (column, rawValue) in rowDict)
        {
            if (string.IsNullOrWhiteSpace(rawValue)) continue;
            if (!mappings.TryGetValue(column, out var mapping)) continue;
            if (!mapping.IsActive) continue;

            if (!result.TryGetValue(mapping.FormType, out var formData))
            {
                formData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                result[mapping.FormType] = formData;
            }

            formData[mapping.FieldPath] = ApplyTransform(rawValue, mapping);
        }

        return result;
    }

    private static object? ApplyTransform(string raw, EmrFieldMapping mapping)
    {
        var hint = mapping.TransformHint ?? "";

        return mapping.DataType switch
        {
            "integer" => int.TryParse(raw, out var i) ? i : (object?)null,
            "decimal" => decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null,
            "boolean" => raw.Equals("Y", StringComparison.OrdinalIgnoreCase)
                      || raw.Equals("true", StringComparison.OrdinalIgnoreCase)
                      || raw == "1",
            "date" => DateTime.TryParse(raw, out var dt) ? dt.ToString("o") : (object?)null,
            _ => hint switch
            {
                "uppercase" => raw.ToUpperInvariant(),
                "lowercase" => raw.ToLowerInvariant(),
                _ => raw,
            },
        };
    }

    private static EmrImportIssue? ValidateFieldValue(string raw, EmrFieldMapping mapping, int rowNum)
    {
        var valid = mapping.DataType switch
        {
            "integer" => int.TryParse(raw, out _),
            "decimal" => decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
            "boolean" => raw.Equals("Y", StringComparison.OrdinalIgnoreCase)
                      || raw.Equals("N", StringComparison.OrdinalIgnoreCase)
                      || raw.Equals("true", StringComparison.OrdinalIgnoreCase)
                      || raw.Equals("false", StringComparison.OrdinalIgnoreCase)
                      || raw == "0" || raw == "1",
            "date" => DateTime.TryParse(raw, out _),
            _ => true,
        };

        if (!valid)
            return new EmrImportIssue
            {
                Severity = "Error",
                RowNumber = rowNum,
                CsvColumn = mapping.CsvColumnName,
                FieldPath = mapping.FieldPath,
                Message = $"Row {rowNum}: Value '{raw}' in column '{mapping.CsvColumnName}' is not a valid {mapping.DataType}.",
            };

        return null;
    }

    /// <summary>Minimal RFC 4180-compliant CSV line parser</summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
