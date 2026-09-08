using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Departments.DTOs;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Models.Departments;

namespace TaskManager.Server.Features.Departments.Services;

public class DepartmentService(ApplicationDbContext db)
    : IDepartmentService
{
    public async Task<IReadOnlyCollection<DepartmentListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Departments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                UserCount = x.Users.Count(u => u.IsActive)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Departments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DepartmentDetailsDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                UserCount = x.Users.Count(u => u.IsActive),

                Users = x.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.DisplayName)
                    .Select(u => new UserListItemDto
                    {
                        Id = u.Id,
                        Login = u.Login,
                        DisplayName = u.DisplayName,
                        Email = u.Email,
                        IsActive = u.IsActive,

                        DepartmentId = u.DepartmentId,
                        DepartmentName = x.Name,

                        PositionId = u.PositionId,
                        PositionName = u.Position != null
                            ? u.Position.Name
                            : null,

                        ManagerId = u.ManagerId,
                        ManagerName = u.Manager != null
                            ? u.Manager.DisplayName
                            : null
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DepartmentDetailsDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название отдела не может быть пустым.",
                nameof(request.Name));
        }

        var exists = await db.Departments
            .AnyAsync(
                x => x.Name == name,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Отдел '{name}' уже существует.");
        }

        var department = new Department
        {
            Name = name,
            Description = Normalize(request.Description),
            IsActive = true
        };

        db.Departments.Add(department);

        await db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(
            department.Id,
            cancellationToken))!;
    }

    public async Task<DepartmentDetailsDto?> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await db.Departments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (department is null)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название отдела не может быть пустым.",
                nameof(request.Name));
        }

        var duplicateExists = await db.Departments
            .AnyAsync(
                x => x.Id != id &&
                     x.Name == name,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Отдел '{name}' уже существует.");
        }

        department.Name = name;
        department.Description = Normalize(request.Description);
        department.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var department = await db.Departments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (department is null)
        {
            return false;
        }

        var hasActiveUsers = await db.Users
            .AnyAsync(
                x => x.DepartmentId == id &&
                     x.IsActive,
                cancellationToken);

        if (hasActiveUsers)
        {
            throw new InvalidOperationException(
                "Нельзя удалить отдел, в котором есть активные пользователи.");
        }

        department.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}