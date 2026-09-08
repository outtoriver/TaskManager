using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Server.Authentication.Models;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;

namespace TaskManager.Server.Authentication.Services;

public sealed class AuthenticationBootstrapService(
    ApplicationDbContext db,
    ILocalAccountService localAccountService,
    IOptions<AuthenticationOptions> options)
{
    public async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var authenticationOptions = options.Value;

        if (!authenticationOptions.BootstrapAdmin.Enabled)
        {
            return;
        }

        var login =
            authenticationOptions.BootstrapAdmin.Login.Trim();

        var exists = await db.Users
            .AnyAsync(
                x => x.Login == login,
                cancellationToken);

        if (exists)
        {
            return;
        }

        await localAccountService.CreateAsync(
            authenticationOptions.BootstrapAdmin.Login,
            authenticationOptions.BootstrapAdmin.DisplayName,
            authenticationOptions.BootstrapAdmin.Password,
            RoleNames.Administrator,
            cancellationToken);
    }
}




