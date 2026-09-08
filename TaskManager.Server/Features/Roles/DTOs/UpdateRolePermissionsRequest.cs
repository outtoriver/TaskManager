namespace TaskManager.Server.Features.Roles.DTOs;

public class UpdateRolePermissionsRequest
{
    public IReadOnlyCollection<int> PermissionIds { get; set; } = [];
}