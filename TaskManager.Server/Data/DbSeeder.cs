using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;
using TaskManager.Server.Models.Roles;

namespace TaskManager.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(db, cancellationToken);
        await SeedPermissionsAsync(db, cancellationToken);
        await SeedRolePermissionsAsync(db, cancellationToken);
    }

    private static async Task SeedRolesAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            new Role
            {
                Name = RoleNames.Administrator,
                Description = "Полный доступ к системе",
                IsSystemRole = true
            },

            new Role
            {
                Name = RoleNames.Manager,
                Description = "Руководитель подразделения",
                IsSystemRole = true
            },

            new Role
            {
                Name = RoleNames.User,
                Description = "Обычный сотрудник",
                IsSystemRole = true
            }
        };

        foreach (var role in roles)
        {
            var exists = await db.Roles
                .AnyAsync(
                    x => x.Name == role.Name,
                    cancellationToken);

            if (!exists)
            {
                db.Roles.Add(role);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPermissionsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var permissions = new[]
        {
            // Users
            new Permission
            {
                Code = PermissionCodes.UsersView,
                Name = "Просмотр пользователей",
                Description = "Просмотр списка и данных пользователей"
            },

            new Permission
            {
                Code = PermissionCodes.UsersCreate,
                Name = "Создание пользователей",
                Description = "Создание новых пользователей"
            },

            new Permission
            {
                Code = PermissionCodes.UsersEdit,
                Name = "Редактирование пользователей",
                Description = "Изменение данных пользователей"
            },

            new Permission
            {
                Code = PermissionCodes.UsersDelete,
                Name = "Удаление пользователей",
                Description = "Деактивация пользователей"
            },

            // Departments
            new Permission
            {
                Code = PermissionCodes.DepartmentsView,
                Name = "Просмотр отделов",
                Description = "Просмотр списка отделов"
            },

            new Permission
            {
                Code = PermissionCodes.DepartmentsManage,
                Name = "Управление отделами",
                Description = "Создание, изменение и деактивация отделов"
            },

            // Positions
            new Permission
            {
                Code = PermissionCodes.PositionsView,
                Name = "Просмотр должностей",
                Description = "Просмотр списка должностей"
            },

            new Permission
            {
                Code = PermissionCodes.PositionsManage,
                Name = "Управление должностями",
                Description = "Создание, изменение и деактивация должностей"
            },

            // Roles
            new Permission
            {
                Code = PermissionCodes.RolesView,
                Name = "Просмотр ролей",
                Description = "Просмотр ролей и их permissions"
            },

            new Permission
            {
                Code = PermissionCodes.RolesManage,
                Name = "Управление ролями",
                Description = "Управление ролями пользователей"
            },

            new Permission
            {
                Code = PermissionCodes.PermissionsManage,
                Name = "Управление permissions",
                Description = "Изменение permissions ролей"
            },

            // Tasks
            new Permission
            {
                Code = PermissionCodes.TasksViewOwn,
                Name = "Просмотр своих задач",
                Description = "Просмотр назначенных пользователю задач"
            },

            new Permission
            {
                Code = PermissionCodes.TasksViewDepartment,
                Name = "Просмотр задач отдела",
                Description = "Просмотр задач пользователей своего подразделения"
            },

            new Permission
            {
                Code = PermissionCodes.TasksViewAll,
                Name = "Просмотр всех задач",
                Description = "Просмотр всех задач системы"
            },

            new Permission
            {
                Code = PermissionCodes.TasksCreate,
                Name = "Создание задач",
                Description = "Создание новых задач"
            },

            new Permission
            {
                Code = PermissionCodes.TasksAssign,
                Name = "Назначение задач",
                Description = "Назначение задач сотрудникам"
            },

            new Permission
            {
                Code = PermissionCodes.TasksEdit,
                Name = "Редактирование задач",
                Description = "Редактирование задач"
            },

            new Permission
            {
                Code = PermissionCodes.TasksDelete,
                Name = "Удаление задач",
                Description = "Удаление или отмена задач"
            },

            new Permission
            {
                Code = PermissionCodes.TasksApprove,
                Name = "Проверка задач",
                Description = "Проверка и принятие выполненных задач"
            },

            // Calendar
            new Permission
            {
                Code = PermissionCodes.CalendarView,
                Name = "Просмотр календаря",
                Description = "Просмотр календаря"
            },

            new Permission
            {
                Code = PermissionCodes.CalendarManage,
                Name = "Управление календарём",
                Description = "Создание и изменение календарных событий"
            },

            // Meetings
            new Permission
            {
                Code = PermissionCodes.MeetingsView,
                Name = "Просмотр встреч",
                Description = "Просмотр встреч"
            },

            new Permission
            {
                Code = PermissionCodes.MeetingsCreate,
                Name = "Создание встреч",
                Description = "Создание встреч"
            },

            new Permission
            {
                Code = PermissionCodes.MeetingsEdit,
                Name = "Редактирование встреч",
                Description = "Изменение встреч"
            },

            new Permission
            {
                Code = PermissionCodes.MeetingsDelete,
                Name = "Удаление встреч",
                Description = "Удаление встреч"
            },

            // Settings
            new Permission
            {
                Code = PermissionCodes.SettingsManage,
                Name = "Управление настройками",
                Description = "Изменение системных настроек"
            }
        };

        foreach (var permission in permissions)
        {
            var exists = await db.Permissions
                .AnyAsync(
                    x => x.Code == permission.Code,
                    cancellationToken);

            if (!exists)
            {
                db.Permissions.Add(permission);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolePermissionsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var administrator = await db.Roles
            .FirstAsync(
                x => x.Name == RoleNames.Administrator,
                cancellationToken);

        var manager = await db.Roles
            .FirstAsync(
                x => x.Name == RoleNames.Manager,
                cancellationToken);

        var user = await db.Roles
            .FirstAsync(
                x => x.Name == RoleNames.User,
                cancellationToken);

        var permissions = await db.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Code,
                cancellationToken);

        var administratorPermissions = permissions.Values
            .Select(x => x.Id)
            .ToArray();

        var managerPermissionCodes = new[]
        {
            PermissionCodes.UsersView,

            PermissionCodes.DepartmentsView,

            PermissionCodes.PositionsView,

            PermissionCodes.TasksViewOwn,
            PermissionCodes.TasksViewDepartment,
            PermissionCodes.TasksCreate,
            PermissionCodes.TasksAssign,
            PermissionCodes.TasksEdit,
            PermissionCodes.TasksApprove,

            PermissionCodes.CalendarView,
            PermissionCodes.CalendarManage,

            PermissionCodes.MeetingsView,
            PermissionCodes.MeetingsCreate,
            PermissionCodes.MeetingsEdit
        };

        var managerPermissions = managerPermissionCodes
            .Select(code => permissions[code].Id)
            .ToArray();

        var userPermissionCodes = new[]
        {
            PermissionCodes.UsersView,

            PermissionCodes.DepartmentsView,

            PermissionCodes.PositionsView,

            PermissionCodes.TasksViewOwn,
            PermissionCodes.TasksCreate,
            PermissionCodes.TasksEdit,

            PermissionCodes.CalendarView,

            PermissionCodes.MeetingsView,
            PermissionCodes.MeetingsCreate
        };

        var userPermissions = userPermissionCodes
            .Select(code => permissions[code].Id)
            .ToArray();

        await AddPermissionsAsync(
            db,
            administrator.Id,
            administratorPermissions,
            cancellationToken);

        await AddPermissionsAsync(
            db,
            manager.Id,
            managerPermissions,
            cancellationToken);

        await AddPermissionsAsync(
            db,
            user.Id,
            userPermissions,
            cancellationToken);
    }

    private static async Task AddPermissionsAsync(
        ApplicationDbContext db,
        int roleId,
        IEnumerable<int> permissionIds,
        CancellationToken cancellationToken)
    {
        var permissionIdSet = permissionIds
            .Distinct()
            .ToHashSet();

        var existing = await db.RolePermissions
            .Where(x =>
                x.RoleId == roleId &&
                permissionIdSet.Contains(x.PermissionId))
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet();

        foreach (var permissionId in permissionIdSet)
        {
            if (existingSet.Contains(permissionId))
            {
                continue;
            }

            db.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}