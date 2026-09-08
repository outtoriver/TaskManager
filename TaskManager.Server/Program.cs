using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Authentication.Services;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Departments.Services;
using TaskManager.Server.Features.Positions.Services;
using TaskManager.Server.Features.Roles.Services;
using TaskManager.Server.Features.Users.Services;
using TaskManager.Server.Models.Users;
using TaskManager.Server.Services.CurrentUser;

namespace TaskManager.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================
        // Controllers / OpenAPI
        // ============================================================

        builder.Services.AddControllers();

        builder.Services.AddOpenApi();

        // ============================================================
        // Database
        // ============================================================

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        // ============================================================
        // Password hashing
        // ============================================================

        builder.Services.AddScoped<
            IPasswordHasher<User>,
            PasswordHasher<User>>();

        // ============================================================
        // Authentication
        // ============================================================

        builder.Services
            .AddAuthentication()
            .AddCookie(
                AuthenticationConstants.CookieScheme,
                options =>
                {
                    options.Cookie.Name =
                        AuthenticationConstants.CookieName;

                    options.LoginPath =
                        "/api/auth/login";

                    options.AccessDeniedPath =
                        "/api/auth/forbidden";

                    options.SlidingExpiration = true;

                    options.ExpireTimeSpan =
                        TimeSpan.FromHours(8);

                    options.Cookie.HttpOnly = true;

                    options.Cookie.SameSite =
                        SameSiteMode.Lax;

                    options.Cookie.SecurePolicy =
                        CookieSecurePolicy.Always;
                })
            .AddNegotiate(
                AuthenticationConstants.WindowsScheme,
                options =>
                {
                });

        // ============================================================
        // Authorization
        // ============================================================

        builder.Services.AddPermissionAuthorization();

        // ============================================================
        // Application services
        // ============================================================

        builder.Services.AddScoped<
            IUserService,
            UserService>();

        builder.Services.AddScoped<
            IDepartmentService,
            DepartmentService>();

        builder.Services.AddScoped<
            IPositionService,
            PositionService>();

        builder.Services.AddScoped<
            IRoleService,
            RoleService>();

        builder.Services.AddScoped<
            IUserRoleService,
            UserRoleService>();

        // ============================================================
        // Authentication services
        // ============================================================

        builder.Services.AddScoped<
            IAppAuthenticationService,
            LocalAuthenticationService>();

        builder.Services.AddScoped<
            IWindowsAuthenticationService,
            WindowsAuthenticationService>();

        builder.Services.AddScoped<
            ILocalAccountService,
            LocalAccountService>();

        builder.Services.AddScoped<
            AuthenticationBootstrapService>();

        // ============================================================
        // Application
        // ============================================================

        var app = builder.Build();

        app.UseDefaultFiles();

        app.MapStaticAssets();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        // ============================================================
        // Database seed
        //
        // Временно запускаем seed один раз.
        // После проверки уберём вызов из startup.
        // ============================================================

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            await DbSeeder.SeedAsync(db);
        }

        app.Run();
    }
}