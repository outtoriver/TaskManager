using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Roles.DTOs;
using TaskManager.Server.Features.Roles.Services;

namespace TaskManager.Server.Features.Roles.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController(
    IRoleService roleService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<PermissionDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var permissions = await roleService
            .GetPermissionsAsync(
                cancellationToken);

        return Ok(permissions);
    }
}