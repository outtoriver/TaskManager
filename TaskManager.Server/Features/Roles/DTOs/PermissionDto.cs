namespace TaskManager.Server.Features.Roles.DTOs;

public class PermissionDto
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}