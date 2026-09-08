using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
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
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================
        // Services
        // ============================================

        builder.Services.AddControllers();

        builder.Services.AddOpenApi();

        builder.Services.Configure<AuthenticationOptions>(
            builder.Configuration.GetSection(
                AuthenticationOptions.SectionName));

        // ============================================
        // Database
        // ============================================

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        // ============================================
        // Authentication
        // ============================================

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    "TaskManagerCookie";

                options.DefaultSignInScheme =
                    "TaskManagerCookie";

                options.DefaultChallengeScheme =
                    "TaskManagerCookie";
            })
            .AddCookie(AuthenticationConstants.ApplicationCookieScheme, options =>
            {
                options.Cookie.Name = AuthenticationConstants.ApplicationCookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddNegotiate(
                NegotiateDefaults.AuthenticationScheme,
                _ => { });

        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        // ============================================
        // Authorization
        // ============================================

        builder.Services.AddPermissionAuthorization();

        // ============================================
        // Application services
        // ============================================

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IPositionService, PositionService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();
        builder.Services.AddScoped<ILocalAccountService, LocalAccountService>();

        builder.Services.AddScoped<AuthenticationBootstrapService>();

        builder.Services.AddScoped<
            IAppAuthenticationService,
            LocalAuthenticationService>();

        builder.Services.AddScoped<
            IWindowsAuthenticationService,
            WindowsAuthenticationService>();

        // ============================================
        // Application
        // ============================================

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

        app.Run();
    }
}
