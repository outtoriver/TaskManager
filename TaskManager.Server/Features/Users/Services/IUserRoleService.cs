using TaskManager.Server.Features.Users.DTOs;

namespace TaskManager.Server.Features.Users.Services;

public interface IUserRoleService
{
    Task<IReadOnlyCollection<UserRoleDto>> GetRolesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task UpdateRolesAsync(
        int userId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default);
}