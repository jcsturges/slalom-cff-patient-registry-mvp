using NgrApi.DTOs;

namespace NgrApi.Services;

/// <summary>
/// Historical data migration service — imports records from the CFF Data Warehouse
/// (portCF) into the NGR database (SRS Section 11).
///
/// Migration is phase-based and re-runnable. Each phase creates a MigrationRun row.
/// All migrated records are tagged with IsMigrated=true and CreatedBy=MIGRATION_USER.
///
/// The source connector reads from a configurable SQL connection string:
///   Migration:SourceConnectionString  (Azure Key Vault secret)
/// When this is not configured, phases complete with 0 source records (safe no-op).
///
/// Valid phases:
///   "Demographics", "Diagnosis", "SweatTest", "ALD", "Transplant",
///   "AnnualReview", "Encounter", "Labs", "CareEpisode", "PhoneNote", "Files"
/// </summary>
public interface IMigrationService
{
    Task<MigrationRunDto> ExecutePhaseAsync(string phase, string triggeredBy);
    Task<(IEnumerable<MigrationRunDto> Runs, int Total)> GetRunsAsync(int page, int pageSize);
    Task<MigrationRunDto?> GetRunByIdAsync(int id);
}
