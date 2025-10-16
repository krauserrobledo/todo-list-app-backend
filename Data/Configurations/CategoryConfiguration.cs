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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            // PK
            builder.HasKey(c => c.Id);
            // Configure properties
            builder.Property(c => c.Id)
                .IsRequired()
                .HasMaxLength(450);
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(450);
            // Relationships
            builder.HasMany(c => c.TaskCategories)
                .WithOne(tc => tc.Category)
                .HasForeignKey(tc => tc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // Indexes
            builder.HasIndex(c => new { c.Name, c.UserId })
                .IsUnique(); // Unique index to prevent duplicate category names for the same user
        }
    }
}
