namespace TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Enums.Tasks;
using TaskStatusEnum = TaskManager.Server.Enums.Tasks.TaskStatus;
public class TaskListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TaskStatusEnum Status { get; set; }
    public TaskPriority Priority { get; set; }
    public bool IsImportant { get; set; }
    public int Progress { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int AssignedToId { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string? AssignedToDepartmentName { get; set; }
    public int CommentCount { get; set; }
}
