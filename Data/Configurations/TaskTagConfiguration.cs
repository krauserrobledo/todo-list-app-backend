using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
    {
        public void Configure(EntityTypeBuilder<TaskTag> builder)
        {
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