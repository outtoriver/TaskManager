namespace TaskManager.Server.Authentication.Models;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public List<string> WindowsAdministrators { get; set; } = [];
}
