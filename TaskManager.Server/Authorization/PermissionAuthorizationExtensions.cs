using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.Server.Authorization;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<
            IAuthorizationPolicyProvider,
            PermissionPolicyProvider>();

        services.AddScoped<
            IAuthorizationHandler,
            PermissionAuthorizationHandler>();

        services.AddScoped<
            Services.CurrentUser.ICurrentUserService,
            Services.CurrentUser.CurrentUserService>();

        services.AddAuthorization();

        return services;
    }
}