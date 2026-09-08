using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Models.Departments;
using TaskManager.Server.Models.Positions;
using TaskManager.Server.Models.Roles;
using TaskManager.Server.Models.Tasks;
using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    public DbSet<TaskHistory> TaskHistory => Set<TaskHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}