using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IReportingService
{
    // Saved Reports
    Task<IEnumerable<SavedReportDto>> GetSavedReportsAsync(string? scope, int? programId, string? ownerEmail);
    Task<SavedReportDto?> GetSavedReportByIdAsync(int id);
    Task<SavedReportDto> CreateSavedReportAsync(CreateSavedReportDto dto, string ownerEmail);
    Task<SavedReportDto?> UpdateSavedReportAsync(int id, UpdateSavedReportDto dto);
    Task<bool> DeleteSavedReportAsync(int id);

    // Execution
    Task<ReportResultDto> ExecuteReportAsync(ExecuteReportDto dto, string executedBy);

    // Pre-defined Reports
    Task<ReportResultDto> RunIncompleteRecordsReportAsync(int programId, int reportingYear, string executedBy);
    Task<ReportResultDto> RunPatientsDueVisitReportAsync(int programId, string executedBy);
    Task<ReportResultDto> RunDiabetesTestingReportAsync(int programId, string executedBy);

    // Admin Reports
    Task<ReportResultDto> RunProgramListReportAsync(string executedBy);
    Task<ReportResultDto> RunMergeReportAsync(string executedBy);
    Task<ReportResultDto> RunTransferReportAsync(string executedBy);
    Task<ReportResultDto> RunFileUploadReportAsync(string executedBy);

    // Audit Reports
    Task<ReportResultDto> RunUserManagementAuditAsync(DateTime startDate, DateTime endDate, string executedBy);
    Task<ReportResultDto> RunDownloadAuditAsync(DateTime startDate, DateTime endDate, string executedBy);

    // Download
    Task<byte[]> GenerateDownloadAsync(int executionId, string format, string userEmail, string? userRole, int? programId);
}

public class ReportingService : IReportingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(ApplicationDbContext context, ILogger<ReportingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── Saved Reports CRUD ───────────────────────────────────────

    public async Task<IEnumerable<SavedReportDto>> GetSavedReportsAsync(
        string? scope, int? programId, string? ownerEmail)
    {
        var query = _context.SavedReports.Where(r => r.IsActive).AsQueryable();
        if (!string.IsNullOrEmpty(scope)) query = query.Where(r => r.Scope == scope);
        if (programId.HasValue) query = query.Where(r => r.ProgramId == programId);
        if (!string.IsNullOrEmpty(ownerEmail)) query = query.Where(r => r.OwnerEmail == ownerEmail);

        var reports = await query.OrderBy(r => r.Title).ToListAsync();
        return reports.Select(MapToDto);
    }

    public async Task<SavedReportDto?> GetSavedReportByIdAsync(int id)
    {
        var report = await _context.SavedReports.FindAsync(id);
        return report == null ? null : MapToDto(report);
    }

    public async Task<SavedReportDto> CreateSavedReportAsync(CreateSavedReportDto dto, string ownerEmail)
    {
        var report = new SavedReport
        {
            Title = dto.Title,
            Description = dto.Description,
            Scope = dto.Scope,
            QueryDefinitionJson = dto.QueryDefinitionJson,
            ReportType = "custom",
            OwnerEmail = ownerEmail,
            ProgramId = dto.ProgramId,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.SavedReports.Add(report);
        await _context.SaveChangesAsync();
        return MapToDto(report);
    }

    public async Task<SavedReportDto?> UpdateSavedReportAsync(int id, UpdateSavedReportDto dto)
    {
        var report = await _context.SavedReports.FindAsync(id);
        if (report == null) return null;

        report.Title = dto.Title;
        report.Description = dto.Description;
        if (dto.QueryDefinitionJson != null) report.QueryDefinitionJson = dto.QueryDefinitionJson;
        report.Version++;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(report);
    }

    public async Task<bool> DeleteSavedReportAsync(int id)
    {
        var report = await _context.SavedReports.FindAsync(id);
        if (report == null) return false;
        report.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Execution ────────────────────────────────────────────────

    public async Task<ReportResultDto> ExecuteReportAsync(ExecuteReportDto dto, string executedBy)
    {
        var sw = Stopwatch.StartNew();

        // For now, execute a basic patient query based on program
        var query = _context.Patients
            .Include(p => p.CareProgram)
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .Where(p => !p.ConsentWithdrawn && p.Status != "Merged")
            .AsQueryable();

        if (dto.ProgramId.HasValue)
        {
            query = query.Where(p =>
                p.ProgramAssignments.Any(pa =>
                    pa.ProgramId == dto.ProgramId.Value && pa.Status == "Active"));
        }

        var patients = await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(1000).ToListAsync();

        var rows = patients.Select(p => new Dictionary<string, object?>
        {
            ["cffId"] = p.CffId,
            ["firstName"] = p.FirstName,
            ["lastName"] = p.LastName,
            ["dateOfBirth"] = p.DateOfBirth.ToString("o"),
            ["sex"] = p.BiologicalSexAtBirth ?? p.Gender,
            ["diagnosis"] = p.Diagnosis,
            ["vitalStatus"] = p.VitalStatus,
            ["program"] = p.CareProgram?.Name,
        }).ToList();

        sw.Stop();

        var title = "Custom Report";
        if (dto.SavedReportId.HasValue)
        {
            var saved = await _context.SavedReports.FindAsync(dto.SavedReportId.Value);
            if (saved != null)
            {
                title = saved.Title;
                saved.LastExecutedAt = DateTime.UtcNow;
            }
        }

        var execution = new ReportExecution
        {
            SavedReportId    = dto.SavedReportId,
            ReportType       = dto.ReportType ?? "custom",
            ExecutedBy       = executedBy,
            ProgramId        = dto.ProgramId,
            ResultDataJson   = JsonSerializer.Serialize(rows),
            RecordCount      = rows.Count,
            ExecutionTimeMs  = (int)sw.ElapsedMilliseconds,
            ExecutedAt       = DateTime.UtcNow,
            OutputMode       = "screen",
            Status           = "Success",
            // Parameters: log filter keys only — no PHI values
            ParametersJson   = JsonSerializer.Serialize(new { programId = dto.ProgramId, reportType = dto.ReportType }),
        };

        _context.ReportExecutions.Add(execution);
        await _context.SaveChangesAsync();

        return new ReportResultDto
        {
            ExecutionId = execution.Id,
            ReportTitle = title,
            ReportType = dto.ReportType ?? "custom",
            ExecutedBy = executedBy,
            ExecutedAt = execution.ExecutedAt,
            RecordCount = rows.Count,
            ExecutionTimeMs = (int)sw.ElapsedMilliseconds,
            Columns = new List<string> { "cffId", "firstName", "lastName", "dateOfBirth", "sex", "diagnosis", "vitalStatus", "program" },
            Rows = rows,
        };
    }

    // ── Pre-defined Reports ──────────────────────────────────────

    public async Task<ReportResultDto> RunIncompleteRecordsReportAsync(
        int programId, int reportingYear, string executedBy)
    {
        var sw = Stopwatch.StartNew();

        var incompleteForms = await _context.FormSubmissions
            .Include(fs => fs.FormDefinition)
            .Include(fs => fs.Patient)
            .Where(fs =>
                fs.ProgramId == programId &&
                fs.CompletionStatus == "Incomplete" &&
                fs.LockStatus == "Unlocked")
            .OrderBy(fs => fs.Patient.CffId)
            .ThenByDescending(fs => fs.UpdatedAt)
            .ToListAsync();

        var rows = incompleteForms.Select(fs => new Dictionary<string, object?>
        {
            ["cffId"] = fs.Patient.CffId,
            ["firstName"] = fs.Patient.FirstName,
            ["lastName"] = fs.Patient.LastName,
            ["formType"] = fs.FormDefinition.Name,
            ["status"] = fs.CompletionStatus,
            ["lastModified"] = fs.UpdatedAt.ToString("o"),
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("Incomplete Records", "incomplete_records", executedBy, rows,
            new[] { "cffId", "firstName", "lastName", "formType", "status", "lastModified" }, sw,
            JsonSerializer.Serialize(new { programId, reportingYear }));
    }

    public async Task<ReportResultDto> RunPatientsDueVisitReportAsync(int programId, string executedBy)
    {
        var sw = Stopwatch.StartNew();

        var patients = await _context.Patients
            .Include(p => p.ProgramAssignments).ThenInclude(pa => pa.Program)
            .Where(p => p.ProgramAssignments.Any(pa => pa.ProgramId == programId && pa.Status == "Active")
                     && !p.ConsentWithdrawn && p.Status == "Active")
            .OrderBy(p => p.LastSeenInProgram ?? DateTime.MinValue)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var rows = patients.Select(p =>
        {
            var daysSinceSeen = p.LastSeenInProgram.HasValue
                ? (int)(now - p.LastSeenInProgram.Value).TotalDays
                : 9999;
            return new Dictionary<string, object?>
            {
                ["cffId"] = p.CffId,
                ["firstName"] = p.FirstName,
                ["lastName"] = p.LastName,
                ["lastSeenDate"] = p.LastSeenInProgram?.ToString("o"),
                ["daysSinceLastSeen"] = daysSinceSeen,
                ["overdue180"] = daysSinceSeen > 180,
                ["overdue2Years"] = daysSinceSeen > 730,
                ["diagnosis"] = p.Diagnosis,
            };
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("Patients Due Visit", "due_visit", executedBy, rows,
            new[] { "cffId", "firstName", "lastName", "lastSeenDate", "daysSinceLastSeen", "overdue180", "overdue2Years", "diagnosis" }, sw,
            JsonSerializer.Serialize(new { programId }));
    }

    public async Task<ReportResultDto> RunDiabetesTestingReportAsync(int programId, string executedBy)
    {
        var sw = Stopwatch.StartNew();

        var patients = await _context.Patients
            .Include(p => p.ProgramAssignments)
            .Where(p => p.ProgramAssignments.Any(pa => pa.ProgramId == programId && pa.Status == "Active")
                     && !p.ConsentWithdrawn && p.Status == "Active")
            .OrderBy(p => p.LastName)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var rows = patients
            .Where(p => {
                var age = (now - p.DateOfBirth).TotalDays / 365.25;
                return age >= 10;
            })
            .Select(p => new Dictionary<string, object?>
            {
                ["cffId"] = p.CffId,
                ["firstName"] = p.FirstName,
                ["lastName"] = p.LastName,
                ["dateOfBirth"] = p.DateOfBirth.ToString("o"),
                ["age"] = (int)((now - p.DateOfBirth).TotalDays / 365.25),
                ["diagnosis"] = p.Diagnosis,
            }).ToList();

        sw.Stop();
        return await BuildResultAsync("Diabetes Testing", "diabetes_testing", executedBy, rows,
            new[] { "cffId", "firstName", "lastName", "dateOfBirth", "age", "diagnosis" }, sw,
            JsonSerializer.Serialize(new { programId }));
    }

    // ── Admin Reports ────────────────────────────────────────────

    public async Task<ReportResultDto> RunProgramListReportAsync(string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var programs = await _context.CarePrograms.OrderBy(p => p.Name).ToListAsync();

        var rows = programs.Select(p => new Dictionary<string, object?>
        {
            ["programId"] = p.ProgramId,
            ["name"] = p.Name,
            ["type"] = p.ProgramType,
            ["city"] = p.City,
            ["state"] = p.State,
            ["isActive"] = p.IsActive,
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("CF Care Program List", "program_list", executedBy, rows,
            new[] { "programId", "name", "type", "city", "state", "isActive" }, sw);
    }

    public async Task<ReportResultDto> RunMergeReportAsync(string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var merges = await _context.AuditLogs
            .Where(a => a.Action == "Merge")
            .OrderByDescending(a => a.Timestamp)
            .Take(500)
            .ToListAsync();

        var rows = merges.Select(a => new Dictionary<string, object?>
        {
            ["entityId"] = a.EntityId,
            ["action"] = a.Action,
            ["userEmail"] = a.UserEmail,
            ["timestamp"] = a.Timestamp.ToString("o"),
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("Duplicate Record Merge Report", "merge_report", executedBy, rows,
            new[] { "entityId", "action", "userEmail", "timestamp" }, sw);
    }

    public async Task<ReportResultDto> RunTransferReportAsync(string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var transfers = await _context.AuditLogs
            .Where(a => a.EntityType == "PatientProgramAssignment")
            .OrderByDescending(a => a.Timestamp)
            .Take(500)
            .ToListAsync();

        var rows = transfers.Select(a => new Dictionary<string, object?>
        {
            ["entityId"] = a.EntityId,
            ["action"] = a.Action,
            ["userEmail"] = a.UserEmail,
            ["timestamp"] = a.Timestamp.ToString("o"),
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("Patient Transfer Report", "transfer_report", executedBy, rows,
            new[] { "entityId", "action", "userEmail", "timestamp" }, sw);
    }

    public async Task<ReportResultDto> RunFileUploadReportAsync(string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var files = await _context.PatientFiles
            .Include(f => f.Program)
            .OrderByDescending(f => f.UploadedAt)
            .Take(500)
            .ToListAsync();

        var rows = files.Select(f => new Dictionary<string, object?>
        {
            ["fileName"] = f.OriginalFileName,
            ["fileType"] = f.FileType,
            ["programName"] = f.Program.Name,
            ["uploadedBy"] = f.UploadedBy,
            ["uploadedAt"] = f.UploadedAt.ToString("o"),
            ["fileSize"] = f.FileSize,
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("File Upload Report", "file_upload_report", executedBy, rows,
            new[] { "fileName", "fileType", "programName", "uploadedBy", "uploadedAt", "fileSize" }, sw);
    }

    // ── Audit Reports ────────────────────────────────────────────

    public async Task<ReportResultDto> RunUserManagementAuditAsync(
        DateTime startDate, DateTime endDate, string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var events = await _context.AuditLogs
            .Where(a => a.EntityType == "User" && a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        var rows = events.Select(a => new Dictionary<string, object?>
        {
            ["action"] = a.Action,
            ["userEmail"] = a.UserEmail,
            ["timestamp"] = a.Timestamp.ToString("o"),
            ["entityId"] = a.EntityId,
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("User Management Audit", "user_management_audit", executedBy, rows,
            new[] { "action", "userEmail", "timestamp", "entityId" }, sw,
            JsonSerializer.Serialize(new { startDate, endDate }));
    }

    public async Task<ReportResultDto> RunDownloadAuditAsync(
        DateTime startDate, DateTime endDate, string executedBy)
    {
        var sw = Stopwatch.StartNew();
        var logs = await _context.ReportDownloadLogs
            .Where(d => d.DownloadedAt >= startDate && d.DownloadedAt <= endDate)
            .OrderByDescending(d => d.DownloadedAt)
            .ToListAsync();

        var rows = logs.Select(d => new Dictionary<string, object?>
        {
            ["reportName"] = d.ReportName,
            ["reportType"] = d.ReportType,
            ["userEmail"] = d.UserEmail,
            ["patientCount"] = d.PatientCount,
            ["format"] = d.Format,
            ["downloadedAt"] = d.DownloadedAt.ToString("o"),
        }).ToList();

        sw.Stop();
        return await BuildResultAsync("Download Details Audit", "download_audit", executedBy, rows,
            new[] { "reportName", "reportType", "userEmail", "patientCount", "format", "downloadedAt" }, sw,
            JsonSerializer.Serialize(new { startDate, endDate }));
    }

    // ── Download ─────────────────────────────────────────────────

    public async Task<byte[]> GenerateDownloadAsync(
        int executionId, string format, string userEmail, string? userRole, int? programId)
    {
        var execution = await _context.ReportExecutions.FindAsync(executionId)
            ?? throw new ArgumentException("Report execution not found");

        var rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(execution.ResultDataJson) ?? new();

        // Log the download and update execution metadata (12-003)
        _context.ReportDownloadLogs.Add(new ReportDownloadLog
        {
            ReportName = execution.ReportType,
            ReportType = execution.ReportType,
            ProgramId = programId,
            UserEmail = userEmail,
            UserRole = userRole,
            PatientCount = rows.Count,
            Format = format,
            DownloadedAt = DateTime.UtcNow,
        });

        // Update execution record with download metadata
        execution.OutputMode = "download";
        execution.FileFormat = format;

        await _context.SaveChangesAsync();

        // Generate CSV
        if (rows.Count == 0) return Encoding.UTF8.GetBytes("No data");

        var sb = new StringBuilder();
        var headers = rows[0].Keys.ToList();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

        foreach (var row in rows)
        {
            var values = headers.Select(h => row.TryGetValue(h, out var val)
                ? EscapeCsv(val.ToString()) : "");
            sb.AppendLine(string.Join(",", values));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        // Record file size on the execution (12-003)
        execution.FileSizeBytes = bytes.Length;
        await _context.SaveChangesAsync();

        return bytes;
    }

    // ── Helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Persists a ReportExecution record and returns the result DTO.
    /// Used by all pre-defined report methods to satisfy 12-003 logging requirements.
    /// </summary>
    private async Task<ReportResultDto> BuildResultAsync(
        string title, string type, string executedBy,
        List<Dictionary<string, object?>> rows, string[] columns, Stopwatch sw,
        string? parametersJson = null)
    {
        var execution = new ReportExecution
        {
            ReportType      = type,
            ExecutedBy      = executedBy,
            ResultDataJson  = JsonSerializer.Serialize(rows),
            RecordCount     = rows.Count,
            ExecutionTimeMs = (int)sw.ElapsedMilliseconds,
            ExecutedAt      = DateTime.UtcNow,
            OutputMode      = "screen",
            Status          = "Success",
            ParametersJson  = parametersJson,
        };
        _context.ReportExecutions.Add(execution);
        await _context.SaveChangesAsync();

        return new ReportResultDto
        {
            ExecutionId     = execution.Id,
            ReportTitle     = title,
            ReportType      = type,
            ExecutedBy      = executedBy,
            ExecutedAt      = execution.ExecutedAt,
            RecordCount     = rows.Count,
            ExecutionTimeMs = (int)sw.ElapsedMilliseconds,
            Columns         = columns.ToList(),
            Rows            = rows,
        };
    }

    private static SavedReportDto MapToDto(SavedReport r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        Scope = r.Scope,
        ReportType = r.ReportType,
        OwnerEmail = r.OwnerEmail,
        ProgramId = r.ProgramId,
        Version = r.Version,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        LastExecutedAt = r.LastExecutedAt,
    };

    private static string EscapeCsv(string? value)
    {
        if (value == null) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
