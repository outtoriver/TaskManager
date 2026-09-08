using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Positions.DTOs;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Models.Positions;

namespace TaskManager.Server.Features.Positions.Services;

public class PositionService(ApplicationDbContext db)
    : IPositionService
{
    public async Task<IReadOnlyCollection<PositionListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Positions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PositionListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                IsManagerPosition = x.IsManagerPosition,
                IsActive = x.IsActive,
                UserCount = x.Users.Count(u => u.IsActive)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PositionDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Positions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PositionDetailsDto
            {
                Id = x.Id,
                Name = x.Name,
                IsManagerPosition = x.IsManagerPosition,
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
                        DepartmentName = u.Department != null
                            ? u.Department.Name
                            : null,

                        PositionId = u.PositionId,
                        PositionName = x.Name,

                        ManagerId = u.ManagerId,
                        ManagerName = u.Manager != null
                            ? u.Manager.DisplayName
                            : null
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PositionDetailsDto> CreateAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название должности не может быть пустым.",
                nameof(request.Name));
        }

        var exists = await db.Positions
            .AnyAsync(
                x => x.Name == name,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Должность '{name}' уже существует.");
        }

        var position = new Position
        {
            Name = name,
            IsManagerPosition = request.IsManagerPosition,
            IsActive = true
        };

        db.Positions.Add(position);

        await db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(
            position.Id,
            cancellationToken))!;
    }

    public async Task<PositionDetailsDto?> UpdateAsync(
        int id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var position = await db.Positions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (position is null)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название должности не может быть пустым.",
                nameof(request.Name));
        }

        var duplicateExists = await db.Positions
            .AnyAsync(
                x => x.Id != id &&
                     x.Name == name,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Должность '{name}' уже существует.");
        }

        position.Name = name;
        position.IsManagerPosition = request.IsManagerPosition;
        position.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var position = await db.Positions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (position is null)
        {
            return false;
        }

        var hasActiveUsers = await db.Users
            .AnyAsync(
                x => x.PositionId == id &&
                     x.IsActive,
                cancellationToken);

        if (hasActiveUsers)
        {
            throw new InvalidOperationException(
                "Нельзя удалить должность, которая назначена активным пользователям.");
        }

        position.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}