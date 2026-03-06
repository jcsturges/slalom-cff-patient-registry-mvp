using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

/// <summary>
/// Service implementation for audit logging.
/// Auto-populates impersonation context from IHttpContextAccessor when available.
/// Logs are PHI-free — entity IDs use opaque system identifiers only.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ApplicationDbContext context,
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
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
            // Auto-populate impersonation context from middleware
            var http = _httpContextAccessor.HttpContext;
            var isImpersonated = http?.Items["IsImpersonating"] is true;
            var actingAdminId  = http?.Items["ActingAdminId"] as string;
            Guid? sessionId    = null;
            if (http?.Request.Headers.TryGetValue("X-Impersonation-Session-Id", out var hdr) == true
                && Guid.TryParse(hdr, out var parsed))
                sessionId = parsed;

            var auditLog = new AuditLog
            {
                EntityType              = entityType,
                EntityId                = entityId,
                Action                  = action,
                OldValues               = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues               = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                UserId                  = userId,
                UserEmail               = userEmail,
                Timestamp               = DateTime.UtcNow,
                IpAddress               = ipAddress ?? http?.Connection.RemoteIpAddress?.ToString(),
                IsImpersonated          = isImpersonated,
                ActingAdminId           = actingAdminId,
                ImpersonationSessionId  = sessionId,
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {EntityType} {EntityId} {Action} by {UserId} impersonated={IsImpersonated}",
                entityType, entityId, action, userId, isImpersonated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing audit log for {EntityType} {EntityId}", entityType, entityId);
            // Don't throw — audit logging must not break the main operation
        }
    }

    public async Task LogChangeTrackingAsync(
        DbContext dbContext,
        string userId,
        string userEmail,
        string? ipAddress)
    {
        try
        {
            var entries = dbContext.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId   = entry.Properties
                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "unknown";

                var action = entry.State switch
                {
                    EntityState.Added    => "Create",
                    EntityState.Deleted  => "Delete",
                    _                    => "Update",
                };

                Dictionary<string, object?> oldValues = new();
                Dictionary<string, object?> newValues = new();

                foreach (var prop in entry.Properties)
                {
                    if (entry.State == EntityState.Modified && !prop.IsModified) continue;

                    var name = prop.Metadata.Name;
                    // Exclude known PHI/PII fields from audit payload (12-001 AC: exclude PHI)
                    if (IsPiiField(entityType, name)) continue;

                    if (entry.State != EntityState.Added)
                        oldValues[name] = prop.OriginalValue;
                    if (entry.State != EntityState.Deleted)
                        newValues[name] = prop.CurrentValue;
                }

                await LogActionAsync(
                    entityType, entityId, action,
                    oldValues.Count > 0 ? oldValues : null,
                    newValues.Count > 0 ? newValues : null,
                    userId, userEmail, ipAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LogChangeTrackingAsync");
        }
    }

    // Fields containing PHI/PII — excluded from audit log payloads
    private static readonly HashSet<string> _phiFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName", "LastName", "MiddleName", "LastNameAtBirth",
        "DateOfBirth", "SsnLast4", "Email", "Phone",
        "MedicalRecordNumber", "Address", "City", "State", "Zip",
        "EmergencyContactName", "EmergencyContactPhone",
    };

    private static bool IsPiiField(string entityType, string fieldName) =>
        _phiFields.Contains(fieldName);
}
