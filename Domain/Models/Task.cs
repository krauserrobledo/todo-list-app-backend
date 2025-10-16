namespace Domain.Models;

public class Task
{
    public required string Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();

    public virtual ICollection<TaskCategory> TaskCategories { get; set; } = new List<TaskCategory>();

    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();

    public virtual User? CreatedBy { get; set; } = null!;

}
