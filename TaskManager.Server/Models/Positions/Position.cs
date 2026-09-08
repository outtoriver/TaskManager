using TaskManager.Server.Models.Users;

namespace TaskManager.Server.Models.Positions;

public class Position
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsManagerPosition { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
}