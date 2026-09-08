using TaskManager.Server.Features.Users.DTOs;

namespace TaskManager.Server.Features.Users.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<UserDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserListItemDto>> GetSubordinatesAsync(
        int managerId,
        CancellationToken cancellationToken = default);

    Task<UserDetailsDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserDetailsDto?> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}