using NgrApi.Models;

namespace NgrApi.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByOktaIdAsync(string oktaId);
    Task<User> CreateOrUpdateUserAsync(string oktaId, string email, string firstName, string lastName);
    Task<bool> DeactivateUserAsync(int id);
}
