using System.Text.Json;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

/// <summary>
/// Service implementation for audit logging
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        object? oldValues,
        object? newValues,
        string userId,
        string userEmail,
        string? ipAddress)
    {
        try
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                UserId = userId,
                UserEmail = userEmail,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {EntityType} {EntityId} {Action} by {UserEmail}",
                entityType, entityId, action, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {EntityType} {EntityId}", entityType, entityId);
            // Don't throw — audit logging must not break the main operation
        }
    }
}
