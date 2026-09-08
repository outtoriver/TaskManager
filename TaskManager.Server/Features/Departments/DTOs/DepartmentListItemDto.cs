namespace TaskManager.Server.Features.Departments.DTOs;

public class DepartmentListItemDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int UserCount { get; set; }
}