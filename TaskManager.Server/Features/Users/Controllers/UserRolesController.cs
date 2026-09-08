using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Features.Users.Services;

namespace TaskManager.Server.Features.Users.Controllers;

[ApiController]
[Route("api/users/{userId:int}/roles")]
[RequirePermission(PermissionCodes.UsersView)]
public class UserRolesController(IUserRoleService userRoleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserRoleDto>>> GetRoles(int userId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await userRoleService.GetRolesAsync(userId, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut]
    [RequirePermission(PermissionCodes.RolesManage)]
    public async Task<IActionResult> UpdateRoles(int userId, [FromBody] UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await userRoleService.UpdateRolesAsync(userId, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
