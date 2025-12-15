namespace Domain.Models;

/// <summary>
/// Class representing the many-to-many relationship between Task and Category
/// </summary>
public class TaskCategory
{

    public required string TaskId { get; set; }

    public required string CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;

}
