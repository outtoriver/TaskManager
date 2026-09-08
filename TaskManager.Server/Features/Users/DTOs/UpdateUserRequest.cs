namespace TaskManager.Server.Features.Users.DTOs;

public class UpdateUserRequest
{
    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? ManagerId { get; set; }

    public bool IsActive { get; set; }
}