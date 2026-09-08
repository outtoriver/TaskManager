using TaskManager.Server.Features.Tasks.DTOs;

namespace TaskManager.Server.Features.Tasks.Services;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskDto?> ChangeStatusAsync(int id, ChangeTaskStatusRequest request, CancellationToken cancellationToken = default);
    Task<TaskDto?> ChangePriorityAsync(int id, ChangeTaskPriorityRequest request, CancellationToken cancellationToken = default);
    Task<TaskCommentDto?> AddCommentAsync(int id, CreateTaskCommentRequest request, CancellationToken cancellationToken = default);
}
