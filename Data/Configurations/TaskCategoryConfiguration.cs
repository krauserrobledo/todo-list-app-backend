using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Configurations
{
    public class TaskCategoryConfiguration : IEntityTypeConfiguration<TaskCategory>
    {
        public void Configure(EntityTypeBuilder<TaskCategory> builder)
        {
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
