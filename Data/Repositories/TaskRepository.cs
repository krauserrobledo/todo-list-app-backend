using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using Tasks = Domain.Models.Task;
using Domain.Models;

namespace Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        // Dependency Injection of AppDbContext
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskRepository"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The <see cref="AppDbContext"/> instance used to access the database. Cannot be <see langword="null"/>.</param>
        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Associates a category with a task by creating a new TaskCategory entry in the database. 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task AddCategoryToTask(string taskId, string categoryId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(categoryId));

            // Check if task exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {taskId} does not exist.");

            // Check if category exists
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");

            // Check if the association already exists
            var existingAssociation = await _context.TaskCategories
                .AnyAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId);
            if (existingAssociation)
                throw new InvalidOperationException("The category is already associated with the task.");

            // Create new association

            var taskCategory = new TaskCategory()
            {

                Id = Guid.NewGuid().ToString(),
                TaskId = taskId,
                CategoryId = categoryId
            };

            // Add and save

            _context.TaskCategories.Add(taskCategory);
            await _context.SaveChangesAsync();

            return;

        }
        /// <summary>
        /// Associates a tag with a task by creating a new TaskTag entry in the database. 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task AddTagToTask(string taskId, string tagId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            if (string.IsNullOrEmpty(tagId))
                throw new ArgumentException("Tag ID cannot be null or empty.", nameof(tagId));
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be whitespace.", nameof(tagId));

            // Check if task exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {taskId} does not exist.");
            // Check if tag exists
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null)
                throw new InvalidOperationException($"Tag with ID {tagId} does not exist.");
            // Check if the association already exists
            var existingAssociation = await _context.TaskTags
                .AnyAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);
            if (existingAssociation)
                throw new InvalidOperationException("The tag is already associated with the task.");
            // Create new association
            var taskTag = new TaskTag()
            {
                Id = Guid.NewGuid().ToString(),
                TaskId = taskId,
                TagId = tagId
            };
            // Add and save
            _context.TaskTags.Add(taskTag);
            await _context.SaveChangesAsync();
            return;

        }

        public async Task<Tasks> CreateTask(Tasks task)
        {
            // Validate input
            if (task == null)
                throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be null or whitespace.", nameof(task.Title));
            if (string.IsNullOrWhiteSpace(task.UserId))
                throw new ArgumentException("User ID cannot be null or whitespace.", nameof(task.UserId));

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(task.Id) || string.IsNullOrWhiteSpace(task.Id))
                task.Id = Guid.NewGuid().ToString();
            // Default status if not provided
            if (string.IsNullOrWhiteSpace(task.Status))
                task.Status = "Non Started";

            // Clean up and initialize collections
            task.Title = task.Title.Trim();
            task.UserId = task.UserId.Trim();
            task.Description = task.Description?.Trim();
            task.TaskCategories = new List<TaskCategory>();
            task.TaskTags = new List<TaskTag>();
            task.Subtasks = new List<SubTask>();
            // task.CreatedBy = null!; // Avoid EF Core tracking issues
            // Add and save
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;


        }

        public async Task<bool> DeleteTask(string taskId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            // Find the task by ID
            var task = await _context.Tasks.FindAsync(taskId);
            // If not found, return false
            if (task == null)
                return false;
            // Remove from DbContext and save changes
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Tasks?> GetTaskById(string taskId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            // Retrieve task
            var task = await _context.Tasks.FindAsync(taskId);
            return task;

        }

        public async Task<ICollection<Tasks>> GetTasksByUser(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Retrieve tasks
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
            return tasks;
        }

        public async Task<ICollection<Tasks>> GetTasksByUserWithDetails(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Retrieve tasks with related entities
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();
            return tasks;

        }

        public async Task<ICollection<Tasks>> GetTasksWithCategoriesAndTags(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Retrieve tasks with related entities
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
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
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(categoryId));

            // Find the association
            var taskCategory = await _context.TaskCategories
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId);

            if (taskCategory == null)
                throw new InvalidOperationException("The category is not associated with the task.");

            // Remove and save
            _context.TaskCategories.Remove(taskCategory);
            await _context.SaveChangesAsync();
            return;

        }

        public async Task RemoveTagFromTask(string taskId, string tagId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            if (string.IsNullOrEmpty(tagId))
                throw new ArgumentException("Tag ID cannot be null or empty.", nameof(tagId));
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be whitespace.", nameof(tagId));
            // Find the association
            var taskTag = await _context.TaskTags
                .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);
            if (taskTag == null)
                throw new InvalidOperationException("The tag is not associated with the task.");
            // Remove and save
            _context.TaskTags.Remove(taskTag);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<Tasks?> UpdateTask(Tasks task)
        {
            // Validate input
            if (task == null)
                throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            if (string.IsNullOrEmpty(task.Id))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(task.Id));
            if (string.IsNullOrWhiteSpace(task.Id))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(task.Id));
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be null or whitespace.", nameof(task.Title));
            if (string.IsNullOrWhiteSpace(task.UserId))
                throw new ArgumentException("User ID cannot be null or whitespace.", nameof(task.UserId));
            // Retrieve existing task
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
                throw new InvalidOperationException($"Task with ID {task.Id} does not exist.");
            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(task.Title))
                existingTask.Title = task.Title.Trim();
            if (task.Description != null)
                existingTask.Description = task.Description.Trim();
            if (task.DueDate.HasValue)
                existingTask.DueDate = task.DueDate;
            if (!string.IsNullOrWhiteSpace(task.Status))
                existingTask.Status = task.Status.Trim();
            if (!string.IsNullOrWhiteSpace(task.UserId))
                existingTask.UserId = task.UserId.Trim();
            if (existingTask != null)
            {
                // Update navigation properties if provided
                existingTask.Subtasks = task.Subtasks ?? existingTask.Subtasks;
                existingTask.TaskCategories = task.TaskCategories ?? existingTask.TaskCategories;
                existingTask.TaskTags = task.TaskTags ?? existingTask.TaskTags;
            }
            if (existingTask != null)
            {
                // Ensure navigation properties are initialized
                existingTask.Subtasks ??= new List<SubTask>();
                existingTask.TaskCategories ??= new List<TaskCategory>();
                existingTask.TaskTags ??= new List<TaskTag>();
            }
            // Save changes
            await _context.SaveChangesAsync();
            return existingTask;

        }
    }
}
