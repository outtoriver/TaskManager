using TaskManager.Server.Features.Users.DTOs;

namespace TaskManager.Server.Features.Departments.DTOs;

public class DepartmentDetailsDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int UserCount { get; set; }

    public IReadOnlyCollection<UserListItemDto> Users { get; set; } = [];
}