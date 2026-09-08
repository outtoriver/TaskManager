using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Models.Roles;

namespace TaskManager.Server.Features.Users.Services;

public sealed class UserRoleService(
    ApplicationDbContext db)
    : IUserRoleService
{
    public async Task<IReadOnlyCollection<UserRoleDto>> GetRolesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                $"Пользователь с ID {userId} не найден.");
        }

        return await db.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Role.Name)
            .Select(x => new UserRoleDto
            {
                Id = x.Role.Id,
                Name = x.Role.Name,
                Description = x.Role.Description,
                IsSystemRole = x.Role.IsSystemRole
            })
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRolesAsync(
        int userId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists = await db.Users
            .AnyAsync(
                x => x.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                $"Пользователь с ID {userId} не найден.");
        }

        var requestedRoleIds = request.RoleIds
            .Distinct()
            .ToHashSet();

        var existingRoleIds = await db.Roles
            .Where(x => requestedRoleIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (existingRoleIds.Count != requestedRoleIds.Count)
        {
            throw new ArgumentException(
                "Одна или несколько указанных ролей не существуют.");
        }

        var currentUserRoles = await db.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        db.UserRoles.RemoveRange(currentUserRoles);

        foreach (var roleId in existingRoleIds)
        {
            db.UserRoles.Add(
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}