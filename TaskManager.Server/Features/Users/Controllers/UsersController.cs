using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Users.DTOs;
using TaskManager.Server.Features.Users.Services;

namespace TaskManager.Server.Features.Users.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(
            id,
            cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{id:int}/subordinates")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemDto>>>
        GetSubordinates(
            int id,
            CancellationToken cancellationToken)
    {
        var users = await userService.GetSubordinatesAsync(
            id,
            cancellationToken);

        return Ok(users);
    }

    [HttpPost]
    [ProducesResponseType(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailsDto>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await userService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}