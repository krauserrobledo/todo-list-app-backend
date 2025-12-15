using Domain.Models;

namespace Application.Abstractions.Repositories
{

    /// <summary>
    /// Interface for Category repository
    /// </summary>
    public interface ICategoryRepository
    {

        // CRUD operations
        Task<Category> Create(Category category);

        Task<Category?> Update(Category category);

        Task<bool> Delete(string name);


        // Get all Categories with related entities
        Task<ICollection<Category>> GetByUser(string userId);

        Task<Category?> GetById(string categoryId);

        Task<ICollection<Category>> GetByTaskId(string taskId, string userId);

        Task<bool> Exists(string categoryId);

        Task<bool> NameExists(string name, string userId);


    }
}
