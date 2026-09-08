namespace TaskManager.Server.Models.Roles;

public class Role
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}