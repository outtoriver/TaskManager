namespace TaskManager.Server.Enums.Users;

public enum TaskStatus
{
    New = 0,
    Assigned = 1,
    Accepted = 2,
    InProgress = 3,
    Review = 4,
    Completed = 5,
    Paused = 6,
    Blocked = 7,
    Cancelled = 8
}