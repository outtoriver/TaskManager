using TaskManager.Server.Enums.Tasks;
using TaskManager.Server.Models.Users;

using TaskStatusEnum = TaskManager.Server.Enums.Tasks.TaskStatus;

namespace TaskManager.Server.Models.Tasks;

public class TaskHistory
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    public int ChangedById { get; set; }

    public User ChangedBy { get; set; } = null!;

    public TaskStatusEnum? OldStatus { get; set; }

    public TaskStatusEnum? NewStatus { get; set; }

    public TaskPriority? OldPriority { get; set; }

    public TaskPriority? NewPriority { get; set; }

    public int? OldAssignedToId { get; set; }

    public int? NewAssignedToId { get; set; }

    public DateTime? OldDueDate { get; set; }

    public DateTime? NewDueDate { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}