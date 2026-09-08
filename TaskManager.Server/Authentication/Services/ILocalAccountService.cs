using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public interface ILocalAccountService
{
    Task<User> CreateAsync(
        string login,
        string displayName,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> SetPasswordAsync(
        int userId,
        string password,
        CancellationToken cancellationToken = default);
}