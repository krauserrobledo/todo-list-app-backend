namespace Domain.Models;

public class Tag
{
    public required string Id { get; set; }

    public string Name { get; set; } = null!;

    public required string UserId { get; set; }

    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();


}
