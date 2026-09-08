using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TaskManager.Server.Authorization;

public sealed class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string Prefix = "Permission:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        if (policyName.StartsWith(
                Prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[Prefix.Length..];

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(
                    new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}