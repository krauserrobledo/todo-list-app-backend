using Tasks = Domain.Models.Task;

namespace Domain.Abstractions
{
    /// <summary>
    /// Interface for Task repository
    /// </summary>
    public interface ITaskRepository
    {

        // Get tasks
        Task<ICollection<Tasks>> GetTasksByUser(string userId);

        Task<ICollection<Tasks>> GetTasksByUserWithDetails(string userId); // with categories and tags

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


        //check if task title exists for a user
        Task<bool> TaskTitleExists(string title, string userId);
    }
}