using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class SubtaskConfiguration : IEntityTypeConfiguration<Subtask>
    {
        public void Configure(EntityTypeBuilder<Subtask> builder)
        {
            // Table name
            builder.ToTable("Subtasks");
            // Primary Key
            builder.HasKey(st => st.Id);
            // Configure properties
            builder.Property(st => st.Id)
                .IsRequired()
                .HasMaxLength(450);
            builder.Property(st => st.Title)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(st => st.TaskId)
                .IsRequired()
                .HasMaxLength(450);
            // Relationships
            builder.HasOne(st => st.Task)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(st => st.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            // Indexes
            builder.HasIndex(st => st.TaskId)
                .IsUnique();

        }
    }
}
