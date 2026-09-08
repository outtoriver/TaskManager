using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Models;

namespace TaskManager.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        await SeedRolesAsync(db);
        await SeedPermissionsAsync(db);
        await SeedRolePermissionsAsync(db);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext db)
    {
        var roles = new[]
        {
            new Role
            {
                Id = 1,
                Name = "Administrator",
                Description = "Полный доступ к системе",
                IsSystemRole = true
            },
            new Role
            {
                Id = 2,
                Name = "Manager",
                Description = "Руководитель",
                IsSystemRole = true
            },
            new Role
            {
                Id = 3,
                Name = "User",
                Description = "Обычный сотрудник",
                IsSystemRole = true
            }
        };

        foreach (var role in roles)
        {
            if (!await db.Roles.AnyAsync(x => x.Id == role.Id))
            {
                db.Roles.Add(role);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext db)
    {
        var permissions = new[]
        {
            new Permission
            {
                Id = 1,
                Code = "Users.View",
                Name = "Просмотр пользователей"
            },
            new Permission
            {
                Id = 2,
                Code = "Users.Create",
                Name = "Создание пользователей"
            },
            new Permission
            {
                Id = 3,
                Code = "Users.Edit",
                Name = "Редактирование пользователей"
            },
            new Permission
            {
                Id = 4,
                Code = "Users.Delete",
                Name = "Удаление пользователей"
            },

            new Permission
            {
                Id = 5,
                Code = "Departments.View",
                Name = "Просмотр отделов"
            },
            new Permission
            {
                Id = 6,
                Code = "Departments.Manage",
                Name = "Управление отделами"
            },

            new Permission
            {
                Id = 7,
                Code = "Tasks.ViewOwn",
                Name = "Просмотр своих задач"
            },
            new Permission
            {
                Id = 8,
                Code = "Tasks.ViewDepartment",
                Name = "Просмотр задач отдела"
            },
            new Permission
            {
                Id = 9,
                Code = "Tasks.ViewAll",
                Name = "Просмотр всех задач"
            },
            new Permission
            {
                Id = 10,
                Code = "Tasks.Create",
                Name = "Создание задач"
            },
            new Permission
            {
                Id = 11,
                Code = "Tasks.Assign",
                Name = "Назначение задач"
            },
            new Permission
            {
                Id = 12,
                Code = "Tasks.Edit",
                Name = "Редактирование задач"
            },
            new Permission
            {
                Id = 13,
                Code = "Tasks.Delete",
                Name = "Удаление задач"
            },
            new Permission
            {
                Id = 14,
                Code = "Tasks.Approve",
                Name = "Проверка и принятие задач"
            },

            new Permission
            {
                Id = 15,
                Code = "Calendar.View",
                Name = "Просмотр календаря"
            },
            new Permission
            {
                Id = 16,
                Code = "Calendar.Manage",
                Name = "Управление календарём"
            },

            new Permission
            {
                Id = 17,
                Code = "Meetings.View",
                Name = "Просмотр встреч"
            },
            new Permission
            {
                Id = 18,
                Code = "Meetings.Create",
                Name = "Создание встреч"
            },
            new Permission
            {
                Id = 19,
                Code = "Meetings.Edit",
                Name = "Редактирование встреч"
            },
            new Permission
            {
                Id = 20,
                Code = "Meetings.Delete",
                Name = "Удаление встреч"
            },

            new Permission
            {
                Id = 21,
                Code = "Settings.Manage",
                Name = "Управление настройками"
            }
        };

        foreach (var permission in permissions)
        {
            if (!await db.Permissions.AnyAsync(x => x.Id == permission.Id))
            {
                db.Permissions.Add(permission);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext db)
    {
        var allPermissions = await db.Permissions
            .Select(x => x.Id)
            .ToListAsync();

        var managerPermissions = await db.Permissions
            .Where(x =>
                x.Code.StartsWith("Tasks.") ||
                x.Code.StartsWith("Calendar.") ||
                x.Code.StartsWith("Meetings."))
            .Select(x => x.Id)
            .ToListAsync();

        managerPermissions.Add(1); // Users.View
        managerPermissions.Add(5); // Departments.View

        var userPermissions = await db.Permissions
            .Where(x =>
                x.Code == "Tasks.ViewOwn" ||
                x.Code == "Tasks.Create" ||
                x.Code == "Tasks.Edit" ||
                x.Code == "Calendar.View" ||
                x.Code == "Meetings.View" ||
                x.Code == "Meetings.Create")
            .Select(x => x.Id)
            .ToListAsync();

        await AddRolePermissionsAsync(db, 1, allPermissions);
        await AddRolePermissionsAsync(db, 2, managerPermissions.Distinct().ToList());
        await AddRolePermissionsAsync(db, 3, userPermissions.Distinct().ToList());
    }

    private static async Task AddRolePermissionsAsync(
        ApplicationDbContext db,
        int roleId,
        IEnumerable<int> permissionIds)
    {
        foreach (var permissionId in permissionIds)
        {
            var exists = await db.RolePermissions.AnyAsync(x =>
                x.RoleId == roleId &&
                x.PermissionId == permissionId);

            if (!exists)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }

        await db.SaveChangesAsync();
    }
}