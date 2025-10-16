using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
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
