using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Positions.DTOs;
using TaskManager.Server.Features.Positions.Services;

namespace TaskManager.Server.Features.Positions.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController(
    IPositionService positionService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<PositionListItemDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var positions = await positionService
            .GetAllAsync(cancellationToken);

        return Ok(positions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var position = await positionService
            .GetByIdAsync(
                id,
                cancellationToken);

        if (position is null)
        {
            return NotFound();
        }

        return Ok(position);
    }

    [HttpPost]
    public async Task<ActionResult<PositionDetailsDto>> Create(
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var position = await positionService
                .CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = position.Id },
                position);
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
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PositionDetailsDto>> Update(
        int id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var position = await positionService
                .UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            if (position is null)
            {
                return NotFound();
            }

            return Ok(position);
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
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await positionService
                .DeleteAsync(
                    id,
                    cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}