using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Features.Positions.DTOs;
using TaskManager.Server.Features.Positions.Services;

namespace TaskManager.Server.Features.Positions.Controllers;

[ApiController]
[Route("api/positions")]
[RequirePermission(PermissionCodes.PositionsView)]
public class PositionsController(IPositionService positionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PositionListItemDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await positionService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var position = await positionService.GetByIdAsync(id, cancellationToken);
        return position is null ? NotFound() : Ok(position);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.PositionsManage)]
    public async Task<ActionResult<PositionDetailsDto>> Create([FromBody] CreatePositionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var position = await positionService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = position.Id }, position);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.PositionsManage)]
    public async Task<ActionResult<PositionDetailsDto>> Update(int id, [FromBody] UpdatePositionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var position = await positionService.UpdateAsync(id, request, cancellationToken);
            return position is null ? NotFound() : Ok(position);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionCodes.PositionsManage)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await positionService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
