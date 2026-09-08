namespace TaskManager.Server.Features.Users.DTOs;

public class UserDetailsDto
{
    public int Id { get; set; }

    public required string Login { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public int? PositionId { get; set; }

    public string? PositionName { get; set; }

    public int? ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public IReadOnlyCollection<UserRoleDto> Roles { get; set; } = [];

    public IReadOnlyCollection<UserListItemDto> Subordinates { get; set; } = [];
}