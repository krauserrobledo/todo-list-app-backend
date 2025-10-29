using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    /// <summary>
    /// Configures the entity type <see cref="TaskCategory"/> for the database context.
    /// </summary>
    /// <remarks>This configuration defines the composite primary key for the <see cref="TaskCategory"/> entity, 
    /// also sets up required properties with maximum lengths, and establishes relationships with the <see cref="Task"/>
    /// and <see cref="Category"/> entities. It ensures cascading deletes for related entities and creates a unique index
    /// </remarks>
    public class TaskCategoryConfiguration : IEntityTypeConfiguration<TaskCategory>
    {
        public void Configure(EntityTypeBuilder<TaskCategory> builder)
        {
            // Table name
            builder.ToTable("TaskCategories");

            // PK
            builder.HasKey(tc => new { tc.TaskId, tc.CategoryId });

            // Configure properties
            builder.Property(tc => tc.TaskId)
                .IsRequired()
                .HasMaxLength(450);
            builder.Property(tc => tc.CategoryId)
                .IsRequired()
                .HasMaxLength(450);

            // Relationships
            builder.HasOne(tc => tc.Task)
                .WithMany(t => t.TaskCategories)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tc => tc.Category)
                .WithMany(c => c.TaskCategories)
                .HasForeignKey(tc => tc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(tc => new { tc.TaskId, tc.CategoryId })
                .IsUnique();
        }
    }
}
