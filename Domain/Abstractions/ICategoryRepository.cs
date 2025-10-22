using Domain.Models;

namespace Domain.Abstractions
{
    public interface ICategoryRepository
    {
        // CRUD operations
        Task<Category> CreateCategory(Category category);
        Task<Category?> UpdateCategory(Category category);
        Task<bool> DeleteCategory(string name);

        // Get all Categories with related entities
        Task<ICollection<Category>> GetCategoriesByUser(string userId);
        Task<Category?> GetCategoryById(string categoryId);
        Task<ICollection<Category>> GetCategoriesByTaskId(string taskId, string userId);
        Task<bool> CategoryExists(string categoryId);
        Task<bool> CategoryNameExists(string name, string userId);


    }
}
