using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    /// <summary>
    /// Configures the entity type <see cref="TaskTag"/> for the database context.
    /// </summary>
    /// <remarks>This configuration defines the composite primary key for the <see cref="TaskTag"/> entity,
    /// sets up required properties with maximum lengths, and establishes relationships with the <see cref="Task"/> and
    /// <see cref="Tag"/> entities. It also ensures cascading deletes for related entities and creates a unique index on
    /// the composite key.</remarks>
    public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
    {
        public void Configure(EntityTypeBuilder<TaskTag> builder)
        {
            // Table name
            builder.ToTable("TaskTags");
            // PK 
            builder.HasKey(tt => new { tt.TaskId, tt.TagId });
            // Configure properties
            builder.Property(tt => tt.TaskId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(tt => tt.TagId)
                .IsRequired()
                .HasMaxLength(450);
            // Relationships
            builder.HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
            // Indexes
            builder.HasIndex(tt => new { tt.TaskId, tt.TagId })
            .IsUnique();
        }
    }
}