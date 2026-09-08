using TaskManager.Server.Features.Departments.DTOs;

namespace TaskManager.Server.Features.Departments.Services;

public interface IDepartmentService
{
    Task<IReadOnlyCollection<DepartmentListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DepartmentDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<DepartmentDetailsDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task<DepartmentDetailsDto?> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}