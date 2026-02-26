using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await _context.Users
            .Include(u => u.ProgramRoles)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.ProgramRoles)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByOktaIdAsync(string oktaId)
    {
        return await _context.Users
            .Include(u => u.ProgramRoles)
            .FirstOrDefaultAsync(u => u.OktaId == oktaId);
    }

    public async Task<User> CreateOrUpdateUserAsync(string oktaId, string email, string firstName, string lastName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.OktaId == oktaId);

        if (user == null)
        {
            user = new User
            {
                OktaId = oktaId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }
        else
        {
            user.Email = email;
            user.FirstName = firstName;
            user.LastName = lastName;
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
