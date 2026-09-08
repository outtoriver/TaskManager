using TaskManager.Server.Features.Users.DTOs;

namespace TaskManager.Server.Features.Positions.DTOs;

public class PositionDetailsDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsManagerPosition { get; set; }

    public bool IsActive { get; set; }

    public int UserCount { get; set; }

    public IReadOnlyCollection<UserListItemDto> Users { get; set; } = [];
}