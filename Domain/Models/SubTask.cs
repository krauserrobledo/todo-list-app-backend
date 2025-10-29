namespace Domain.Models;

/// <summary>
/// Class representing a Subtask entity
/// </summary>
public class Subtask
{

    public required string Id { get; set; }

    public required string TaskId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Task Task { get; set; } = null!;

}
