using NgrApi.DTOs;

namespace NgrApi.Services;

public interface IDatabaseLockService
{
    Task<IEnumerable<DatabaseLockDto>> GetLocksAsync();
    Task<DatabaseLockImpactDto> GetImpactAsync(int reportingYear);
    Task<DatabaseLockProgressDto> GetProgressAsync(int lockId);

    /// <summary>Execute synchronous lock immediately; blocks until complete.</summary>
    Task<DatabaseLockDto> ExecuteSynchronousLockAsync(int reportingYear, DateTime lockDate, string initiatedBy);

    /// <summary>Create a Pending lock record to be executed in the overnight batch window.</summary>
    Task<DatabaseLockDto> ScheduleBatchLockAsync(int reportingYear, DateTime lockDate, DateTime scheduledDate, string initiatedBy);
}
