using System.Security.Claims;
using System.Text.Json;
using NgrApi.Services;

namespace NgrApi.Middleware;

/// <summary>
/// Detects the X-Impersonation-Session-Id header and, when present and valid,
/// replaces HttpContext.User with a claims principal built from the target user's roles.
/// The acting admin's identity is preserved in HttpContext.Items["ActingAdminId"].
/// </summary>
public class ImpersonationMiddleware
{
    private const string HeaderName = "X-Impersonation-Session-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<ImpersonationMiddleware> _logger;

    public ImpersonationMiddleware(RequestDelegate next, ILogger<ImpersonationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IImpersonationService impersonationService)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValue)
            && Guid.TryParse(headerValue, out var sessionId))
        {
            var session = await impersonationService.ValidateSessionAsync(sessionId);

            if (session != null)
            {
                // Preserve the acting admin's identity for audit logging
                var actingAdminId = context.User.FindFirstValue("sub")
                                 ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? session.AdminUserId;

                context.Items["ActingAdminId"]        = actingAdminId;
                context.Items["ImpersonationSessionId"] = sessionId;
                context.Items["IsImpersonating"]      = true;

                // Build a new ClaimsPrincipal from the target user's groups
                var groups = JsonSerializer.Deserialize<List<string>>(session.TargetUserGroupsJson)
                          ?? new List<string>();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, session.TargetUserId),
                    new("sub",   session.TargetUserId),
                    new("email", session.TargetUserEmail),
                    new("name",  session.TargetUserName),
                };

                // Add each group as a claim matching the RoleClaimType configured in Program.cs
                foreach (var group in groups)
                    claims.Add(new Claim("groups", group));

                var identity  = new ClaimsIdentity(claims, "Impersonation", "name", "groups");
                context.User  = new ClaimsPrincipal(identity);

                _logger.LogInformation(
                    "Impersonation active: admin {Admin} acting as {Target}, session {Session}",
                    actingAdminId, session.TargetUserEmail, sessionId);
            }
            else
            {
                _logger.LogWarning(
                    "Invalid or expired impersonation session {SessionId} in request header", sessionId);
            }
        }

        await _next(context);
    }
}
