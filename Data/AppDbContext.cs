using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Domain.Models.Task> Tasks { get; set; } = null!;

        public DbSet<Subtask> Subtasks { get; set; } = null!;

        public DbSet<Tag> Tags { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<TaskTag> TaskTags { get; set; } = null!;

        public DbSet<TaskCategory> TaskCategories { get; set; } = null!;
        /**
        src/
├── Domain/
│   ├── Entities/           # models (User, Task, etc.)
│   └── Interfaces/         # IAppDbContext, repository
├── Infrastructure/
│   └── Data/
│       ├── Configurations/ # 
│       ├── AppDbContext.cs # 
│       └── Repositories/
└── Web/                    # API 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskId, tt.TagId });
            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId);
            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId);
            modelBuilder.Entity<TaskCategory>()
                .HasKey(tc => new { tc.TaskId, tc.CategoryId });
            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskCategories)
                .HasForeignKey(tc => tc.TaskId);
            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.Category)
                .WithMany(c => c.TaskCategories)
                .HasForeignKey(tc => tc.CategoryId);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.CreatedBy)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tags)
                .WithOne(t => t.CreatedBy)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.CreatedBy)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        **/
    }
}
