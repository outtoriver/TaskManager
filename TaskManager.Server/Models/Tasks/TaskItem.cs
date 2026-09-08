using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Models.Tasks;

public class TaskItem
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.New;

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    public bool IsImportant { get; set; }

    public int Progress { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    public int AssignedToId { get; set; }

    public User AssignedTo { get; set; } = null!;

    public ICollection<TaskComment> Comments { get; set; } = [];

    public ICollection<TaskHistory> History { get; set; } = [];
}