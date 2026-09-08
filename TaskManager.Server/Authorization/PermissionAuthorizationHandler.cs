using Microsoft.AspNetCore.Authorization;
using TaskManager.Server.Services.CurrentUser;

namespace TaskManager.Server.Authorization;

public sealed class PermissionAuthorizationHandler(
    ICurrentUserService currentUserService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var hasPermission = await currentUserService
            .HasPermissionAsync(requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}