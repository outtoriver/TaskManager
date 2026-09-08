using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Enums.Users;
using TaskManager.Server.Models.Roles;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Authentication.Services;

public sealed class LocalAccountService(
    ApplicationDbContext db,
    IPasswordHasher<User> passwordHasher)
    : ILocalAccountService
{
    public async Task<User> CreateAsync(
        string login,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        login = login.Trim();
        displayName = displayName.Trim();

        ValidateCredentials(login, displayName, password);

        var exists = await db.Users
            .AnyAsync(
                x => x.Login == login,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Пользователь '{login}' уже существует.");
        }

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(
                x => x.Name == "Administrator",
                cancellationToken);

        if (adminRole is null)
        {
            throw new InvalidOperationException(
                "Роль Administrator отсутствует в базе данных.");
        }

        var user = new User
        {
            Login = login,
            DisplayName = displayName,
            AuthenticationType = AuthenticationType.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            password);

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = adminRole,
            RoleId = adminRole.Id
        });

        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<bool> SetPasswordAsync(
        int userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Пароль не может быть пустым.",
                nameof(password));
        }

        var user = await db.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            password);

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateCredentials(
        string login,
        string displayName,
        string password)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException(
                "Логин не может быть пустым.",
                nameof(login));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Имя пользователя не может быть пустым.",
                nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Пароль не может быть пустым.",
                nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException(
                "Пароль должен содержать минимум 8 символов.",
                nameof(password));
        }
    }
}