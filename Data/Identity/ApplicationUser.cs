using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Task = Domain.Models.Task;

namespace Data.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Attributes
        public string? Name { get; set; }

        // Navigation properties
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
