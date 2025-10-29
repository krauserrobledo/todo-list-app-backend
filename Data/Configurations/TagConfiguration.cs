using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    /// <summary>
    /// Configures the entity type <see cref="Tag"/> for the database context.
    /// </summary>
    /// <remarks>This configuration defines the table name, primary key, property constraints, relationships,
    /// and indexes for the <see cref="Tag"/> entity. It ensures that each tag has a unique combination of name and user
    /// ID, and enforces cascading deletes for related <see cref="TaskTag"/> entities.</remarks>
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            // Table name
            builder.ToTable("Tags");

            // PK
            builder.HasKey(t => t.Id);

            // Configure properties
            builder.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // Relationships
            builder.HasMany(t => t.TaskTags)
                .WithOne(tt => tt.Tag)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => new { t.Name, t.UserId })
                .IsUnique(); // Unique index to prevent duplicate tag names for the same user
        }
    }
}
