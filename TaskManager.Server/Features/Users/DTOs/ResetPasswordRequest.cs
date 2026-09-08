namespace TaskManager.Server.Features.Users.DTOs;

public sealed class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public bool ConvertToLocal { get; set; }
}
