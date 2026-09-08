using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Common.Constants;
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
        string roleName,
        CancellationToken cancellationToken = default)
    {
        login = login.Trim();
        displayName = displayName.Trim();
        roleName = roleName.Trim();

        ValidateCredentials(
            login,
            displayName,
            password);

        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException(
                "Роль пользователя не может быть пустой.",
                nameof(roleName));
        }

        var exists = await db.Users
            .AnyAsync(
                x => x.Login == login,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Пользователь '{login}' уже существует.");
        }

        var role = await db.Roles
            .FirstOrDefaultAsync(
                x => x.Name == roleName,
                cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException(
                $"Роль '{roleName}' отсутствует в базе данных.");
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

        user.UserRoles.Add(
            new UserRole
            {
                User = user,
                Role = role,
                RoleId = role.Id
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

        if (password.Length < 8)
        {
            throw new ArgumentException(
                "Пароль должен содержать минимум 8 символов.",
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

        user.PasswordHash =
            passwordHasher.HashPassword(
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