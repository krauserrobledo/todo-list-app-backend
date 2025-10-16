namespace Domain.Models;

public class TaskTag
{
    public required string TaskId { get; set; }

    public required string TagId { get; set; }

    public virtual Tag Tag { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
