namespace TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Enums.Tasks;
public sealed class ChangeTaskPriorityRequest
{
    public TaskPriority Priority { get; set; }
    public string? Comment { get; set; }
}
