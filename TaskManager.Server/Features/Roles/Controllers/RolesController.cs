using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Roles.DTOs;
using TaskManager.Server.Features.Roles.Services;

namespace TaskManager.Server.Features.Roles.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController(IRoleService roleService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<RoleDto>>> GetRoles(
        CancellationToken cancellationToken)
    {
        return Ok(
            await roleService.GetRolesAsync(
                cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetRole(
        int id,
        CancellationToken cancellationToken)
    {
        var role = await roleService.GetRoleAsync(
            id,
            cancellationToken);

        return role is null
            ? NotFound()
            : Ok(role);
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<
        IReadOnlyCollection<PermissionDto>>> GetPermissions(
        CancellationToken cancellationToken)
    {
        return Ok(
            await roleService.GetPermissionsAsync(
                cancellationToken));
    }

    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
        int id,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await roleService.UpdateRolePermissionsAsync(
                id,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}