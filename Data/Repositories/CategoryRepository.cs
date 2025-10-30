using Domain.Abstractions.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    /// <summary>
    /// Repository implementation for category repository interface
    /// </summary>
    /// <param name="context"></param>
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        // DbContext instance
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Creates a new category for the specified user.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> object to create.</param>
        /// <returns>The created <see cref="Category"/> object, including its generated or updated properties such as <see
        /// cref="Category.Id"/> and <see cref="Category.Color"/>.</returns>
       
        public async Task<Category> Create(Category category)
        {

            // Data access with no business logic
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Updates existing category for a specific user.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> object with updated values.</param>
        /// <returns>The updated <see cref="Category"/> object, or null if not found.</returns>
        public async Task<Category?> Update(Category category)
        {

            // Retrieve existing category
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            // Update in DbContext and save changes
            await _context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Deletes a category by its ID if it exists.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Returns true if the category was found and deleted; otherwise, returns false</returns>
        public async Task<bool> Delete(string categoryId)
        {

            // Retrieve existing category
            var categoryExist = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            // Delete and save if exists
            if (categoryExist == null)
            
                return false;
            
            _context.Categories.Remove(categoryExist);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks if a category exists by its ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category to check.</param>
        /// <returns>Returns true if the category exists; otherwise, false.</returns>
        public async Task<bool> Exists(string categoryId)
        {

            // Validate input and check existance using LINQ
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        /// <summary>
        /// Checks if a category name already exists for a specific user.
        /// </summary>
        /// <param name="name">The name of the category to check.</param>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>Returns true if a category with the specified name exists for the user; otherwise, false.</returns>
        public async Task<bool> NameExists(string name, string userId)
        {

            // Validate input and check existance using LINQ
            return await _context.Categories
                .AnyAsync(c => c.Name == name && c.UserId == userId);
        }

        /// <summary>
        /// Get all categories by user id ordered by descending id.
        /// </summary>
        /// <remarks> Gets UserId from context</remarks>
        /// <param name="userId"></param>
        /// <Returns>Collection of Categories with details</returns>
        public async Task<ICollection<Category>> GetByUser(string userId)
        {

            // Validate input and retrieve using LINQ
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Get categories from tasks and users
        /// </summary>
        /// <remarks>Joins TaskCategories and Categories tables to get categories for a specific task and user</remarks>
        /// <param name="taskId">The ID of the task to check.</param>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>Returns categories async List</returns>
        public async Task<ICollection<Category>> GetByTaskId(string taskId, string userId)
        {

            // Validate input and retrieve using LINQ
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .Join(_context.TaskCategories,
                      c => c.Id,
                      tc => tc.CategoryId,
                      (c, tc) => new { Category = c, TaskCategory = tc })
                .Where(joined => joined.TaskCategory.TaskId == taskId)
                .Select(joined => joined.Category)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific category by id
        /// </summary>
        /// <remarks>Returns null if not found</remarks>
        /// <param name="categoryId">The ID of the category to retrieve.</param>
        /// <returns>Returns Categories by FirstDefaultAsync method</returns>
        public async Task<Category?> GetById(string categoryId)
        {

            // Validate input and retrieve using LINQ
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }
}
