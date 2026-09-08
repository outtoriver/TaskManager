using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Features.Users.Services;

public class UserService(ApplicationDbContext db)
    : IUserService
{
    public async Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .Select(x => new UserListItemDto
            {
                Id = x.Id,
                Login = x.Login,
                DisplayName = x.DisplayName,
                Email = x.Email,
                IsActive = x.IsActive,

                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null
                    ? x.Department.Name
                    : null,

                PositionId = x.PositionId,
                PositionName = x.Position != null
                    ? x.Position.Name
                    : null,

                ManagerId = x.ManagerId,
                ManagerName = x.Manager != null
                    ? x.Manager.DisplayName
                    : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDetailsDto
            {
                Id = x.Id,
                Login = x.Login,
                DisplayName = x.DisplayName,
                Email = x.Email,
                Phone = x.Phone,

                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null
                    ? x.Department.Name
                    : null,

                PositionId = x.PositionId,
                PositionName = x.Position != null
                    ? x.Position.Name
                    : null,

                ManagerId = x.ManagerId,
                ManagerName = x.Manager != null
                    ? x.Manager.DisplayName
                    : null,

                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                LastLoginAt = x.LastLoginAt,

                Roles = x.UserRoles
                    .OrderBy(r => r.Role.Name)
                    .Select(r => new UserRoleDto
                    {
                        Id = r.Role.Id,
                        Name = r.Role.Name,
                        Description = r.Role.Description,
                        IsSystemRole = r.Role.IsSystemRole
                    })
                    .ToArray(),

                Subordinates = x.Subordinates
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DisplayName)
                    .Select(s => new UserListItemDto
                    {
                        Id = s.Id,
                        Login = s.Login,
                        DisplayName = s.DisplayName,
                        Email = s.Email,
                        IsActive = s.IsActive,

                        DepartmentId = s.DepartmentId,
                        DepartmentName = s.Department != null
                            ? s.Department.Name
                            : null,

                        PositionId = s.PositionId,
                        PositionName = s.Position != null
                            ? s.Position.Name
                            : null,

                        ManagerId = s.ManagerId,
                        ManagerName = x.DisplayName
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserListItemDto>>
        GetSubordinatesAsync(
            int managerId,
            CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x =>
                x.ManagerId == managerId &&
                x.IsActive)
            .OrderBy(x => x.DisplayName)
            .Select(x => new UserListItemDto
            {
                Id = x.Id,
                Login = x.Login,
                DisplayName = x.DisplayName,
                Email = x.Email,
                IsActive = x.IsActive,

                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null
                    ? x.Department.Name
                    : null,

                PositionId = x.PositionId,
                PositionName = x.Position != null
                    ? x.Position.Name
                    : null,

                ManagerId = x.ManagerId,
                ManagerName = x.Manager != null
                    ? x.Manager.DisplayName
                    : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailsDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var login = request.Login.Trim();
        var displayName = request.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException(
                "Login не может быть пустым.",
                nameof(request.Login));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "DisplayName не может быть пустым.",
                nameof(request.DisplayName));
        }

        var loginExists = await db.Users
            .AnyAsync(
                x => x.Login == login,
                cancellationToken);

        if (loginExists)
        {
            throw new InvalidOperationException(
                $"Пользователь с логином '{login}' уже существует.");
        }

        await ValidateReferencesAsync(
            request.DepartmentId,
            request.PositionId,
            request.ManagerId,
            null,
            cancellationToken);

        var user = new User
        {
            Login = login,
            DisplayName = displayName,
            Email = Normalize(request.Email),
            Phone = Normalize(request.Phone),
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            ManagerId = request.ManagerId,
            IsActive = request.IsActive
        };

        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(
            user.Id,
            cancellationToken))!;
    }

    public async Task<UserDetailsDto?> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var displayName = request.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "DisplayName не может быть пустым.",
                nameof(request.DisplayName));
        }

        await ValidateReferencesAsync(
            request.DepartmentId,
            request.PositionId,
            request.ManagerId,
            id,
            cancellationToken);

        user.DisplayName = displayName;
        user.Email = Normalize(request.Email);
        user.Phone = Normalize(request.Phone);
        user.DepartmentId = request.DepartmentId;
        user.PositionId = request.PositionId;
        user.ManagerId = request.ManagerId;
        user.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateReferencesAsync(
        int? departmentId,
        int? positionId,
        int? managerId,
        int? currentUserId,
        CancellationToken cancellationToken)
    {
        if (departmentId.HasValue)
        {
            var departmentExists = await db.Departments
                .AnyAsync(
                    x =>
                        x.Id == departmentId.Value &&
                        x.IsActive,
                    cancellationToken);

            if (!departmentExists)
            {
                throw new KeyNotFoundException(
                    $"Отдел с ID {departmentId.Value} не найден.");
            }
        }

        if (positionId.HasValue)
        {
            var positionExists = await db.Positions
                .AnyAsync(
                    x =>
                        x.Id == positionId.Value &&
                        x.IsActive,
                    cancellationToken);

            if (!positionExists)
            {
                throw new KeyNotFoundException(
                    $"Должность с ID {positionId.Value} не найдена.");
            }
        }

        if (!managerId.HasValue)
        {
            return;
        }

        if (currentUserId.HasValue &&
            managerId.Value == currentUserId.Value)
        {
            throw new InvalidOperationException(
                "Пользователь не может быть руководителем самого себя.");
        }

        var managerExists = await db.Users
            .AnyAsync(
                x =>
                    x.Id == managerId.Value &&
                    x.IsActive,
                cancellationToken);

        if (!managerExists)
        {
            throw new KeyNotFoundException(
                $"Руководитель с ID {managerId.Value} не найден.");
        }

        if (currentUserId.HasValue)
        {
            var createsCycle = await CreatesManagerCycleAsync(
                currentUserId.Value,
                managerId.Value,
                cancellationToken);

            if (createsCycle)
            {
                throw new InvalidOperationException(
                    "Нельзя назначить руководителя: " +
                    "это создаст цикл в иерархии.");
            }
        }
    }

    private async Task<bool> CreatesManagerCycleAsync(
        int userId,
        int managerId,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<int>();

        var currentId = managerId;

        while (true)
        {
            if (!visited.Add(currentId))
            {
                return true;
            }

            if (currentId == userId)
            {
                return true;
            }

            var parentId = await db.Users
                .Where(x => x.Id == currentId)
                .Select(x => x.ManagerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!parentId.HasValue)
            {
                return false;
            }

            currentId = parentId.Value;
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}