

namespace Domain.Models;


public class Category
{
    public required string Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Color { get; set; }

    public required string UserId { get; set; }

    public virtual ICollection<TaskCategory> TaskCategories { get; set; } = new List<TaskCategory>();

}