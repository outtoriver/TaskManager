using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;

namespace TaskManager.Server.Services.CurrentUser;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext db)
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor;

    private readonly ApplicationDbContext _db = db;

    public int? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var claim = user?.FindFirstValue(
                AuthenticationConstants.UserIdClaim);

            if (int.TryParse(claim, out var id))
            {
                return id;
            }

            claim = user?.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(claim, out id)
                ? id
                : null;
        }
    }

    public string? Login =>
        _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .IsAuthenticated == true;

    public async Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || !UserId.HasValue)
        {
            return false;
        }

        return await _db.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == UserId.Value)
            .SelectMany(x => x.Role.RolePermissions)
            .AnyAsync(
                x => x.Permission.Code == permission,
                cancellationToken);
    }

    public async Task<bool> HasRoleAsync(
        string role,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || !UserId.HasValue)
        {
            return false;
        }

        return await _db.UserRoles
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == UserId.Value &&
                     x.Role.Name == role,
                cancellationToken);
    }
}
