namespace TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Enums.Tasks;
using TaskStatusEnum = TaskManager.Server.Enums.Tasks.TaskStatus;
public sealed class TaskDto : TaskListItemDto
{
    public string? Description { get; set; }
    public IReadOnlyCollection<TaskCommentDto> Comments { get; set; } = [];
    public IReadOnlyCollection<TaskHistoryDto> History { get; set; } = [];
}
public sealed class TaskCommentDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public sealed class TaskHistoryDto
{
    public int Id { get; set; }
    public int ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public TaskStatusEnum? OldStatus { get; set; }
    public TaskStatusEnum? NewStatus { get; set; }
    public TaskPriority? OldPriority { get; set; }
    public TaskPriority? NewPriority { get; set; }
    public int? OldAssignedToId { get; set; }
    public int? NewAssignedToId { get; set; }
    public DateTime? OldDueDate { get; set; }
    public DateTime? NewDueDate { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
