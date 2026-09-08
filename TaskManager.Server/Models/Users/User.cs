using TaskManager.Server.Enums.Tasks;
using TaskManager.Server.Models.Departments;
using TaskManager.Server.Models.Positions;
using TaskManager.Server.Models.Roles;

namespace TaskManager.Server.Models.Users;

public class User
{
    public int Id { get; set; }

    public required string Login { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public AuthenticationType AuthenticationType { get; set; }

    public string? PasswordHash { get; set; }

    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int? PositionId { get; set; }

    public Position? Position { get; set; }

    public int? ManagerId { get; set; }

    public User? Manager { get; set; }

    public ICollection<User> Subordinates { get; set; } = [];

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}