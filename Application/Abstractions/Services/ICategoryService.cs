using Domain.Models;

namespace Application.Abstractions.Services
{

    /// <summary>
    /// Interface for Category service 
    /// </summary>
    public interface ICategoryService
    {

        // business logic for category management
        Task<Category> CreateCategory(string name, string? color, string userId);

        Task<Category?> UpdateCategory(string categoryId, string userId, string? name, string? color);

        Task<bool> DeleteCategory(string categoryId, string userId);

        Task<ICollection<Category>> GetUserCategories(string userId);

        Task<Category?> GetCategoryById(string categoryId, string userId);

        Task<ICollection<Category>> GetCategoriesByTask(string taskId, string userId);
    }
}
