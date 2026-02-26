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
}
