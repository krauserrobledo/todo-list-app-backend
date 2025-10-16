using Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .IsRequired()
                .HasMaxLength(36); // UUID length
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(u => u.Email)
                .IsUnique();
            builder.HasMany(u => u.Tasks)
                .WithOne(t => t.CreatedBy)
                .HasForeignKey("UserId") // Shadow property for FK
                .IsRequired()
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasMany(u => u.Categories)
                .WithOne(c => c.CreatedBy)
                .HasForeignKey("UserId") // Shadow property for FK
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Tags)
                .WithOne(t => t.CreatedBy)
                .HasForeignKey("UserId") // Shadow property for FK
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Additional configurations as needed

        }
    }
}
