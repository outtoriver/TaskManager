namespace TaskManager.Server.Authentication.Models;

public sealed class LoginRequest
{
    public required string Login { get; set; }

    public required string Password { get; set; }
}
