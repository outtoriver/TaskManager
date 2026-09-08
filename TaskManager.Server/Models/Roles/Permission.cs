namespace TaskManager.Server.Models.Roles;

public class Permission
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}