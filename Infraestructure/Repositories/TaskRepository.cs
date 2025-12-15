using Application.Abstractions.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using Tasks = Domain.Models.Task;

namespace Infraestructure.Repositories
{

    /// <summary>
    /// Concrete implementation of ITaskRepository using Entity Framework Core.
    /// </summary>
    /// <param name="context">HTTP context</param>
    public class TaskRepository(AppDbContext context) : ITaskRepository
    {

        // Dependency Injection of AppDbContext
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <remarks> Validates before creating a Task</remarks>
        /// <param name="task"> Task model</param>
        /// <returns>Task</returns>
        /// <exception cref="InvalidOperationException">Thrown when a task with the same title already exists for the user.</exception>
        public async Task<Tasks> Create(Tasks task)
        {
            // No business logic validation for duplicate titles here 
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <remarks> Validates before updating a Task</remarks>
        /// <param name="task"> Task entity</param>
        /// <returns> Existing Task</returns>
        public async Task<Tasks?> Update(Tasks task)
        {
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (existingTask == null)
                return null;

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.DueDate = task.DueDate;

            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            return await _context.Tasks
                .Where(t => t.Id == task.Id)
                .Include(t => t.Subtasks)
                .Include(t => t.TaskCategories).ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <remarks> Validates before deleting a Task</remarks>
        /// <param name="taskId">Id for a task</param>
        /// <returns>Boolean value </returns>
        public async Task<bool> Delete(string taskId)
        {

            // Get task using LINQ
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            // If not found, return false
            if (task == null) return false;

            // Remove and save
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets a task by its ID.
        /// </summary>
        /// <remarks> Retrieves a Task by ID</remarks>
        /// <param name="taskId">Id for a task</param>
        /// <returns> Existing Task</returns>
        public async Task<Tasks?> GetById(string taskId)
        {

            // Get task using LINQ
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        /// <summary>
        /// Gets tasks by user ID.
        /// </summary>
        /// <param name="userId"> User Identification</param>
        /// <returns>List of Tasks</returns>
        public async Task<ICollection<Tasks>> GetByUser(string userId)
        {

            // Get tasks using LINQ
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets tasks by user ID with related details.
        /// </summary>
        /// <remarks> Retrieves tasks along with their related entities for a specific user</remarks>
        /// <param name="userId"> User identification</param>
        /// <returns> Task list by user id</returns>
        public async Task<ICollection<Tasks>> GetByUserWithDetails(string userId)
        {

            // get tasks with related entities using LINQ
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .Include(t => t.Subtasks)
                .Include(t => t.Subtasks)
                .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();

            return tasks;
        }

        /// <summary>
        /// Gets a task by its ID with related details.
        /// </summary>
        /// <param name="taskId">Id for a task</param>
        /// <returns>Task with related details</returns>
        /// <exception cref="ArgumentException">Thrown when taskId is null or empty</exception>
        public async Task<Tasks?> GetWithDetails(string taskId)
        {

            // Retrieve task with related entities
            var task = await _context.Tasks
                .Where(t => t.Id == taskId)
                .Include(t => t.Subtasks)
                .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync();

            return task;
        }

        /// <summary>
        /// Gets a task by its ID with related details.
        /// </summary>
        /// <remarks> Removes category from task after validating association</remarks>
        /// <param name="taskId">Id for a task</param>
        /// <param name="categoryId">Id for a category</param>
        /// <returns>Task with related details</returns>
        /// <exception cref="InvalidOperationException">Thrown when the category is not associated with the task.</exception>
        public async Task RemoveCategory(string taskId, string categoryId)
        {

            // Validate input AND association using LINQ
            var taskCategory = await _context.TaskCategories
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId) ?? throw new InvalidOperationException("The category is not associated with the task.");

            // Remove and save
            _context.TaskCategories.Remove(taskCategory);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a tag from a task after validating association.
        /// </summary>
        /// <remarks> Removes tag from task after validating association</remarks>
        /// <param name="taskId">Id for a task</param>
        /// <param name="tagId">Id for a tag</param>
        /// <returns>Task with related details</returns>
        /// <exception cref="InvalidOperationException">Thrown when the tag is not associated with the task.</exception>
        public async Task RemoveTag(string taskId, string tagId)
        {

            // Validate association using LINQ
            var taskTag = await _context.TaskTags
                .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId) ?? throw new Exception ("That association does not exists");


            if (taskTag != null)

                // Remove and save
                _context.TaskTags.Remove(taskTag);
                await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets a task by its ID with related details.
        /// </summary>
        /// <remarks> Adds category to task after validating association</remarks>
        /// <param name="taskId">Id for a task</param>
        /// <param name="categoryId">Id for a category</param>
        /// <returns>Task with related details</returns>
        /// <exception cref="InvalidOperationException">Thrown when the category is not associated with the task.</exception>
        public async Task AddCategory(string taskId, string categoryId)
        {
            
            // Create new association
            var taskCategory = new TaskCategory()
            {
                TaskId = taskId,
                CategoryId = categoryId
            };

            // Add and save
            _context.TaskCategories.Add(taskCategory);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets a task by its ID with related details.
        /// </summary>
        /// <param name="taskId">Id for a task</param>
        /// <param name="tagId">Id for a tag</param>
        /// <returns>Task with related details</returns>
        /// <exception cref="InvalidOperationException">Thrown when the tag is not associated with the task.</exception>
        public async Task AddTag(string taskId, string tagId)
        {

            

            // Create new association
            var taskTag = new TaskTag()
            {
                TaskId = taskId,
                TagId = tagId
            };

            // Add and save
            _context.TaskTags.Add(taskTag);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a task title exists for a specific user.
        /// </summary>
        /// <remarks> Validates if task title exists for a user</remarks>
        /// <param name="title">The title of the task.</param>
        /// <param name="userId">Id for user</param>
        /// <returns>Boolean value</returns>
        public async Task<bool> TitleExists(string title, string userId)
        {

            return await _context.Tasks
                .AnyAsync(t => t.Title.ToLower() ==  title.ToLower() && t.UserId == userId);
        }
    }
}
