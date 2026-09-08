using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Models.Tasks;

public class TaskComment
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    public int AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public required string Text { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}