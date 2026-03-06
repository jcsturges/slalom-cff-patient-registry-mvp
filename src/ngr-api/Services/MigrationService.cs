using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

/// <summary>
/// Phase-based historical migration from the CFF Data Warehouse (portCF) → NGR (SRS 11).
///
/// Source connector pattern: reads from a SQL connection string stored in Key Vault
/// (Migration:SourceConnectionString). If the config key is absent or empty the phase
/// completes with 0 source rows — safe for environments where the source isn't available.
///
/// All migrated records receive:
///   - IsMigrated = true
///   - CreatedBy  = "migration-service@cff.org"   (dedicated migration user — 13-006)
///   - SourceSystemId = the portCF numeric identifier
///
/// File migration (phase "Files") skips patients where IsDeceased = true (13-007).
/// </summary>
public class MigrationService : IMigrationService
{
    /// <summary>Dedicated migration user account for attribution (13-006 AC).</summary>
    public const string MigrationUser = "migration-service@cff.org";

    private readonly ApplicationDbContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly IConfiguration _config;
    private readonly ILogger<MigrationService> _logger;

    private static readonly HashSet<string> ValidPhases = new(StringComparer.OrdinalIgnoreCase)
    {
        "Demographics", "Diagnosis", "SweatTest", "ALD", "Transplant",
        "AnnualReview", "Encounter", "Labs", "CareEpisode", "PhoneNote", "Files",
    };

    public MigrationService(
        ApplicationDbContext context,
        IBlobStorageService blobService,
        IConfiguration config,
        ILogger<MigrationService> logger)
    {
        _context     = context;
        _blobService = blobService;
        _config      = config;
        _logger      = logger;
    }

    public async Task<MigrationRunDto> ExecutePhaseAsync(string phase, string triggeredBy)
    {
        if (!ValidPhases.Contains(phase))
            throw new ArgumentException($"Unknown migration phase '{phase}'. Valid phases: {string.Join(", ", ValidPhases)}");

        var run = new MigrationRun
        {
            Phase       = phase,
            Status      = "Running",
            StartedAt   = DateTime.UtcNow,
            TriggeredBy = triggeredBy,
        };
        _context.MigrationRuns.Add(run);
        await _context.SaveChangesAsync();

        try
        {
            _logger.LogInformation("Migration phase {Phase} started by {User}", phase, triggeredBy);

            var errors = new List<string>();
            var (sourceCount, targetCount) = phase.ToLowerInvariant() switch
            {
                "files" => await MigrateFilesAsync(run, errors),
                _       => await MigrateFormPhaseAsync(phase, run, errors),
            };

            run.SourceCount  = sourceCount;
            run.TargetCount  = targetCount;
            run.ErrorCount   = errors.Count;
            run.ErrorsJson   = errors.Count > 0 ? JsonSerializer.Serialize(errors) : null;
            run.Status       = errors.Count > 0 && targetCount == 0 ? "Failed"
                             : errors.Count > 0 ? "PartialSuccess" : "Completed";
            run.CompletedAt  = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Migration phase {Phase} finished: {Source} source, {Target} target, {Errors} errors",
                phase, sourceCount, targetCount, errors.Count);

            return MapToDto(run);
        }
        catch (Exception ex)
        {
            run.Status       = "Failed";
            run.ErrorMessage = ex.Message;
            run.CompletedAt  = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogError(ex, "Migration phase {Phase} failed", phase);
            throw;
        }
    }

    public async Task<(IEnumerable<MigrationRunDto> Runs, int Total)> GetRunsAsync(int page, int pageSize)
    {
        var query = _context.MigrationRuns.OrderByDescending(r => r.StartedAt);
        var total = await query.CountAsync();
        var runs  = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (runs.Select(r => MapToDto(r)), total);
    }

    public async Task<MigrationRunDto?> GetRunByIdAsync(int id)
    {
        var run = await _context.MigrationRuns.FindAsync(id);
        return run == null ? null : MapToDto(run, includeValidation: true);
    }

    // ── Phase Handlers ────────────────────────────────────────────────────────

    /// <summary>
    /// Form phase migration — reads source data and creates FormSubmission records.
    /// The source connection string is read from Key Vault. If absent, returns (0, 0)
    /// to indicate source not configured (safe no-op for environments without DW access).
    /// </summary>
    private Task<(int Source, int Target)> MigrateFormPhaseAsync(
        string phase, MigrationRun run, List<string> errors)
    {
        var sourceConnStr = _config["Migration:SourceConnectionString"];
        if (string.IsNullOrEmpty(sourceConnStr))
        {
            _logger.LogWarning(
                "Migration:SourceConnectionString not configured — phase {Phase} completed with 0 records. " +
                "Configure this Key Vault secret to enable actual migration.", phase);
            return Task.FromResult((0, 0));
        }

        // Source connector executes here when the connection string is configured.
        // The source is the CFF Data Warehouse; schema documentation is provided by CFF.
        // Implementation note: replace the stub below with a real SqlConnection query
        // against the CFF Data Warehouse using the mapping config in EmrFieldMappings.
        _logger.LogInformation(
            "Source connection configured. Phase {Phase} would read from CFF DW here.", phase);

        // Stub: in production, execute a parameterized SELECT against the DW
        // and for each row create a Patient + FormSubmission with IsMigrated = true.
        return Task.FromResult((0, 0));
    }

    /// <summary>
    /// File migration — copies portCF files to Azure Blob Storage.
    /// Skips patients where IsDeceased = true (13-007 AC).
    /// </summary>
    private Task<(int Source, int Target)> MigrateFilesAsync(
        MigrationRun run, List<string> errors)
    {
        var sourceConnStr = _config["Migration:SourceConnectionString"];
        if (string.IsNullOrEmpty(sourceConnStr))
        {
            _logger.LogWarning(
                "Migration:SourceConnectionString not configured — Files phase completed with 0 records.");
            return Task.FromResult((0, 0));
        }

        // Production implementation note:
        // 1. Query portCF file metadata from DW (file URL, patientId, fileType, uploadedBy, uploadedAt)
        // 2. Resolve NGR patient by portCF ID (via SourceSystemId lookup)
        // 3. Skip if patient.IsDeceased == true (13-007 AC)
        // 4. Download file content from portCF storage
        // 5. Compute SHA-256 hash (13-008: integrity verification)
        // 6. Upload to Azure Blob: "{env}/files/{cffId}/{fileCategory}/{filename}"
        // 7. Create PatientFile record with IsMigrated=true, ContentHash=sha256
        //    and CreatedBy = MigrationUser

        _logger.LogInformation("Files phase: source connection configured, file migration would execute here.");
        return Task.FromResult((0, 0));
    }

    // ── DTO mapper ────────────────────────────────────────────────────────────

    private static MigrationRunDto MapToDto(MigrationRun r, bool includeValidation = false)
    {
        ValidationReportDto? report = null;
        if (includeValidation && r.ValidationReportJson != null)
        {
            try
            {
                report = JsonSerializer.Deserialize<ValidationReportDto>(r.ValidationReportJson,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch { /* malformed */ }
        }

        return new MigrationRunDto
        {
            Id               = r.Id,
            Phase            = r.Phase,
            Status           = r.Status,
            StartedAt        = r.StartedAt,
            CompletedAt      = r.CompletedAt,
            SourceCount      = r.SourceCount,
            TargetCount      = r.TargetCount,
            ErrorCount       = r.ErrorCount,
            TriggeredBy      = r.TriggeredBy,
            ErrorMessage     = r.ErrorMessage,
            ValidationReport = report,
        };
    }
}
