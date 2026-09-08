using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Server.Authentication.Models;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;
using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Roles;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public sealed class WindowsAuthenticationService(
    ApplicationDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AuthenticationOptions> options)
    : IWindowsAuthenticationService
{
    public async Task<User?> GetOrCreateCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var login = principal.Identity.Name;

        if (string.IsNullOrWhiteSpace(login))
        {
            return null;
        }

        login = NormalizeWindowsLogin(login);

        var user = await db.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Login == login,
                cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Login = login,
                DisplayName = GetDisplayName(login, principal.Identity),
                AuthenticationType = AuthenticationType.Windows,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            await AssignInitialRoleAsync(
                user,
                cancellationToken);
        }
        else
        {
            if (!user.IsActive)
            {
                return null;
            }

            user.DisplayName = GetDisplayName(
                login,
                principal.Identity);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return user;
    }

    private async Task AssignInitialRoleAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var normalizedAdministrators = options.Value.WindowsAdministrators
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeWindowsLogin)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roleName = normalizedAdministrators.Contains(user.Login)
            ? RoleNames.Administrator
            : RoleNames.User;

        var role = await db.Roles
            .FirstOrDefaultAsync(
                x => x.Name == roleName,
                cancellationToken);

        if (role is null)
        {
            return;
        }

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            Role = role
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeWindowsLogin(string login)
    {
        return login.Trim();
    }

    private static string GetDisplayName(
        string login,
        IIdentity? identity)
    {
        if (!string.IsNullOrWhiteSpace(identity?.Name))
        {
            var name = identity.Name!;
            var separatorIndex = name.IndexOf('\\');

            if (separatorIndex >= 0 &&
                separatorIndex < name.Length - 1)
            {
                return name[(separatorIndex + 1)..];
            }
        }

        var slashIndex = login.IndexOf('\\');

        return slashIndex >= 0 && slashIndex < login.Length - 1
            ? login[(slashIndex + 1)..]
            : login;
    }
}
