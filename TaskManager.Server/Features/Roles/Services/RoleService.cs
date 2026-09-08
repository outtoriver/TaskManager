using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Roles.DTOs;
using TaskManager.Server.Models.Roles;

namespace TaskManager.Server.Features.Roles.Services;

public sealed class RoleService(ApplicationDbContext db)
    : IRoleService
{
    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsSystemRole = x.IsSystemRole,
                UserCount = x.UserRoles.Count,

                Permissions = x.RolePermissions
                    .OrderBy(p => p.Permission.Code)
                    .Select(p => p.Permission.Code)
                    .ToArray()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleDto?> GetRoleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Roles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsSystemRole = x.IsSystemRole,
                UserCount = x.UserRoles.Count,

                Permissions = x.RolePermissions
                    .OrderBy(p => p.Permission.Code)
                    .Select(p => p.Permission.Code)
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PermissionDto>>
        GetPermissionsAsync(
            CancellationToken cancellationToken = default)
    {
        return await db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRolePermissionsAsync(
        int roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(
                x => x.Id == roleId,
                cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException(
                $"Роль с ID {roleId} не найдена.");
        }

        var permissionIds = request.PermissionIds
            .Distinct()
            .ToHashSet();

        var existingPermissions = await db.Permissions
            .Where(x => permissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissions.Count != permissionIds.Count)
        {
            throw new ArgumentException(
                "Один или несколько указанных permissions не существуют.");
        }

        role.RolePermissions.Clear();

        foreach (var permissionId in existingPermissions)
        {
            role.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}