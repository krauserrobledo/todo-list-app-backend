namespace Domain.Models;

/// <summary>
/// Class representing the many-to-many relationship between Task and Tag
/// </summary>
public class TaskTag
{
    public required string TaskId { get; set; }

    public required string TagId { get; set; }

    public virtual Tag Tag { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
