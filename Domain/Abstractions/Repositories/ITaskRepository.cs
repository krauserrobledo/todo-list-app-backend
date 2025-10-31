using Tasks = Domain.Models.Task;

namespace Domain.Abstractions.Repositories
{
    /// <summary>
    /// Interface for Task repository
    /// </summary>
    public interface ITaskRepository
    {

        // Get tasks
        Task<ICollection<Tasks>> GetByUser(string userId);

        Task<ICollection<Tasks>> GetByUserWithDetails(string userId); // with categories and tags

        Task<Tasks?> GetWithDetails(string taskId);


        // Manage relationships
        Task AddTag(string taskId, string tagId);

        Task RemoveTag(string taskId, string tagId);

        Task AddCategory(string taskId, string categoryId);

        Task RemoveCategory(string taskId, string categoryId);


        // CRUD operations
        Task<Tasks?> GetById(string taskId);

        Task<Tasks> Create(Tasks task);

        Task<Tasks?> Update(Tasks task);

        Task<bool> Delete(string taskId);


        //check if task title exists for a user
        Task<bool> TitleExists(string title, string userId);
    }
}