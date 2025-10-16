namespace Domain.Models;

public class TaskCategory
{
    public required string TaskId { get; set; }

    public required string CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;

}
