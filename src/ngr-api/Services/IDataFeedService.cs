using NgrApi.DTOs;

namespace NgrApi.Services;

/// <summary>
/// Outbound data feed service — extracts changed/deleted NGR records and writes
/// them to Azure Blob Storage for ingestion by the CFF Data Warehouse (SRS 10).
///
/// The extraction mechanism is timestamp-based (change-data-capture pattern):
///   Delta: WHERE UpdatedAt > last_successful_run OR DeletedAt > last_successful_run
///   Full:  ALL current records + all tombstones within retention window
///
/// All credentials (Blob SAS, SFTP keys) are read from Azure Key Vault via
/// the IConfiguration binding — never hardcoded.
/// </summary>
public interface IDataFeedService
{
    /// <summary>Run a delta feed (changed since the last successful run) or
    /// reprocess a specific window when overrides are supplied (13-001).</summary>
    Task<FeedRunDto> RunDeltaFeedAsync(
        string triggeredBy,
        DateTime? windowOverrideStart = null,
        DateTime? windowOverrideEnd   = null);

    /// <summary>Run a full data resynchronization — all current records + tombstones (13-003).
    /// Restricted to SystemAdmin. Does not affect the stored last-run watermark.</summary>
    Task<FeedRunDto> RunFullResyncAsync(string triggeredBy);

    /// <summary>Paginated run history (13-004).</summary>
    Task<(IEnumerable<FeedRunDto> Runs, int Total)> GetRunsAsync(int page, int pageSize);

    /// <summary>Get a single run including full reconciliation JSON (13-004).</summary>
    Task<FeedRunDto?> GetRunByIdAsync(int id);

    /// <summary>List all field mappings (13-001).</summary>
    Task<IEnumerable<FeedFieldMappingDto>> GetFieldMappingsAsync();

    /// <summary>Update an existing field mapping (creates a new versioned row) (13-001).</summary>
    Task<FeedFieldMappingDto?> UpdateFieldMappingAsync(int id, UpdateFeedFieldMappingDto dto, string updatedBy);

    /// <summary>Seed default NGR→CFF field mappings if none exist.</summary>
    Task EnsureDefaultMappingsAsync(string createdBy);
}
