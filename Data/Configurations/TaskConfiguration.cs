using Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks = Domain.Models.Task;

namespace Data.Configurations
{
    /// <summary>
    /// Configures the entity type <see cref="Tasks"/> for the database context.
    /// </summary>
    /// <remarks>This configuration defines the table name, primary key, property constraints, relationships, 
    /// 
    public class TaskConfiguration : IEntityTypeConfiguration<Tasks>
    {
        public void Configure(EntityTypeBuilder<Tasks> builder)
        {
            // Table name
            builder.ToTable("Tasks");

            // PK
            builder.HasKey(t => t.Id);

            // Configure properties
            builder.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.DueDate)
                .IsRequired(false);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Non Started");

            // Relationships
            builder.HasOne<ApplicationUser>()
                .WithMany(u => u.Tasks)
                .HasForeignKey("UserId") // Shadow property for FK
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TaskCategories)
                .WithOne(tc => tc.Task)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TaskTags)
                .WithOne(tt => tt.Task)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Subtasks)
                .WithOne(st => st.Task)
                .HasForeignKey(st => st.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => new { t.Title, t.UserId })
                .IsUnique(); // Unique index to prevent duplicate task titles for the same user
        }
    }
}
