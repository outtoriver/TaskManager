using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Features.Users.Services;

namespace TaskManager.Server.Features.Users.Controllers;

[ApiController]
[Route("api/users")]
[RequirePermission(PermissionCodes.UsersView)]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await userService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("{id:int}/subordinates")]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemDto>>> GetSubordinates(int id, CancellationToken cancellationToken)
        => Ok(await userService.GetSubordinatesAsync(id, cancellationToken));

    [HttpPost]
    [RequirePermission(PermissionCodes.UsersCreate)]
    public async Task<ActionResult<UserDetailsDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.UsersEdit)]
    public async Task<ActionResult<UserDetailsDto>> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(id, request, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionCodes.UsersDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await userService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
