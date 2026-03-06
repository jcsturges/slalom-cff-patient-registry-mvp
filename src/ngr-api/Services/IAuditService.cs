using Microsoft.EntityFrameworkCore;

namespace NgrApi.Services;

/// <summary>
/// Service interface for audit logging
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        object? oldValues,
        object? newValues,
        string userId,
        string userEmail,
        string? ipAddress);

    /// <summary>
    /// Iterates EF Core ChangeTracker modified/added/deleted entries and logs
    /// field-level diffs for each entity, excluding PHI/PII fields.
    /// Call immediately after SaveChangesAsync() or before it (on tracked entities).
    /// </summary>
    Task LogChangeTrackingAsync(
        DbContext dbContext,
        string userId,
        string userEmail,
        string? ipAddress);
}
