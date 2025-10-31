using Application.Abstractions.Services;
using Application.Tools;
using Domain.Abstractions.Repositories;
using Domain.Models;
using System.Threading.Tasks;


namespace Application.Services
{
    /// <summary>
    /// Service implementation for Category service interface 
    /// </summary>
    public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        /// <summary>
        /// Creates a new category for a user. 
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <param name="color">The color of the category.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns> The created category if successful, null otherwise.</returns>
        /// <exception cref="ArgumentException"> When the input is invalid.</exception>
        /// <exception cref="InvalidOperationException"> When a category with the same name already exists.</exception>
        public async Task<Category> CreateCategory(string name, string? color, string userId)
        {

            // Bussiness logic validations
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            // Check for duplicate names for the same user
            var nameExists = await _categoryRepository.NameExists(name.Trim(), userId);
            if (nameExists)
                throw new InvalidOperationException("A category with the same name already exists for this user");

            // Validate and format color
            var validatedColor = Validations.ValidateAndFormatColor(color);

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Color = validatedColor,
                UserId = userId
            };

            // Delegate creation to repository
            return await _categoryRepository.Create(category);
        }

        /// <summary>
        /// Updates an existing category for a user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="userId">The ID of the user who owns the category.</param>
        /// <param name="name">The new name for the category.</param>
        /// <param name="color">The new color for the category.</param>
        /// <returns>The updated category, or null if not found.</returns>
        /// <exception cref="InvalidOperationException">When a category with the same name already exists.</exception>
        public async Task<Category?> UpdateCategory(string categoryId, string userId, string? name, string? color)
        {

            // Check ownership of category 
            var existingCategory = await _categoryRepository.GetById(categoryId);
            if (existingCategory == null || existingCategory.UserId != userId)
                return null;

            // Validate and update name if provided
            if (!string.IsNullOrWhiteSpace(name))
            {

                if (name.Trim() != existingCategory.Name)
                {

                    var nameExists = await _categoryRepository.NameExists(name.Trim(), userId);

                    if (nameExists)
                        throw new InvalidOperationException("A category with the same name already exists for this user");

                    existingCategory.Name = name.Trim();
                }
            }

            // Validate and update color if provided
            if (!string.IsNullOrWhiteSpace(color))
            {
                existingCategory.Color = Validations.ValidateAndFormatColor(color);
            }

            return await _categoryRepository.Update(existingCategory);
        }

        /// <summary>
        /// Deletes a category for a user. 
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <param name="userId">The ID of the user who owns the category.</param>
        /// <returns>True if the category was deleted; otherwise, false.</returns>
        public async Task<bool> DeleteCategory(string categoryId, string userId)
        {
            // Verify ownership before deletion
            var category = await _categoryRepository.GetById(categoryId);

            if (category == null || category.UserId != userId)
                return false;

            return await _categoryRepository.Delete(categoryId);
        }

        /// <summary>
        /// Gets all categories for a specific user. 
        /// </summary>
        /// <param name="userId">The ID of the user whose categories to retrieve.</param>
        /// <returns>A collection of categories belonging to the user.</returns>
        public async Task<ICollection<Category>> GetUserCategories(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            return await _categoryRepository.GetByUser(userId);
        }

        /// <summary>
        /// Gets a category by its ID for a specific user. 
        /// </summary>
        /// <param name="categoryId">The ID of the category to retrieve.</param>
        /// <param name="userId">The ID of the user who owns the category.</param>
        /// <returns>The category if found and owned by the user; otherwise, null.</returns>
        public async Task<Category?> GetCategoryById(string categoryId, string userId)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Task ID is required");

            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            var category = await _categoryRepository.GetById(categoryId);


            // Return category
            return category?.UserId == userId ? category : null;
        }

        /// <summary>
        /// Gets categories associated with a specific task for a user. 
        /// </summary>
        /// <param name="taskId">The ID of the task whose categories to retrieve.</param>
        /// <param name="userId">The ID of the user who owns the task.</param>
        /// <returns>A collection of categories associated with the task.</returns>
        public async Task<ICollection<Category>> GetCategoriesByTask(string taskId, string userId)
        {

            // Validate inputs
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID is required");

            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            return await _categoryRepository.GetByTaskId(taskId, userId);
        }
    }
}
