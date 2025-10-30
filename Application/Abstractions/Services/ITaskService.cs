using Tasks = Domain.Models.Task;
namespace Application.Abstractions.Services
{
    /// <summary>
    /// Interface for Task service
    /// </summary>
    public interface ITaskService
    {
        // business logic for task management
        Task<Tasks> CreateTask(string title, string? description, DateTime? dueDate, string userId);
        Task<Tasks?> UpdateTask(string taskId, string? title, string? description, DateTime? dueDate, bool? isCompleted);
        Task<bool> DeleteTask(string taskId);
        Task<ICollection<Tasks>> GetUserTasks(string userId);
        Task<Tasks?> GetTaskById(string taskId);

        // manage relationships
        Task<bool> AddTagToTask(string taskId, string tagId);
        Task<bool> RemoveTagFromTask(string taskId, string tagId);
        Task<bool> AddCategoryToTask(string taskId, string categoryId);
        Task<bool> RemoveCategoryFromTask(string taskId, string categoryId);
    }
}
