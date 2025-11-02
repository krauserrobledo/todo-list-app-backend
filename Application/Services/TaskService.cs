using Application.Abstractions.Services;
using Application.Abstractions.Repositories;
using Domain.Models;
using Tasks = Domain.Models.Task;

namespace Application.Services
{
    public class TaskService(ITaskRepository taskRepository, 
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository) : ITaskService
    {
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly ITagRepository _tagRepository = tagRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="dueDate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks> CreateTask(string title, string? description, DateTime? dueDate, string userId)
        {

            // Business logic validations
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            var titleExists = _taskRepository.TitleExists(title.Trim(), userId).Result;

            if (titleExists)
                throw new ArgumentException("A task with the same title already exists");


            var task = new Tasks
            {
                Id = Guid.NewGuid().ToString(),
                Title = title.Trim(),
                Description = description?.Trim(),
                DueDate = dueDate,
                UserId = userId
            };

            return await _taskRepository.Create(task);
        }

        /// <summary>
        /// Updates an existing task. 
        /// </summary>
        /// <param name="taskId">The ID of the task to update.</param>
        /// <param name="title">The new title for the task.</param>
        /// <param name="description">The new description for the task.</param>
        /// <param name="dueDate">The new due date for the task.</param>
        /// <returns>The updated task.</returns>
        /// <exception cref="ArgumentException">Thrown when the task is not found.</exception>
        public async Task<Tasks?> UpdateTask(string taskId, string? title, string? description, DateTime? dueDate)
        {
            // Business logic validations
            var existingTask = _taskRepository.GetById(taskId).Result ?? throw new ArgumentException("Task not found");

            if (!string.IsNullOrWhiteSpace(title))
                existingTask.Title = title.Trim();

            if (!string.IsNullOrWhiteSpace(description))
                existingTask.Description = description.Trim();

            if (dueDate.HasValue)
                existingTask.DueDate = dueDate;

            return await _taskRepository.Update(existingTask);
        }

        /// <summary>
        /// Deletes a task by its ID. 
        /// </summary>
        /// <param name="taskId">The ID of the task to delete.</param>
        /// <returns>True if the task was deleted; otherwise, false.</returns>
        public async Task<bool> DeleteTask(string taskId)
        {
            // Verify existence before deletion
            var existingTask = _taskRepository.GetById(taskId).Result;

            if (existingTask == null)

                return false;

            return await _taskRepository.Delete(taskId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks?> GetTaskById(string taskId, string userId)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID is required");

            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            var task = await _taskRepository.GetById(taskId);


            // Return task
            return task?.UserId == userId ? task : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ICollection<Tasks>> GetUserTasks(string userId)
        {
            var existingTasks = await _taskRepository.GetByUser(userId);

            if (existingTasks == null || existingTasks.Count == 0)
                return Array.Empty<Tasks>();

            return existingTasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> AddCategoryToTask(string taskId, string categoryId)
        {
            // Validate inputs
            var task = _taskRepository.GetById(taskId).Result ?? throw new ArgumentException("Task not found");

            var category = _categoryRepository.GetById(categoryId).Result ?? throw new ArgumentException("Category not found");

            // Add category to task
            return await (Task<bool>)_taskRepository.AddCategory(taskId, categoryId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> AddTagToTask(string taskId, string tagId)
        {

            // Validate inputs
            var task = _taskRepository.GetById(taskId).Result ?? throw new ArgumentException("Task not found");

            var tag = _tagRepository.GetById(tagId).Result ?? throw new ArgumentException("Tag not found");

            // Add tag to task
            return await (Task<bool>)_taskRepository.AddTag(taskId, tagId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> RemoveCategoryFromTask(string taskId, string categoryId)
        {

            // Validate inputs
            var task = _taskRepository.GetById(taskId).Result ?? throw new ArgumentException("Task not found");

            var category = _categoryRepository.GetById(categoryId).Result ?? throw new ArgumentException("Category not found");

            // Remove category from task
            return await (Task<bool>)_taskRepository.RemoveCategory(taskId, categoryId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> RemoveTagFromTask(string taskId, string tagId)
        {

            // Validate inputs
            var task = _taskRepository.GetById(taskId).Result ?? throw new ArgumentException("Task not found");

            var tag = _tagRepository.GetById(tagId).Result ?? throw new ArgumentException("Tag not found");

            // Remove tag from task
            return await (Task<bool>)_taskRepository.RemoveTag(taskId, tagId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> TaskTitleExistsAsync(string title, string userId)
        {

            // Validate inputs
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            return await _taskRepository.TitleExists(title.Trim(), userId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> TaskTitleExists(string title, string userId)
        {

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            return await TaskTitleExistsAsync(title, userId);
        }
    }
}
