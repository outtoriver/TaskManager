using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;
using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public sealed class LocalAuthenticationService(
    ApplicationDbContext db,
    IPasswordHasher<User> passwordHasher,
    IHttpContextAccessor httpContextAccessor)
    : IAppAuthenticationService
{
    public async Task<User?> ValidateLocalCredentialsAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrEmpty(password))
        {
            return null;
        }

        var normalizedLogin = login.Trim();

        var user = await db.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Login == normalizedLogin &&
                     x.AuthenticationType == AuthenticationType.Local &&
                     x.IsActive,
                cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }

        user.LastLoginAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task SignInAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new InvalidOperationException(
                "HTTP context is unavailable.");
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Login),

            new(
                AuthenticationConstants.UserIdClaim,
                user.Id.ToString()),

            new(
                AuthenticationConstants.DisplayNameClaim,
                user.DisplayName)
        };

        foreach (var userRole in user.UserRoles)
        {
            if (!string.IsNullOrWhiteSpace(userRole.Role.Name))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        userRole.Role.Name));
            }
        }

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationConstants.ApplicationCookieScheme);

        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            AuthenticationConstants.ApplicationCookieScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        httpContext.User = principal;
    }

    public async Task SignOutAsync(
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        await httpContext.SignOutAsync(
            AuthenticationConstants.ApplicationCookieScheme);
    }
}
