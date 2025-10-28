using Domain.Abstractions;
using Domain.Models;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using Tasks = Domain.Models.Task;

namespace Data.Repositories
{
      public class TaskRepository(AppDbContext context) : ITaskRepository
    {
        // Dependency Injection of AppDbContext
        private readonly AppDbContext _context = context;

        public async Task<Tasks> CreateTask(Tasks task)
        {
            // Get user ID from task 
            // Validate input and check for duplicate
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == task.Title && t.UserId == task.UserId);
            if (existingTask != null)
                throw new InvalidOperationException("A task with the same title already exists for this user.");

            if (!Domain.Constants.TaskStatus.IsValid(task.Status))
            {
                task.Status = Domain.Constants.TaskStatus.NonStarted;
        }

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(task.Id) || string.IsNullOrWhiteSpace(task.Id))
                task.Id = Guid.NewGuid().ToString();
            // Add to DbContext and save changes
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<Tasks?> UpdateTask(Tasks task)
        {
            // Validate input using LINQ
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == task.Id); 
            if (existingTask == null)
                return null;
            // Update properties
            existingTask.Title = task.Title?? existingTask.Title;
            existingTask.Description = task.Description?? existingTask.Description;
            existingTask.DueDate = task.DueDate ?? existingTask.DueDate;
            existingTask.Status = task.Status ?? existingTask.Status;
            // Save changes
            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task<bool> DeleteTask(string taskId)
        {
            // Get task using LINQ
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<Tasks?> GetTaskById(string taskId)
        {
            // Get task using LINQ
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

        }

        public async Task<ICollection<Tasks>> GetTasksByUser(string userId)
        {
            // Get tasks using LINQ
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<ICollection<Tasks>> GetTasksByUserWithDetails(string userId)
        {
            // get tasks with related entities using LINQ
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .Include(t => t.Subtasks)
                .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();
            return tasks;

        }
        
        public async Task<Tasks?> GetTaskWithDetails(string taskId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            // Retrieve task with related entities
            var task = await _context.Tasks
                .Where(t => t.Id == taskId)
                .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync();
            return task;
        }

        public async Task RemoveCategoryFromTask(string taskId, string categoryId)
        {
            // Validate input AND association using LINQ
            var taskCategory = await _context.TaskCategories
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId) ?? throw new InvalidOperationException("The category is not associated with the task.");
            // Remove and save
            _context.TaskCategories.Remove(taskCategory);
            await _context.SaveChangesAsync();
            return;

        }

        public async Task RemoveTagFromTask(string taskId, string tagId)
        {
            // Validate association using LINQ
            var taskTag = await _context.TaskTags
                .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId)
                ?? throw new InvalidOperationException("The tag is not associated with the task.");

            // Remove and save
            _context.TaskTags.Remove(taskTag);
            await _context.SaveChangesAsync();
        }

        public async Task AddCategoryToTask(string taskId, string categoryId)
        {
            // Validate association using LINQ
            var existingAssociation = await _context.TaskCategories
                .AnyAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId);
            if (existingAssociation)
                throw new InvalidOperationException("The category is already associated with the task.");
            // Create new association
            var taskCategory = new TaskCategory()
            {
                TaskId = taskId,
                CategoryId = categoryId
            };
            // Add and save
            _context.TaskCategories.Add(taskCategory);
            await _context.SaveChangesAsync();
            return;

        }
        public async Task AddTagToTask(string taskId, string tagId)
        {
            // Validate input and association using LINQ
            var existingAssociation = await _context.TaskTags
                .AnyAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);
            if (existingAssociation)
                throw new InvalidOperationException("The tag is already associated with the task.");
            // Create new association
            var taskTag = new TaskTag()
            {
                TaskId = taskId,
                TagId = tagId
            };
            // Add and save
            _context.TaskTags.Add(taskTag);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<bool> TaskTitleExists(string title, string userId)
        {
            return await _context.Tasks
                .AnyAsync(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && t.UserId == userId);
        }
    }
}
