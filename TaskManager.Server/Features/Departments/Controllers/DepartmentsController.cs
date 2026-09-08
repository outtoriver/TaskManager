using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Features.Departments.DTOs;
using TaskManager.Server.Features.Departments.Services;

namespace TaskManager.Server.Features.Departments.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController(
    IDepartmentService departmentService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyCollection<DepartmentListItemDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var departments = await departmentService
            .GetAllAsync(cancellationToken);

        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var department = await departmentService
            .GetByIdAsync(
                id,
                cancellationToken);

        if (department is null)
        {
            return NotFound();
        }

        return Ok(department);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDetailsDto>> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await departmentService
                .CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = department.Id },
                department);
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
    public async Task<ActionResult<DepartmentDetailsDto>> Update(
        int id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await departmentService
                .UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            if (department is null)
            {
                return NotFound();
            }

            return Ok(department);
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
            var deleted = await departmentService
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