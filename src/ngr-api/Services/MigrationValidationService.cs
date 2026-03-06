using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;

namespace NgrApi.Services;

/// <summary>
/// Post-migration validation service (13-008).
/// Runs after a migration phase to verify:
/// - Record count reconciliation (source vs target)
/// - Migrated records have required fields populated (spot-check)
/// - IsMigrated flag is set on all migrated rows
/// - Referential integrity (patients referenced by forms exist)
///
/// Generates a machine-readable ValidationReport stored in MigrationRun.ValidationReportJson.
/// </summary>
public interface IMigrationValidationService
{
    /// <summary>
    /// Run all validation checks for a given migration run and persist the report.
    /// </summary>
    Task<ValidationReportDto> ValidateRunAsync(int migrationRunId);
}

public class MigrationValidationService : IMigrationValidationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MigrationValidationService> _logger;

    public MigrationValidationService(
        ApplicationDbContext context,
        ILogger<MigrationValidationService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task<ValidationReportDto> ValidateRunAsync(int migrationRunId)
    {
        var run = await _context.MigrationRuns.FindAsync(migrationRunId)
            ?? throw new ArgumentException($"Migration run {migrationRunId} not found.");

        var checks = new List<ValidationCheckDto>();

        // ── Count reconciliation ─────────────────────────────────────────────
        checks.Add(await CheckMigratedRecordCountAsync(run.Phase, run.TargetCount));

        // ── IsMigrated flag verification ─────────────────────────────────────
        checks.Add(await CheckIsMigratedFlagAsync(run.Phase));

        // ── Required field spot-checks ───────────────────────────────────────
        checks.AddRange(await SpotCheckRequiredFieldsAsync(run.Phase));

        // ── Referential integrity ────────────────────────────────────────────
        checks.AddRange(await CheckReferentialIntegrityAsync(run.Phase));

        var passed = checks.Count(c => c.Status == "Pass");
        var failed = checks.Count(c => c.Status == "Fail");

        var report = new ValidationReportDto
        {
            GeneratedAt   = DateTime.UtcNow,
            OverallStatus = failed == 0 ? "Pass" : "Fail",
            TotalChecks   = checks.Count,
            PassedChecks  = passed,
            FailedChecks  = failed,
            Checks        = checks,
        };

        // Persist the report
        run.ValidationReportJson = JsonSerializer.Serialize(report,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Migration validation run {RunId}: {Passed}/{Total} checks passed",
            migrationRunId, passed, checks.Count);

        return report;
    }

    // ── Check implementations ─────────────────────────────────────────────────

    private async Task<ValidationCheckDto> CheckMigratedRecordCountAsync(string phase, int expectedCount)
    {
        var actualCount = phase.ToLowerInvariant() switch
        {
            "files"       => await _context.PatientFiles.CountAsync(f => f.IsMigrated),
            "demographics" or "diagnosis" or "sweattest" or "ald" or "transplant"
                or "annualreview" or "encounter" or "labs" or "careepisode" or "phonenote"
                          => await _context.FormSubmissions.CountAsync(f =>
                                 f.FormDefinition.FormType.ToLower().Contains(phase.ToLower())),
            _             => await _context.Patients.CountAsync(p => p.IsMigrated),
        };

        // If no source records were available (source not configured), pass trivially
        var status = expectedCount == 0 || actualCount >= expectedCount ? "Pass" : "Fail";

        return new ValidationCheckDto
        {
            CheckName     = "RecordCountReconciliation",
            EntityType    = phase,
            Status        = status,
            ExpectedCount = expectedCount,
            ActualCount   = actualCount,
            Detail        = status == "Pass"
                ? $"Target has {actualCount} migrated records (expected {expectedCount})."
                : $"Target has {actualCount} but expected {expectedCount}. Missing {expectedCount - actualCount} records.",
        };
    }

    private async Task<ValidationCheckDto> CheckIsMigratedFlagAsync(string phase)
    {
        // Spot-check: for patients created by the migration user, IsMigrated must be true
        var untagged = await _context.Patients
            .CountAsync(p => p.CreatedBy == MigrationService.MigrationUser && !p.IsMigrated);

        return new ValidationCheckDto
        {
            CheckName  = "IsMigratedFlagConsistency",
            EntityType = "Patient",
            Status     = untagged == 0 ? "Pass" : "Fail",
            ActualCount = untagged,
            Detail     = untagged == 0
                ? "All records created by migration user have IsMigrated=true."
                : $"{untagged} patient(s) created by migration user are missing IsMigrated=true.",
        };
    }

    private async Task<IEnumerable<ValidationCheckDto>> SpotCheckRequiredFieldsAsync(string phase)
    {
        var results = new List<ValidationCheckDto>();

        // For patient phases: verify CffId and DateOfBirth are populated on migrated records
        if (phase != "Files")
        {
            var missingCffId = await _context.Patients
                .CountAsync(p => p.IsMigrated && p.CffId == 0);
            results.Add(new ValidationCheckDto
            {
                CheckName  = "PatientCffIdPopulated",
                EntityType = "Patient",
                Status     = missingCffId == 0 ? "Pass" : "Fail",
                ActualCount = missingCffId,
                Detail     = missingCffId == 0
                    ? "All migrated patients have CFF ID populated."
                    : $"{missingCffId} migrated patient(s) have CFF ID = 0.",
            });

            var missingDob = await _context.Patients
                .CountAsync(p => p.IsMigrated && p.DateOfBirth == default);
            results.Add(new ValidationCheckDto
            {
                CheckName  = "PatientDobPopulated",
                EntityType = "Patient",
                Status     = missingDob == 0 ? "Pass" : "Fail",
                ActualCount = missingDob,
                Detail     = missingDob == 0
                    ? "All migrated patients have Date of Birth populated."
                    : $"{missingDob} migrated patient(s) have missing Date of Birth.",
            });
        }

        if (phase == "Files")
        {
            // Verify content hash was computed for all migrated files (13-008 integrity)
            var missingHash = await _context.PatientFiles
                .CountAsync(f => f.IsMigrated && string.IsNullOrEmpty(f.ContentHash));
            results.Add(new ValidationCheckDto
            {
                CheckName  = "FilesContentHashPresent",
                EntityType = "PatientFile",
                Status     = missingHash == 0 ? "Pass" : "Fail",
                ActualCount = missingHash,
                Detail     = missingHash == 0
                    ? "All migrated files have SHA-256 content hash."
                    : $"{missingHash} migrated file(s) are missing content hash.",
            });
        }

        return results;
    }

    private async Task<IEnumerable<ValidationCheckDto>> CheckReferentialIntegrityAsync(string phase)
    {
        var results = new List<ValidationCheckDto>();

        // Form submissions must reference existing patients
        var orphanedForms = await _context.FormSubmissions
            .CountAsync(f => !_context.Patients.Any(p => p.Id == f.PatientId));
        results.Add(new ValidationCheckDto
        {
            CheckName  = "FormSubmissionsHavePatients",
            EntityType = "FormSubmission",
            Status     = orphanedForms == 0 ? "Pass" : "Fail",
            ActualCount = orphanedForms,
            Detail     = orphanedForms == 0
                ? "All form submissions reference valid patients."
                : $"{orphanedForms} form submission(s) reference non-existent patients.",
        });

        return results;
    }
}
