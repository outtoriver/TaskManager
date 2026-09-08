namespace TaskManager.Server.Features.Users.DTOs;

public class UpdateUserRolesRequest
{
    public IReadOnlyCollection<int> RoleIds { get; set; } = [];
}