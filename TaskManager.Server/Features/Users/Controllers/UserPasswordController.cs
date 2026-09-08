using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Enums.Users;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Models.Users;
using TaskManager.Server.Data;

namespace TaskManager.Server.Features.Users.Controllers;

[ApiController]
[Route("api/users/{userId:int}/password")]
[RequirePermission(PermissionCodes.UsersEdit)]
public sealed class UserPasswordController(
    ApplicationDbContext db,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Reset(int userId, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "Пароль должен содержать минимум 8 символов." });

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(new { message = "Пользователь не найден." });

        if (user.AuthenticationType == AuthenticationType.Windows && !request.ConvertToLocal)
            return BadRequest(new { message = "Windows-пользователю пароль TaskManager не назначается. Включите преобразование в локальную учётную запись, если это необходимо." });

        if (request.ConvertToLocal) user.AuthenticationType = AuthenticationType.Local;
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
