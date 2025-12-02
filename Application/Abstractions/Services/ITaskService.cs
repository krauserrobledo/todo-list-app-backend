using Application.DTOs.TaskDTOs;
using Tasks = Domain.Models.Task;

namespace Application.Abstractions.Services
{
    /// <summary>
    /// Interface for Task service
    /// </summary>
    public interface ITaskService
    {
        // Business logic for task management
        Task<Tasks> CreateTask(TaskCreateRequest request, string userId);
        Task<Tasks?> UpdateTask(TaskUpdateRequest request, string id, string userId);
        Task<bool> DeleteTask(string taskId);
        Task<ICollection<Tasks>> GetUserTasks(string userId);
        Task<Tasks?> GetTaskById(string taskId, string userId);

        // Manage relationships (return updated Task for consistency)
        Task<Tasks?> AddTagToTask(string taskId, string tagId);
        Task<Tasks?> RemoveTagFromTask(string taskId, string tagId);
        Task<Tasks?> AddCategoryToTask(string taskId, string categoryId);
        Task<Tasks?> RemoveCategoryFromTask(string taskId, string categoryId);

        // Validation
        Task<bool> TaskTitleExists(string title, string userId);
    }
}
