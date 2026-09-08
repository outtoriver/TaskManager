namespace TaskManager.Server.Features.Departments.DTOs;

public class CreateDepartmentRequest
{
    public required string Name { get; set; }

    public string? Description { get; set; }
}