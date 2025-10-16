using Tasks = Domain.Models.Task;

namespace Application.Abstractions
{
    public interface ITaskRepository
    {
        // Get all tasks for a specific user
        Task<ICollection<Tasks>> GetTasksByUser(string userId);
        Task<ICollection<Tasks>> GetTasksByUserWithDetails(string userId); // with categories and tags

        // Get tasks with related entities
        Task<ICollection<Tasks>> GetTasksWithCategoriesAndTags(string userId);
        Task<Tasks?> GetTaskWithDetails(string taskId);

        // Manage relationships
        Task AddTagToTask(string taskId, string tagId);
        Task RemoveTagFromTask(string taskId, string tagId);
        Task AddCategoryToTask(string taskId, string categoryId);
        Task RemoveCategoryFromTask(string taskId, string categoryId);

        // CRUD operations
        Task<Tasks?> GetTaskById(string taskId);
        Task<Tasks> CreateTask(Tasks task);
        Task<Tasks?> UpdateTask(Tasks task);
        Task<bool> DeleteTask(string taskId);
    }
}