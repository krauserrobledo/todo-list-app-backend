using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    /// <summary>
    /// AppDbContext for Entity Framework Core, integrating IdentityDbContext for user management.
    /// </summary>
    /// <param name="options">DbContextOptions for configuration.</param>
    public class AppDbContext(DbContextOptions options) : IdentityDbContext(options)
    {
        // DbSet for Tasks
        public DbSet<Domain.Models.Task> Tasks { get; set; } = null!;
        // DbSet for Subtasks
        public DbSet<Subtask> Subtasks { get; set; } = null!;
        // DbSet for Tags
        public DbSet<Tag> Tags { get; set; } = null!;
        // DbSet for Categories
        public DbSet<Category> Categories { get; set; } = null!;
        // DbSet for TaskTags (many-to-many relationship)
        public DbSet<TaskTag> TaskTags { get; set; } = null!;
        // DbSet for TaskCategories (many-to-many relationship)
        public DbSet<TaskCategory> TaskCategories { get; set; } = null!;
        // Apply all configurations from the current assembly
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
     }    
}
        
