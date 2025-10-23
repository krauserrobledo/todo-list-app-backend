using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;

namespace MinimalApi.Endpoints
{
    /// <summary>
    /// Endpoints for managing categories.
    /// </summary>
    public static class CategoryEndpoints
    {
        /// <summary>
        /// Configures the API endpoints for category-related operations.
        /// </summary>
        /// <param name="app"> </param>
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories");
            // Defined category endpoints
            group.MapPost("/", CreateCategory)
                .WithSummary("Create a new Category")
                .RequireAuthorization();
            group.MapPut("/{id}", UpdateCategory)
                .WithSummary("Update an existing Category")
                .RequireAuthorization();
            group.MapDelete("/{id}", DeleteCategory)
                .WithSummary("Delete a Category by ID")
                .RequireAuthorization();
            group.MapGet("/{id}", GetCategoryById)
                .WithSummary("Get a Category by ID")
                .RequireAuthorization();                
            group.MapGet("/user/", GetCategoriesByUser)
                .WithSummary("Get Categories by User ID")
                .RequireAuthorization();
        }
        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <remarks>Validates the request and ensures the category name is unique for the user before creation.</remarks>
        /// <param name="request">The category creation request.</param>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A result indicating the outcome of the category creation.</returns>
        private static async Task<IResult> CreateCategory(
            [FromBody] CategoryCreateDTO request,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to create a category
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Results.BadRequest("Category name is required.");
                // Get user ID from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check for duplicate category name for the same user
                var nameExists = await categoryRepository.CategoryNameExists(request.Name, userId);
                if (nameExists)
                {
                    return Results.Conflict("A category with the same name already exists for this user.");
                }
                // Create new Category entity
                var newCategory = new Domain.Models.Category
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = request.Name,
                    Color = request.Color
                };
                // Save to repository
                var createdCategory = await categoryRepository.CreateCategory(newCategory);
                // Return success response
                return Results.Created($"/api/categories/{createdCategory.Id}", new
                {
                    id = createdCategory.Id,
                    name = createdCategory.Name,
                    color = createdCategory.Color
                });
            }
            catch (Exception ex)
            {
                // Log ex
                return Results.Problem($"Error while creating Category: {ex.Message}");
            }
        }
        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <remarks>Validates the request, checks for category existence, and ensures the category name is unique for the user before updating.</remarks>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="request">The category update request.</param>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A result indicating the outcome of the category update.</returns>
        private static async Task<IResult> UpdateCategory(
            string id,
            [FromBody] CategoryUpdateDTO request,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to update a category
            try
            {
                // Check if exists 
                var existingCategory = await categoryRepository.GetCategoryById(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category not found");
                }
                // Limit update by user
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != existingCategory.UserId)
                    return Results.Unauthorized();
                // Check for duplicate name
                var nameExists = await categoryRepository.CategoryNameExists(request.Name, existingCategory.UserId);
                if (nameExists)
                {
                    return Results.Conflict("A category with the same name already exists for this user.");
                }
                existingCategory.Name = request.Name ?? existingCategory.Name;
                // Color Validation
                if (!Data.Tools.Validations.IsValidHexColor(request.Color))
                {
                    // If invalid, set to default white
                    existingCategory.Color = "#FFFFFF";
                }
                else
                {
                    existingCategory.Color = request.Color;
                }
                // Update in repository
                var updatedCategory = await categoryRepository.UpdateCategory(existingCategory);
                if (updatedCategory == null)
                {
                    return Results.Problem("Error while updating Category");
                }
                // Return response
                return Results.Ok(new
                {
                    id = existingCategory.Id,
                    name = updatedCategory.Name?? existingCategory.Name,
                    color = updatedCategory.Color?? existingCategory.Color
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while updating Category{ex.Message}");
            }
        }
        /// <summary>
        /// Deletes a category by ID.
        /// </summary>
        /// <remarks>Verifies the category existence before deletion.</remarks>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Results indicating the outcome of the deletion.</returns>
        private static async Task<IResult> DeleteCategory(
            string id,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to delete a category
            try
            {
                // Verify ID
                var existingCategory = await categoryRepository.GetCategoryById(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category with ID {id} not found.");
                }
                // Delete from repository
                var deleted = await categoryRepository.DeleteCategory(id);
                if (!deleted)
                {
                    return Results.Problem("Error while deleting Category");
                }
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while deleting Category: {ex.Message}");
            }
        }
        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        /// <remarks>Retrieves a category based on the provided ID.</remarks>
        /// <param name="id">Category Id</param>
        /// <param name="categoryRepository"> Repository class for category</param>
        /// <param name="context"> HTTP context</param>
        /// <returns>Categories collection with details if exists or valid</returns>
        private static async Task<IResult> GetCategoryById(
            string id,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to get a category by ID
            try
            {
                var category = await categoryRepository.GetCategoryById(id);
                if (category == null)
                {
                    return Results.NotFound($"Category with ID {id} not found.");
                }
                return Results.Ok(new
                {
                    id = category.Id,
                    name = category.Name,
                    color = category.Color
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Category: {ex.Message}");
            }
        }
        /// <summary>
        /// Gets categories by user ID.
        /// </summary>
        /// <remarks>Retrieves all categories associated with the authenticated user.</remarks>
        /// <param name="categoryRepository">Category Repository class implementing interface </param>
        /// <param name="context">HTTP context</param>
        /// <returns> categories for an user by context</returns>
        private static async Task<IResult> GetCategoriesByUser(
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to get categories by user
            try
            {
                // Check user 
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                ICollection<Category> categories = await categoryRepository.GetCategoriesByUser(userId);
                // Return response
                return Results.Ok(categories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    color = c.Color
                }));
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Categories: {ex.Message}");
            }
        }
    }
}
