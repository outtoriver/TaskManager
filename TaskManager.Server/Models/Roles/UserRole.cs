using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Models.Roles;

public class UserRole
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;
}