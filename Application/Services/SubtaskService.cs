using Application.Abstractions.Services;
using Application.Abstractions.Repositories;

using Domain.Models;

namespace Application.Services
{
    /// <summary>
    /// Service implementation for  subtask service interface
    /// </summary>
    public class SubtaskService(ISubtaskRepository subtaskRepository, ITaskRepository taskRepository) : ISubtaskService
    {
        private readonly ISubtaskRepository _subtaskRepository = subtaskRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;

        /// <summary>
        /// Creates a new Subtask
        /// </summary>
        /// <param name="title">title for a task</param>
        /// <param name="taskId">Id for a task</param>
        /// <param name="userId">Owner Id</param>
        /// <returns>Returns a new subtask</returns>
        /// <exception cref="ArgumentException">If title is null or empty</exception>
        /// <exception cref="InvalidOperationException">If task not found or access denied</exception>
        public async Task<Subtask> CreateSubtask(string title, string taskId, string userId)
        {

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Subtask title is required");

            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID is required");

            // Check if task belongs to user
            var task = await _taskRepository.GetById(taskId);
            if (task == null || task.UserId != userId)
                throw new InvalidOperationException("Task not found or access denied");

            // Check if subtask title already exists for this task
            var titleExists = await _subtaskRepository.TitleExists(title.Trim(), taskId);
            if (titleExists)
                throw new InvalidOperationException($"A subtask with the title '{title}' already exists for this task");

            var subtask = new Subtask
            {
                Id = Guid.NewGuid().ToString(),
                Title = title.Trim(),
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow
            };

            return await _subtaskRepository.Create(subtask);
        }
        /// <summary>
        /// Updates an existing Subtask
        /// </summary>
        /// <param name="subtaskId">Identification for a subtask</param>
        /// <param name="title">Title for a subtask</param>
        /// <param name="userId">Owner Id</param>
        /// <returns>Updated subtask</returns>
        public async Task<Subtask?> UpdateSubtask(string subtaskId, string? title, string userId)
        {
            var subtask = await _subtaskRepository.GetById(subtaskId);
            if (subtask == null)
                return null;

            // Check that belongs to user
            var task = await _taskRepository.GetById(subtask.TaskId);
            if (task == null || task.UserId != userId)
                return null;

            if (!string.IsNullOrWhiteSpace(title))
                subtask.Title = title.Trim();

            return await _subtaskRepository.Update(subtask);
        }

        /// <summary>
        /// Service method to delete a subtask
        /// </summary>
        /// <param name="subtaskId">Subtask Id</param>
        /// <param name="userId">Owner Id</param>
        /// <returns>returns true if deleted successfully</returns>
        public async Task<bool> DeleteSubtask(string subtaskId, string userId)
        {
            var subtask = await _subtaskRepository.GetById(subtaskId);
            if (subtask == null)
                return false;

            // Check owner
            var task = await _taskRepository.GetById(subtask.TaskId);
            if (task == null || task.UserId != userId)
                return false;

            return await _subtaskRepository.Delete(subtaskId);
        }

        /// <summary>
        /// Service method to get a subtask by id
        /// </summary>
        /// <param name="subtaskId">Id for a subtask</param>
        /// <param name="userId">Owner</param>
        /// <returns>A subtask finded by Id</returns>
        public async Task<Subtask?> GetSubtaskById(string subtaskId, string userId)
        {
            var subtask = await _subtaskRepository.GetById(subtaskId);
            if (subtask == null)
                return null;

            // Validate owner
            var task = await _taskRepository.GetById(subtask.TaskId);
            return task?.UserId == userId ? subtask : null;
        }

        /// <summary>
        /// Service method to get all subtasks by task id
        /// </summary>
        /// <param name="taskId">Id for a task</param>
        /// <param name="userId">Owner Id</param>
        /// <returns>All subtasks for a task</returns>
        public async Task<ICollection<Subtask>> GetSubtasksByTask(string taskId, string userId)
        {
            // Check if belongs to user
            var task = await _taskRepository.GetById(taskId);
            if (task == null || task.UserId != userId)
                return [];

            return await _subtaskRepository.GetAllByTask(taskId);
        }
    }
}