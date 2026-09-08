using Microsoft.AspNetCore.Mvc;
using TaskManager.Server.Authorization;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Features.Departments.DTOs;
using TaskManager.Server.Features.Departments.Services;

namespace TaskManager.Server.Features.Departments.Controllers;

[ApiController]
[Route("api/departments")]
[RequirePermission(PermissionCodes.DepartmentsView)]
public class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentListItemDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await departmentService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var department = await departmentService.GetByIdAsync(id, cancellationToken);
        return department is null ? NotFound() : Ok(department);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.DepartmentsManage)]
    public async Task<ActionResult<DepartmentDetailsDto>> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var department = await departmentService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.DepartmentsManage)]
    public async Task<ActionResult<DepartmentDetailsDto>> Update(int id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var department = await departmentService.UpdateAsync(id, request, cancellationToken);
            return department is null ? NotFound() : Ok(department);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionCodes.DepartmentsManage)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try { return await departmentService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
