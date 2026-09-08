namespace TaskManager.Server.Features.Tasks.DTOs;
using TaskStatusEnum = TaskManager.Server.Enums.Tasks.TaskStatus;
public sealed class ChangeTaskStatusRequest
{
    public TaskStatusEnum Status { get; set; }
    public int? Progress { get; set; }
    public string? Comment { get; set; }
}
