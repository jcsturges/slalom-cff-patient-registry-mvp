using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgrApi.Data;

namespace NgrApi.Controllers;

/// <summary>
/// User listing and management (Foundation Admin tools — user impersonation entry point).
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = "FoundationAnalyst")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Search users by name or email (Foundation Admin only)</summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search)
    {
        var query = _context.Users
            .Include(u => u.ProgramRoles)
                .ThenInclude(pr => pr.Role)
            .Include(u => u.ProgramRoles)
                .ThenInclude(pr => pr.Program)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Take(100)
            .Select(u => new
            {
                u.Id,
                OktaId     = u.OktaId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                ProgramRoles = u.ProgramRoles
                    .Where(pr => pr.Status == "Active")
                    .Select(pr => new
                    {
                        ProgramName = pr.Program.Name,
                        RoleName    = pr.Role.Name,
                    })
                    .ToList(),
            })
            .ToListAsync();

        return Ok(users);
    }
}
