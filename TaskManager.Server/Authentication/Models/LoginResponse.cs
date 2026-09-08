namespace TaskManager.Server.Authentication.Models;

public sealed class LoginResponse
{
    public int Id { get; set; }

    public required string Login { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public string? DepartmentName { get; set; }

    public string? PositionName { get; set; }

    public string? ManagerName { get; set; }

    public bool IsActive { get; set; }

    public required string AuthenticationType { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = [];

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}
