namespace TaskManager.Server.Services.CurrentUser;

public interface ICurrentUserService
{
    int? UserId { get; }

    string? Login { get; }

    bool IsAuthenticated { get; }

    Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        string role,
        CancellationToken cancellationToken = default);
}
