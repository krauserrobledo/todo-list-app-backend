using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Task = System.Threading.Tasks.Task;
using Tasks = Domain.Models.Task;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository;

        public TaskService(ITaskRepository taskRepository,
                           ICategoryRepository categoryRepository,
                           ITagRepository tagRepository)
        {
            _taskRepository = taskRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
        }

        /// <summary>
        /// Service for task create Endppoints implementing repositories
        /// </summary>
        /// <param name="title">title for a task</param>
        /// <param name="description">Description for a task</param>
        /// <param name="dueDate">Creation date auto by date now</param>
        /// <param name="userId">task creator´s id</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks> CreateTask(string title, string? description, DateTime? dueDate, string userId, string status)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            if (string.IsNullOrEmpty(status) || !Domain.Constants.TaskStatus.IsValid(status))
                status = Domain.Constants.TaskStatus.NonStarted;

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required");

            var titleExists = await _taskRepository.TitleExists(title.Trim(), userId);
            if (titleExists)
                throw new ArgumentException("A task with the same title already exists");

            var task = new Tasks
            {
                Id = Guid.NewGuid().ToString(),
                Title = title.Trim(),
                Description = description?.Trim(),
                DueDate = dueDate,
                UserId = userId,
                Status = status
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
        public async Task<Tasks?> UpdateTask(string taskId, string? title, string? description, DateTime? dueDate, string? status)
        {
            var existingTask = await _taskRepository.GetById(taskId)
                               ?? throw new ArgumentException("Task not found");

            if (!string.IsNullOrWhiteSpace(title))
                existingTask.Title = title.Trim();

            if (!string.IsNullOrWhiteSpace(description))
                existingTask.Description = description.Trim();

            if (dueDate.HasValue)
                existingTask.DueDate = dueDate;

            if (!string.IsNullOrWhiteSpace(status))
                existingTask.Status = status;

            return await _taskRepository.Update(existingTask);
        }


        /// <summary>
        /// Deletes a task by its ID. 
        /// </summary>
        /// <param name="taskId">The ID of the task to delete.</param>
        /// <returns>True if the task was deleted; otherwise, false.</returns>
        public async Task<bool> DeleteTask(string taskId)
        {
            var existingTask = await _taskRepository.GetById(taskId);
            if (existingTask == null) return false;

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
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            var task = await _taskRepository.GetWithDetails(taskId);
            return task?.UserId == userId ? task : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ICollection<Tasks>> GetUserTasks(string userId)
        {
            var tasks = await _taskRepository.GetByUserWithDetails(userId);
            return tasks ?? Array.Empty<Tasks>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks?> AddCategoryToTask(string taskId, string categoryId)
        {
            var task = await _taskRepository.GetById(taskId) ?? throw new ArgumentException("Task not found");
            var category = await _categoryRepository.GetById(categoryId) ?? throw new ArgumentException("Category not found");

            await _taskRepository.AddCategory(taskId, categoryId);
            return await _taskRepository.GetWithDetails(taskId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks?> AddTagToTask(string taskId, string tagId)
        {
            var task = await _taskRepository.GetById(taskId) ?? throw new ArgumentException("Task not found");
            var tag = await _tagRepository.GetById(tagId) ?? throw new ArgumentException("Tag not found");

            await _taskRepository.AddTag(taskId, tagId);
            return await _taskRepository.GetWithDetails(taskId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks?> RemoveCategoryFromTask(string taskId, string categoryId)
        {
            var task = await _taskRepository.GetById(taskId) ?? throw new ArgumentException("Task not found");
            var category = await _categoryRepository.GetById(categoryId) ?? throw new ArgumentException("Category not found");

            await _taskRepository.RemoveCategory(taskId, categoryId);
            return await _taskRepository.GetWithDetails(taskId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tasks?> RemoveTagFromTask(string taskId, string tagId)
        {
            var task = await _taskRepository.GetById(taskId) ?? throw new ArgumentException("Task not found");
            var tag = await _tagRepository.GetById(tagId) ?? throw new ArgumentException("Tag not found");

            await _taskRepository.RemoveTag(taskId, tagId);
            return await _taskRepository.GetWithDetails(taskId);
        }

        /// <summary>
        /// Check if title exsists in another task
        /// </summary>
        /// <param name="title">Title from task to validate</param>
        /// <param name="userId">Task creators´s id</param>
        /// <returns>Valid title if false, if true, title already exists</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> TaskTitleExists(string title, string userId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            return await _taskRepository.TitleExists(title, userId);
        }
    }
}
