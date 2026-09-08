namespace TaskManager.Server.Features.Users.DTOs;

public class UserRoleDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }
}