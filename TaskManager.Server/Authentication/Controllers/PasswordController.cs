using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Authentication.Models;
using TaskManager.Server.Data;
using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Users;
using TaskManager.Server.Services.CurrentUser;

namespace TaskManager.Server.Authentication.Controllers;

[ApiController]
[Route("api/auth/password")]
[Authorize]
public sealed class PasswordController(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Change([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "Новый пароль должен содержать минимум 8 символов." });

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
        if (user is null || !user.IsActive) return Unauthorized();
        if (user.AuthenticationType != AuthenticationType.Local)
            return BadRequest(new { message = "Для Windows-учётной записи пароль меняется в Active Directory / Windows." });
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return BadRequest(new { message = "У пользователя не установлен локальный пароль." });

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Текущий пароль указан неверно." });

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
