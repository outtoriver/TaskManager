namespace TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Enums.Tasks;
public sealed class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public bool IsImportant { get; set; }
    public int Progress { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int AssignedToId { get; set; }
}
