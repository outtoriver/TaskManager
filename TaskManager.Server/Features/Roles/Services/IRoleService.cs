using TaskManager.Server.Features.Roles.DTOs;

namespace TaskManager.Server.Features.Roles.Services;

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(
        CancellationToken cancellationToken = default);

    Task<RoleDto?> GetRoleAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync(
        CancellationToken cancellationToken = default);

    Task UpdateRolePermissionsAsync(
        int roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default);
}