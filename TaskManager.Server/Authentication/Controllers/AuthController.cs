using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Authentication.Models;
using TaskManager.Server.Authentication.Services;
using TaskManager.Server.Data;
using TaskManager.Server.Services.CurrentUser;

namespace TaskManager.Server.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAppAuthenticationService authenticationService,
    IWindowsAuthenticationService windowsAuthenticationService,
    ICurrentUserService currentUserService,
    ApplicationDbContext db)
    : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await authenticationService
            .ValidateLocalCredentialsAsync(
                request.Login,
                request.Password,
                cancellationToken);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Неверный логин или пароль."
            });
        }

        await authenticationService.SignInAsync(
            user,
            cancellationToken);

        var response = await BuildResponseAsync(
            user.Id,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("windows")]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public async Task<ActionResult<LoginResponse>> Windows(
        CancellationToken cancellationToken)
    {
        var user = await windowsAuthenticationService
            .GetOrCreateCurrentUserAsync(cancellationToken);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Windows-пользователь не найден или отключён."
            });
        }

        await authenticationService.SignInAsync(
            user,
            cancellationToken);

        var response = await BuildResponseAsync(
            user.Id,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var response = await BuildResponseAsync(
            userId.Value,
            cancellationToken);

        return response is null
            ? Unauthorized()
            : Ok(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        await authenticationService.SignOutAsync(cancellationToken);

        return NoContent();
    }

    private async Task<LoginResponse?> BuildResponseAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new LoginResponse
            {
                Id = x.Id,
                Login = x.Login,
                DisplayName = x.DisplayName,
                Email = x.Email,
                DepartmentName = x.Department != null
                    ? x.Department.Name
                    : null,
                PositionName = x.Position != null
                    ? x.Position.Name
                    : null,
                ManagerName = x.Manager != null
                    ? x.Manager.DisplayName
                    : null,
                IsActive = x.IsActive,
                AuthenticationType = x.AuthenticationType.ToString(),
                Roles = x.UserRoles
                    .OrderBy(r => r.Role.Name)
                    .Select(r => r.Role.Name)
                    .ToArray(),
                Permissions = x.UserRoles
                    .SelectMany(r => r.Role.RolePermissions)
                    .OrderBy(p => p.Permission.Code)
                    .Select(p => p.Permission.Code)
                    .Distinct()
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
