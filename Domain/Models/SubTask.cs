namespace Domain.Models;

public class Subtask
{
    public required string Id { get; set; }

    public required string TaskId { get; set; }

    public string Title { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;


}
