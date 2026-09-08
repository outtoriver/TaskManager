using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Server.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string Prefix = "Permission:";

    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException(
                "Permission cannot be null or empty.",
                nameof(permission));
        }

        Permission = permission;
        Policy = $"{Prefix}{permission}";
    }
}