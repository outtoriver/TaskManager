using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Features.Users.Services;

namespace TaskManager.Server.Features.Users.Controllers;

[ApiController]
[Route("api/users/{userId:int}/roles")]
public class UserRolesController(
    IUserRoleService userRoleService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<UserRoleDto>>> GetRoles(
        int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var roles = await userRoleService.GetRolesAsync(
                userId,
                cancellationToken);

            return Ok(roles);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRoles(
        int userId,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await userRoleService.UpdateRolesAsync(
                userId,
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