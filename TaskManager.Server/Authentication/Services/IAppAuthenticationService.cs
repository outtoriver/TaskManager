using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public interface IAppAuthenticationService
{
    Task<User?> ValidateLocalCredentialsAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default);

    Task SignInAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(
        CancellationToken cancellationToken = default);
}
