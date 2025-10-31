using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Task = Domain.Models.Task;

namespace Data.Identity
{
    /// <summary>
    /// Represents an application user with additional profile information and navigation properties.
    /// </summary>
    /// <remarks>This class extends the <see cref="IdentityUser"/> class to include additional attributes such
    /// as the user's name and relationships to tasks, categories, and tags. It is typically used in applications that
    /// require user management and role-based authentication.</remarks>
    public class ApplicationUser : IdentityUser
    {

        // Attributes
        public string? Name { get; set; }

        // Navigation properties
        public virtual ICollection<Task> Tasks { get; set; } = [];
        public virtual ICollection<Category> Categories { get; set; } = [];
        public virtual ICollection<Tag> Tags { get; set; } = [];
    }
}
