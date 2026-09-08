using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Data;
using TaskManager.Server.Features.Users.Services;
using TaskManager.Server.Features.Departments.Services;
using TaskManager.Server.Features.Positions.Services;

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

        // Entity Framework Core + SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        // Application services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IPositionService, PositionService>();

        // ============================================
        // Application
        // ============================================

        var app = builder.Build();

        // React/Vite static files
        app.UseDefaultFiles();
        app.MapStaticAssets();

        // OpenAPI
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        // Authentication пока не подключаем.
        // Добавим Windows + Local authentication следующим этапом.

        app.UseAuthorization();

        // API controllers
        app.MapControllers();

        // React fallback
        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}