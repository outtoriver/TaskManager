using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Authentication.Models;
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
        // Authentication options
        // ============================================================

        builder.Services.Configure<AuthenticationOptions>(
            builder.Configuration.GetSection(
                AuthenticationOptions.SectionName));

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
        // HTTP context / password hashing
        // ============================================================

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<
            IPasswordHasher<User>,
            PasswordHasher<User>>();

        // ============================================================
        // Authentication
        // ============================================================

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    AuthenticationConstants.ApplicationCookieScheme;

                options.DefaultSignInScheme =
                    AuthenticationConstants.ApplicationCookieScheme;

                options.DefaultChallengeScheme =
                    AuthenticationConstants.ApplicationCookieScheme;
            })
            .AddCookie(
                AuthenticationConstants.ApplicationCookieScheme,
                options =>
                {
                    options.Cookie.Name =
                        AuthenticationConstants.ApplicationCookieName;

                    options.LoginPath = "/api/auth/login";
                    options.AccessDeniedPath = "/api/auth/forbidden";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode =
                                StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode =
                                StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                })
            .AddNegotiate(
                AuthenticationConstants.WindowsScheme,
                _ => { });

        // ============================================================
        // Authorization
        // ============================================================

        builder.Services.AddPermissionAuthorization();

        // ============================================================
        // Current user
        // ============================================================

        builder.Services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();

        // ============================================================
        // Application services
        // ============================================================

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IPositionService, PositionService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();

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

        builder.Services.AddScoped<AuthenticationBootstrapService>();

        // ============================================================
        // Application
        // ============================================================

        var app = builder.Build();

        // ============================================================
        // Database initialization
        // ============================================================

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();
            await DbSeeder.SeedAsync(db);

            var bootstrap = scope.ServiceProvider
                .GetRequiredService<AuthenticationBootstrapService>();

            await bootstrap.ExecuteAsync();
        }

        // ============================================================
        // HTTP pipeline
        // ============================================================

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

        await app.RunAsync();
    }
}
