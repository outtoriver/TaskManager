using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public interface IWindowsAuthenticationService
{
    Task<User?> GetOrCreateCurrentUserAsync(
        CancellationToken cancellationToken = default);
}
