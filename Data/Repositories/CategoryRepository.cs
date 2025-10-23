using Domain.Abstractions;
using Data.Tools;
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
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Creates a new category for the specified user, ensuring that the category name is unique for that user.
        /// </summary>
        /// <remarks>If the <see cref="Category.Id"/> is not provided, a new GUID will be generated. If
        /// the <see cref="Category.Color"/> is not provided or is invalid, it defaults to white (#FFFFFF).</remarks>
        /// <param name="category">The <see cref="Category"/> object to create. The <see cref="Category.Name"/> and <see
        /// cref="Category.UserId"/> properties must be set.</param>
        /// <returns>The created <see cref="Category"/> object, including its generated or updated properties such as <see
        /// cref="Category.Id"/> and <see cref="Category.Color"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a category with the same name already exists for the specified user.</exception>
        public async Task<Category> CreateCategory(Category category)
        {

            // Validate NAME input using LINQ
            var existingCategory = await _context.Categories
                 .FirstOrDefaultAsync(c => c.Name == category.Name && c.UserId == category.UserId);

            if (existingCategory != null)
            {
                throw new InvalidOperationException("A category with the same name already exists for this user.");
            }

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(category.Id) || string
                .IsNullOrWhiteSpace(category.Id))
                category.Id = Guid
                    .NewGuid()
                    .ToString();

            // Validate color default white
            if (string.IsNullOrEmpty(category.Color))
                category.Color = "#FFFFFF";
            else if (!Validations.IsValidHexColor(category.Color))
                category.Color = "#FFFFFF";

            // Add to DbContext and save changes
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Updates existing category for a specific user, valdating inputs and ensuring color integrity.
        /// </summary>
        /// <remarks>If the <see cref="Category.Color"/> is not provided or is invalid, it defaults to white (#FFFFFF).
        /// If the category does not exist, null is returned.</remarks> 
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category?> UpdateCategory(Category category)
        {

            // Validation: Check if category exists using LINQ
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == category.Id);
            if (existingCategory == null)
                return null;

            // Validate color default White
            if (string.IsNullOrWhiteSpace(category.Color))
                category.Color = "#FFFFFF";

            else if (!Validations.IsValidHexColor(category.Color))
                category.Color = "#FFFFFF";

            // Update properties if changed
            if (!string.IsNullOrWhiteSpace(category.Name))
                existingCategory.Name = category.Name;

            if (existingCategory.Color != category.Color)
                existingCategory.Color = category.Color;

            // Update in DbContext and save changes
            await _context.SaveChangesAsync();
            return existingCategory;
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Deletes a category by its ID if it exists.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Returns true if the category was found and deleted; otherwise, returns false</returns>
        public async Task<bool> DeleteCategory(string categoryId)
        {

            // Validate input using LINQ
            var categoryExist = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            // Delete and save if exists
            if (categoryExist != null)
            {
                _context.Categories.Remove(categoryExist);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Checks if a category exists by its ID.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Returns true if the category exists; otherwise, false.</returns>
        public async Task<bool> CategoryExists(string categoryId)
        {

            // Validate input and check existance using LINQ
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId);
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Checks if a category name already exists for a specific user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns>Returns true if a category with the specified name exists for the user; otherwise, false.</returns>
        public async Task<bool> CategoryNameExists(string name, string userId)
        {

            // Validate input and check existance using LINQ
            return await _context.Categories
                .AnyAsync(c => c.Name == name && c.UserId == userId);
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Get all categories by user id ordered by descending id.
        /// </summary>
        /// <remarks> Gets UserId from context</remarks>
        /// <param name="userId"></param>
        /// <Returns>Collection of Categories with details</returns>
        public async Task<ICollection<Category>> GetCategoriesByUser(string userId)
        {

            // Validate input and retrieve using LINQ
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Get categories from tasks and users
        /// </summary>
        /// <remarks>Joins TaskCategories and Categories tables to get categories for a specific task and user</remarks>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns>Returns categories async List</returns>
        public async Task<ICollection<Category>> GetCategoriesByTaskId(string taskId, string userId)
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
<<<<<<< HEAD

=======
>>>>>>> 9b74040 (refactor: clean code and documentation)
        /// <summary>
        /// Get a specific category by id
        /// </summary>
        /// <remarks>Returns null if not found</remarks>
        /// <param name="categoryId"></param>
        /// <returns>Returns Categories by FirstDefaultAsync method</returns>
        public async Task<Category?> GetCategoryById(string categoryId)
        {

            // Validate input and retrieve using LINQ
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }
}
