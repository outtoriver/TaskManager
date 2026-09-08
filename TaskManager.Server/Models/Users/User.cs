using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Departments;
using TaskManager.Server.Models.Positions;
using TaskManager.Server.Models.Roles;
using TaskManager.Server.Models.Tasks;

namespace TaskManager.Server.Models.Users;

public class User
{
    public int Id { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// Для Windows: DOMAIN\username
    /// Для Local: обычный логин.
    /// </summary>
    public required string Login { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public AuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// Используется только для локальной аутентификации.
    /// Для Windows Authentication должно быть null.
    /// </summary>
    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int? PositionId { get; set; }

    public Position? Position { get; set; }

    /// <summary>
    /// Непосредственный руководитель пользователя.
    /// </summary>
    public int? ManagerId { get; set; }

    public User? Manager { get; set; }

    /// <summary>
    /// Пользователи, для которых этот пользователь является руководителем.
    /// </summary>
    public ICollection<User> Subordinates { get; set; } = [];

    /// <summary>
    /// Роли пользователя.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// Задачи, созданные пользователем.
    /// </summary>
    public ICollection<TaskItem> CreatedTasks { get; set; } = [];

    /// <summary>
    /// Задачи, назначенные пользователю.
    /// </summary>
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];

    /// <summary>
    /// Комментарии пользователя к задачам.
    /// </summary>
    public ICollection<TaskComment> TaskComments { get; set; } = [];

    /// <summary>
    /// История изменений задач, выполненных пользователем.
    /// </summary>
    public ICollection<TaskHistory> TaskHistoryEntries { get; set; } = [];
}