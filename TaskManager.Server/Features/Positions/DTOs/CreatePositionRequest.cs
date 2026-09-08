namespace TaskManager.Server.Features.Positions.DTOs;

public class CreatePositionRequest
{
    public required string Name { get; set; }

    public bool IsManagerPosition { get; set; }
}