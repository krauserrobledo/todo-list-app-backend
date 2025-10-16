using Application.Abstractions;
using Domain.Models;
using Task = System.Threading.Tasks.Task;
using Microsoft.EntityFrameworkCore;
using Data.Tools;


namespace Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context) { 
            _context = context;
        }

        public async Task<Category> CreateCategory(Category category)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be null or WhiteSpace.", nameof(category.Name));

            if (category.Color != null && category.Color.Length > 7)
                // Default to white if invalid
                category.Color = "#FFFFFF";

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(category.Id) || string
                .IsNullOrWhiteSpace(category.Id))
                category.Id = Guid.NewGuid().ToString();

            // Clean up and save
            category.Name = category.Name.Trim();
            category.UserId = category.UserId.Trim();
            //category.CreatedBy = null!; // Avoid EF Core tracking issues
            category.TaskCategories = new List<TaskCategory>();

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return category;
        }
        public async Task<bool> CategoryExists(string categoryId)
        {
            // Validate input
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(categoryId));
               
            // Check existence
            if (await _context.Categories.AnyAsync(c => c.Id == categoryId))
                return true;

            return false;
        }

        public async Task<bool> CategoryNameExists(string name, string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be whitespace.", nameof(name));
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));

            // Check existence
            if (await _context.Categories.AnyAsync(c => c.Name == name && c.UserId == userId))
                return true;

            return false;
        }

        public async Task<ICollection<Category>> GetCategoriesByUser(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));

            // Retrieve categories for the user
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByTaskId(string taskId, string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));

            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));

            // Retrieve the Category associated with the Task for the user
            var category = await _context.TaskCategories
                .Include(tc => tc.Category)
                .Where(tc => tc.TaskId == taskId && tc.Category.UserId == userId)
                .Select(tc => tc.Category)
                .FirstOrDefaultAsync();

            return category;
        }

        public async Task<Category?> GetCategoryById(string categoryId)
        {
            // Validate input
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(categoryId));

            // Retrieve the Category by ID
            var category = await _context.Categories.FindAsync(categoryId);
            return category;    
        }

        public async Task<Category?> UpdateCategory(Category category)
        {
            // Validate input
            if (string.IsNullOrEmpty(category.Id))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(category.Id));
            if (string.IsNullOrWhiteSpace(category.Id))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(category.Id));
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be null or WhiteSpace.", nameof(category.Name));
            if (category.Color != null && category.Color.Length > 7)
                // Default to white if invalid
                category.Color = "#FFFFFF";

            // Find existing Category
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            // Not found
            if (existingCategory == null)
                return null;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(category.Name))
            { 
                var cleanName = category.Name.Trim();
                if (cleanName.Length > 0)
                    existingCategory.Name = cleanName;

                var exists = await _context.Categories
                    .AnyAsync(c => c.Name == cleanName && 
                    c.UserId == existingCategory.UserId && 
                    c.Id != existingCategory.Id);

                if (exists)
                    throw new InvalidOperationException("A category named '{cleanName}' already exists for this user.");

                existingCategory.Name = cleanName;
            }
            // Update color if provided
            if (!string.IsNullOrWhiteSpace(category.Color))
            {   
                var cleanColor = category.Color.Trim();

                if (!Validations.IsValidHexColor(cleanColor))
                    existingCategory.Color = cleanColor;
                else
                    existingCategory.Color = "#FFFFFF"; // Default to white if invalid

            }
            // Note: Not updating UserId or CreatedBy to maintain integrity
            // Save changes
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
            return existingCategory;

        }

        public async Task<bool> DeleteCategory(string categoryId)
        {
            // Validate input
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("Category ID cannot be null or empty.", nameof(categoryId));

            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be whitespace.", nameof(categoryId));

            // Find the Category by ID
            var category = await _context.Categories.FindAsync(categoryId);
            // If not found, return false
            if (category == null)
                return false;
            // Remove from DbContext and save changes
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
