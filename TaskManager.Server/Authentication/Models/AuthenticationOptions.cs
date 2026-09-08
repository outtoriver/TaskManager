namespace TaskManager.Server.Authentication.Models;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public List<string> WindowsAdministrators { get; set; } = [];

    public BootstrapAdminOptions BootstrapAdmin { get; set; } = new();
}

public sealed class BootstrapAdminOptions
{
    public bool Enabled { get; set; }

    public string Login { get; set; } = "admin";

    public string DisplayName { get; set; } = "System Administrator";

    public string Password { get; set; } = string.Empty;
}