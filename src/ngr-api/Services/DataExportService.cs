using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IDataExportService
{
    Task<byte[]> ExecuteExportAsync(DataExportRequestDto request, string executedBy);
    Task<IEnumerable<SavedDownloadDefinitionDto>> GetSavedDefinitionsAsync(string ownerEmail, int? programId);
    Task<SavedDownloadDefinitionDto?> GetDefinitionByIdAsync(int id);
    Task<SavedDownloadDefinitionDto> CreateDefinitionAsync(CreateSavedDownloadDto dto, string ownerEmail);
    Task<SavedDownloadDefinitionDto?> UpdateDefinitionAsync(int id, UpdateSavedDownloadDto dto);
    Task<bool> DeleteDefinitionAsync(int id);
}

public class DataExportService : IDataExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(ApplicationDbContext context, ILogger<DataExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> ExecuteExportAsync(DataExportRequestDto request, string executedBy)
    {
        var formTypesToExport = request.FormTypes.Contains("ALL")
            ? FormTypes.AllFormTypes.ToList()
            : request.FormTypes;

        // Query form submissions filtered by program, type, date range, completeness
        var query = _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Include(fs => fs.Patient)
            .Include(fs => fs.Program)
            .Where(fs => fs.ProgramId == request.ProgramId)
            .Where(fs => !fs.Patient.ConsentWithdrawn)
            .AsQueryable();

        if (formTypesToExport.Count > 0)
            query = query.Where(fs => formTypesToExport.Contains(fs.FormDefinition.FormType));

        if (request.DateFrom.HasValue)
            query = query.Where(fs => fs.UpdatedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(fs => fs.UpdatedAt <= request.DateTo.Value);

        if (request.CompletenessFilter == "complete_only")
            query = query.Where(fs => fs.CompletionStatus == "Complete");

        var submissions = await query.OrderBy(fs => fs.Patient.CffId).ToListAsync();

        // Group by form type and generate one CSV per type
        var grouped = submissions.GroupBy(fs => fs.FormDefinition.FormType);

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var group in grouped)
            {
                var formType = group.Key;
                var csv = GenerateCsv(group.ToList(), request.OutputFormat);
                var fileName = $"{formType}_{request.ProgramId}_{DateTime.UtcNow:yyyyMMdd}.csv";

                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                var bytes = Encoding.UTF8.GetBytes(csv);
                await entryStream.WriteAsync(bytes);
            }

            // Add a manifest file
            var manifest = $"Export Date: {DateTime.UtcNow:O}\n" +
                           $"Program ID: {request.ProgramId}\n" +
                           $"Executed By: {executedBy}\n" +
                           $"Completeness: {request.CompletenessFilter}\n" +
                           $"Format: {request.OutputFormat}\n" +
                           $"Form Types: {string.Join(", ", formTypesToExport)}\n" +
                           $"Total Records: {submissions.Count}\n";

            var manifestEntry = archive.CreateEntry("_manifest.txt", CompressionLevel.Optimal);
            using var manifestStream = manifestEntry.Open();
            await manifestStream.WriteAsync(Encoding.UTF8.GetBytes(manifest));
        }

        // Log the download
        _context.ReportDownloadLogs.Add(new ReportDownloadLog
        {
            ReportName = "Raw Data Export",
            ReportType = "data_export",
            ProgramId = request.ProgramId,
            UserEmail = executedBy,
            PatientCount = submissions.Select(s => s.PatientId).Distinct().Count(),
            Format = "zip",
            DownloadedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        _logger.LogInformation("Data export for program {ProgramId}: {Count} forms exported by {User}",
            request.ProgramId, submissions.Count, executedBy);

        return zipStream.ToArray();
    }

    // ── Saved Definitions CRUD ───────────────────────────────────

    public async Task<IEnumerable<SavedDownloadDefinitionDto>> GetSavedDefinitionsAsync(
        string ownerEmail, int? programId)
    {
        var query = _context.SavedDownloadDefinitions.AsQueryable();
        if (!string.IsNullOrEmpty(ownerEmail))
            query = query.Where(d => d.OwnerEmail == ownerEmail);
        if (programId.HasValue)
            query = query.Where(d => d.ProgramId == programId.Value);

        var defs = await query.OrderBy(d => d.Name).ToListAsync();
        return defs.Select(MapToDto);
    }

    public async Task<SavedDownloadDefinitionDto?> GetDefinitionByIdAsync(int id)
    {
        var def = await _context.SavedDownloadDefinitions.FindAsync(id);
        return def == null ? null : MapToDto(def);
    }

    public async Task<SavedDownloadDefinitionDto> CreateDefinitionAsync(
        CreateSavedDownloadDto dto, string ownerEmail)
    {
        var def = new SavedDownloadDefinition
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerEmail = ownerEmail,
            ProgramId = dto.ProgramId,
            ParametersJson = dto.ParametersJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.SavedDownloadDefinitions.Add(def);
        await _context.SaveChangesAsync();
        return MapToDto(def);
    }

    public async Task<SavedDownloadDefinitionDto?> UpdateDefinitionAsync(int id, UpdateSavedDownloadDto dto)
    {
        var def = await _context.SavedDownloadDefinitions.FindAsync(id);
        if (def == null) return null;

        def.Name = dto.Name;
        def.Description = dto.Description;
        if (dto.ParametersJson != null) def.ParametersJson = dto.ParametersJson;
        def.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(def);
    }

    public async Task<bool> DeleteDefinitionAsync(int id)
    {
        var def = await _context.SavedDownloadDefinitions.FindAsync(id);
        if (def == null) return false;
        _context.SavedDownloadDefinitions.Remove(def);
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static string GenerateCsv(List<FormSubmission> submissions, string format)
    {
        if (submissions.Count == 0) return "No data\n";

        var sb = new StringBuilder();

        // Header
        sb.AppendLine("CFF_ID,Patient_Name,Form_Type,Completion_Status,Lock_Status,Created_Date,Modified_Date,Modified_By,Form_Data");

        foreach (var fs in submissions)
        {
            var cffId = fs.Patient.CffId;
            var name = $"{fs.Patient.LastName}, {fs.Patient.FirstName}";
            var formType = fs.FormDefinition.FormType;

            // For "descriptive" format, we'd translate coded values to labels
            // For now, output the raw JSON data
            var formData = fs.FormDataJson.Replace("\"", "\"\"");

            sb.AppendLine(string.Join(",",
                cffId,
                EscapeCsv(name),
                formType,
                fs.CompletionStatus,
                fs.LockStatus,
                fs.CreatedAt.ToString("o"),
                fs.UpdatedAt.ToString("o"),
                EscapeCsv(fs.UpdatedBy),
                EscapeCsv(formData)));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (value == null) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static SavedDownloadDefinitionDto MapToDto(SavedDownloadDefinition d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        OwnerEmail = d.OwnerEmail,
        ProgramId = d.ProgramId,
        ParametersJson = d.ParametersJson,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
        LastExecutedAt = d.LastExecutedAt,
    };
}
