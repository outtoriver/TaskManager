namespace TaskManager.Server.Features.Positions.DTOs;

public class PositionListItemDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsManagerPosition { get; set; }

    public bool IsActive { get; set; }

    public int UserCount { get; set; }
}