using TaskManager.Server.Features.Positions.DTOs;

namespace TaskManager.Server.Features.Positions.Services;

public interface IPositionService
{
    Task<IReadOnlyCollection<PositionListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PositionDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PositionDetailsDto> CreateAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default);

    Task<PositionDetailsDto?> UpdateAsync(
        int id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}