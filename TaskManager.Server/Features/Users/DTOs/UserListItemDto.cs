namespace TaskManager.Server.Features.Users.DTOs;

public class UserListItemDto
{
    public int Id { get; set; }

    public required string Login { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public int? PositionId { get; set; }

    public string? PositionName { get; set; }

    public int? ManagerId { get; set; }

    public string? ManagerName { get; set; }
}