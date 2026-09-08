using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Features.Tasks.Services;

namespace TaskManager.Server.Features.Tasks.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TaskListItemDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await taskService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await taskService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await taskService.UpdateAsync(id, request, cancellationToken);
            return task is null ? NotFound() : Ok(task);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await taskService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> ChangeStatus(
        int id,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await taskService.ChangeStatusAsync(id, request, cancellationToken);
            return task is null ? NotFound() : Ok(task);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/priority")]
    public async Task<ActionResult<TaskDto>> ChangePriority(
        int id,
        [FromBody] ChangeTaskPriorityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await taskService.ChangePriorityAsync(id, request, cancellationToken);
            return task is null ? NotFound() : Ok(task);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TaskCommentDto>> AddComment(
        int id,
        [FromBody] CreateTaskCommentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var comment = await taskService.AddCommentAsync(id, request, cancellationToken);
            return comment is null ? NotFound() : Ok(comment);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
