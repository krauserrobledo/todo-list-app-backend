using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    /// <summary>
    /// Configures the entity type <see cref="Category"/> for the database context.
    /// </summary>
    /// <remarks>This configuration defines the table name, primary key, property constraints, relationships,
    /// and indexes for the <see cref="Category"/> entity. - The table is mapped to "Categories". - The primary key is
    /// the <see cref="Category.Id"/> property. - The <see cref="Category.Name"/> and <see cref="Category.UserId"/>
    /// properties are required and have maximum lengths of 100 and 450 characters, respectively. - A unique index is
    /// created on the combination of <see cref="Category.Name"/> and <see cref="Category.UserId"/> to ensure that
    /// category names are unique per user. - A one-to-many relationship is configured between <see cref="Category"/>
    /// and <see cref="TaskCategory"/>, with cascading deletes.</remarks>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Table name
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
