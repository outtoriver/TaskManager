using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Models.Departments;

public class Department
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
}