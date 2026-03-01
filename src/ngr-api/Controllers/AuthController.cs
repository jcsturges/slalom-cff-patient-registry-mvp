using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Syncs the authenticated user's profile from JWT claims into the local database.
    /// Creates the user record on first login; updates name/email on subsequent logins.
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncUser()
    {
        var oktaId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(oktaId))
        {
            _logger.LogWarning("Auth sync called without a valid sub/NameIdentifier claim");
            return BadRequest("Missing user identifier in token");
        }

        var email = User.FindFirstValue(ClaimTypes.Email)
                   ?? User.FindFirstValue("email")
                   ?? "";
        var firstName = User.FindFirstValue(ClaimTypes.GivenName)
                       ?? User.FindFirstValue("given_name")
                       ?? "";
        var lastName = User.FindFirstValue(ClaimTypes.Surname)
                      ?? User.FindFirstValue("family_name")
                      ?? "";

        var user = await _userService.CreateOrUpdateUserAsync(oktaId, email, firstName, lastName);

        _logger.LogInformation("User synced: {OktaId} ({Email})", oktaId, email);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
        });
    }
}
