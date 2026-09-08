namespace TaskManager.Server.Features.Roles.DTOs;

public class RoleDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public int UserCount { get; set; }

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}